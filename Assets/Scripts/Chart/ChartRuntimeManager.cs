using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AudioSource))]
public class ChartRuntimeManager : MonoBehaviour
{
    private enum JudgeResult
    {
        None,
        Great,
        Good,
        Fair,
        Miss
    }

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TextAsset chartJsonAsset;
    [SerializeField] private Transform upperSpawnPoint;
    [SerializeField] private Transform lowerSpawnPoint;
    [SerializeField] private Transform upperJudgePoint;
    [SerializeField] private Transform lowerJudgePoint;
    [SerializeField] private Transform noteRoot;

    [Header("Chart")]
    [SerializeField] private string chartFileName = "new_chart";
    [SerializeField] private string chartFolder = "ChartData";
    [SerializeField] private bool playOnStart = true;

    [Header("Visuals")]
    [SerializeField] private GameObject upperNotePrefab;
    [SerializeField] private GameObject lowerNotePrefab;
    [SerializeField] private Vector3 fallbackNoteScale = new Vector3(0.7f, 0.7f, 0.7f);
    [SerializeField] private float travelTime = 2f;
    [SerializeField] private float cleanupDelayAfterJudge = 0.15f;

    [Header("Judge Windows")]
    [SerializeField] private float greatWindow = 0.05f;
    [SerializeField] private float goodWindow = 0.1f;
    [SerializeField] private float fairWindow = 0.18f;

    public event Action NoteHit;
    public event Action NoteMiss;

    private readonly List<RuntimeChartNote> chartNotes = new List<RuntimeChartNote>();
    private int nextSpawnIndex;
    private bool isLoaded;

    private void Reset()
    {
        EnsureReferences();
    }

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.JumpPressed += HandleUpperInput;
            inputManager.AttackPressed += HandleLowerInput;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.JumpPressed -= HandleUpperInput;
            inputManager.AttackPressed -= HandleLowerInput;
        }
    }

    private void Start()
    {
        LoadChart();

        if (playOnStart)
        {
            StartPlayback();
        }
    }

    private void Update()
    {
        if (!isLoaded || audioSource == null || !audioSource.isPlaying)
        {
            return;
        }

        SpawnPendingNotes();
        UpdateActiveNotes();
        MissExpiredNotes();
    }

    [ContextMenu("Load Chart")]
    public void LoadChart()
    {
        ChartFileData loadedChart = TryLoadChart();

        if (loadedChart == null)
        {
            Debug.LogWarning("[ChartRuntime] Failed to load chart.");
            return;
        }

        ClearRuntimeNotes();
        chartNotes.Clear();

        List<ChartNoteData> loadedNotes = loadedChart.notes ?? new List<ChartNoteData>();
        loadedNotes.Sort((a, b) => a.time.CompareTo(b.time));

        for (int i = 0; i < loadedNotes.Count; i++)
        {
            chartNotes.Add(new RuntimeChartNote(loadedNotes[i]));
        }

        nextSpawnIndex = 0;
        isLoaded = true;

        Debug.Log(string.Format("[ChartRuntime] Loaded chart '{0}' with {1} notes.", loadedChart.songName, chartNotes.Count));
    }

    [ContextMenu("Start Playback")]
    public void StartPlayback()
    {
        if (!isLoaded)
        {
            LoadChart();
        }

        if (!ValidateRuntimeSetup())
        {
            return;
        }

        ResetRuntimeState();
        audioSource.Play();
        Debug.Log("[ChartRuntime] Playback started.");
    }

    [ContextMenu("Stop Playback")]
    public void StopPlayback()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }

        ResetRuntimeState();
        Debug.Log("[ChartRuntime] Playback stopped.");
    }

    [ContextMenu("Judge Upper")]
    public void JudgeUpper()
    {
        HandleLaneJudge(LaneType.Upper);
    }

    [ContextMenu("Judge Lower")]
    public void JudgeLower()
    {
        HandleLaneJudge(LaneType.Lower);
    }

    private void HandleUpperInput()
    {
        HandleLaneJudge(LaneType.Upper);
    }

    private void HandleLowerInput()
    {
        HandleLaneJudge(LaneType.Lower);
    }

    private void HandleLaneJudge(LaneType lane)
    {
        if (!isLoaded || audioSource == null)
        {
            return;
        }

        RuntimeChartNote candidate = FindBestCandidate(lane, false);

        if (candidate == null)
        {
            Debug.Log(string.Format("[ChartRuntime] {0} input -> no candidate", lane));
            return;
        }

        float error = Mathf.Abs(audioSource.time - candidate.NoteData.time);
        JudgeResult result = EvaluateJudge(error);

        if (result == JudgeResult.None || result == JudgeResult.Miss)
        {
            Debug.Log(string.Format("[ChartRuntime] {0} input -> outside fair window ({1:F3}s)", lane, error));
            return;
        }

        ConsumeNote(candidate, result);
        Debug.Log(string.Format("[ChartRuntime] {0} -> {1} ({2:F3}s)", lane, result, error));
    }

    private void SpawnPendingNotes()
    {
        float currentTime = audioSource.time;

        while (nextSpawnIndex < chartNotes.Count)
        {
            RuntimeChartNote runtimeNote = chartNotes[nextSpawnIndex];
            float spawnTime = runtimeNote.NoteData.time - travelTime;

            if (currentTime < spawnTime)
            {
                break;
            }

            SpawnRuntimeNote(runtimeNote);
            nextSpawnIndex++;
        }
    }

    private void SpawnRuntimeNote(RuntimeChartNote runtimeNote)
    {
        if (runtimeNote.IsSpawned)
        {
            return;
        }

        Transform spawnPoint = runtimeNote.NoteData.lane == LaneType.Upper ? upperSpawnPoint : lowerSpawnPoint;
        GameObject prefab = runtimeNote.NoteData.lane == LaneType.Upper ? upperNotePrefab : lowerNotePrefab;

        GameObject noteObject;

        if (prefab != null)
        {
            noteObject = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, noteRoot);
        }
        else
        {
            noteObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noteObject.transform.SetParent(noteRoot, false);
            noteObject.transform.position = spawnPoint.position;
            noteObject.transform.rotation = spawnPoint.rotation;
            noteObject.transform.localScale = fallbackNoteScale;

            Collider fallbackCollider = noteObject.GetComponent<Collider>();
            if (fallbackCollider != null)
            {
                Destroy(fallbackCollider);
            }

            Renderer renderer = noteObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = runtimeNote.NoteData.lane == LaneType.Upper ? Color.cyan : Color.magenta;
            }
        }

        noteObject.name = string.Format("ChartNote_{0}_{1:F3}", runtimeNote.NoteData.lane, runtimeNote.NoteData.time);

        runtimeNote.GameObject = noteObject;
        runtimeNote.IsSpawned = true;
    }

    private void UpdateActiveNotes()
    {
        float currentTime = audioSource.time;

        for (int i = 0; i < chartNotes.Count; i++)
        {
            RuntimeChartNote runtimeNote = chartNotes[i];

            if (!runtimeNote.IsSpawned || runtimeNote.IsConsumed || runtimeNote.GameObject == null)
            {
                continue;
            }

            Transform spawnPoint = runtimeNote.NoteData.lane == LaneType.Upper ? upperSpawnPoint : lowerSpawnPoint;
            Transform judgePoint = runtimeNote.NoteData.lane == LaneType.Upper ? upperJudgePoint : lowerJudgePoint;

            float spawnTime = runtimeNote.NoteData.time - travelTime;
            float normalized = travelTime <= 0f
                ? 1f
                : Mathf.InverseLerp(spawnTime, runtimeNote.NoteData.time, currentTime);

            runtimeNote.GameObject.transform.position = Vector3.Lerp(
                spawnPoint.position,
                judgePoint.position,
                normalized
            );
        }
    }

    private void MissExpiredNotes()
    {
        float currentTime = audioSource.time;

        for (int i = 0; i < chartNotes.Count; i++)
        {
            RuntimeChartNote runtimeNote = chartNotes[i];

            if (runtimeNote.IsConsumed || currentTime <= runtimeNote.NoteData.time + fairWindow)
            {
                continue;
            }

            ConsumeNote(runtimeNote, JudgeResult.Miss);
            Debug.Log(string.Format("[ChartRuntime] {0} -> Miss", runtimeNote.NoteData.lane));
        }
    }

    private RuntimeChartNote FindBestCandidate(LaneType lane, bool includeExpired)
    {
        RuntimeChartNote best = null;
        float bestError = float.MaxValue;
        float currentTime = audioSource.time;

        for (int i = 0; i < chartNotes.Count; i++)
        {
            RuntimeChartNote runtimeNote = chartNotes[i];

            if (runtimeNote.NoteData.lane != lane || runtimeNote.IsConsumed || !runtimeNote.IsSpawned)
            {
                continue;
            }

            if (!includeExpired && currentTime > runtimeNote.NoteData.time + fairWindow)
            {
                continue;
            }

            float error = Mathf.Abs(currentTime - runtimeNote.NoteData.time);

            if (error < bestError)
            {
                bestError = error;
                best = runtimeNote;
            }
        }

        return best;
    }

    private JudgeResult EvaluateJudge(float error)
    {
        if (error <= greatWindow)
        {
            return JudgeResult.Great;
        }

        if (error <= goodWindow)
        {
            return JudgeResult.Good;
        }

        if (error <= fairWindow)
        {
            return JudgeResult.Fair;
        }

        return JudgeResult.None;
    }

    private void ConsumeNote(RuntimeChartNote runtimeNote, JudgeResult result)
    {
        if (runtimeNote.IsConsumed)
        {
            return;
        }

        runtimeNote.IsConsumed = true;
        runtimeNote.Result = result;

        if (result == JudgeResult.Miss)
            NoteMiss?.Invoke();
        else
            NoteHit?.Invoke();

        if (runtimeNote.GameObject != null)
        {
            Destroy(runtimeNote.GameObject, cleanupDelayAfterJudge);
            runtimeNote.GameObject = null;
        }
    }

    private ChartFileData TryLoadChart()
    {
        if (chartJsonAsset != null)
        {
            return JsonUtility.FromJson<ChartFileData>(chartJsonAsset.text);
        }

        string filePath = Path.Combine(Application.dataPath, chartFolder, chartFileName + ".json");

        if (!File.Exists(filePath))
        {
            return null;
        }

        return JsonUtility.FromJson<ChartFileData>(File.ReadAllText(filePath));
    }

    private void EnsureReferences()
    {
        if (inputManager == null)
        {
            inputManager = FindFirstObjectByType<InputManager>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (noteRoot == null)
        {
            noteRoot = transform;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private bool ValidateRuntimeSetup()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogWarning("[ChartRuntime] AudioSource clip is missing.");
            return false;
        }

        if (upperSpawnPoint == null || lowerSpawnPoint == null || upperJudgePoint == null || lowerJudgePoint == null)
        {
            Debug.LogWarning("[ChartRuntime] Spawn/Judge point references are missing.");
            return false;
        }

        return true;
    }

    private void ResetRuntimeState()
    {
        ClearRuntimeNotes();

        for (int i = 0; i < chartNotes.Count; i++)
        {
            chartNotes[i].ResetRuntimeState();
        }

        nextSpawnIndex = 0;
        if (audioSource != null)
        {
            audioSource.time = 0f;
        }
    }

    private void ClearRuntimeNotes()
    {
        for (int i = 0; i < chartNotes.Count; i++)
        {
            if (chartNotes[i].GameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(chartNotes[i].GameObject);
                }
                else
                {
                    DestroyImmediate(chartNotes[i].GameObject);
                }

                chartNotes[i].GameObject = null;
            }
        }
    }

    [Serializable]
    private sealed class RuntimeChartNote
    {
        public RuntimeChartNote(ChartNoteData noteData)
        {
            NoteData = noteData;
        }

        public ChartNoteData NoteData { get; private set; }
        public GameObject GameObject { get; set; }
        public bool IsSpawned { get; set; }
        public bool IsConsumed { get; set; }
        public JudgeResult Result { get; set; }

        public void ResetRuntimeState()
        {
            GameObject = null;
            IsSpawned = false;
            IsConsumed = false;
            Result = JudgeResult.None;
        }
    }
}
