using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

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
    // 두 레인을 하나로 합쳤습니다. 이전 씬의 위쪽 지점 연결은 이름을 바꿔도 유지합니다.
    [FormerlySerializedAs("upperSpawnPoint")]
    [FormerlySerializedAs("SpawnPoint")]
    [SerializeField] private Transform spawnPoint;
    [FormerlySerializedAs("upperJudgePoint")]
    [FormerlySerializedAs("JudgePoint")]
    [SerializeField] private Transform judgePoint;
    [SerializeField] private Transform noteRoot;

    [Header("Chart")]
    [SerializeField] private string chartFileName = "new_chart";
    [SerializeField] private string chartFolder = "ChartData";
    [SerializeField] private bool playOnStart = true;

    [Header("Visuals")]
    [FormerlySerializedAs("upperNotePrefab")]
    [SerializeField] private GameObject notePrefab;

    [SerializeField] private Vector3 fallbackNoteScale = new Vector3(0.7f, 0.7f, 0.7f);
    [SerializeField] private float travelTime = 2f;
    [SerializeField] private float cleanupDelayAfterJudge = 0.15f;

    [Header("Judge Windows")]
    [SerializeField] private float greatWindow = 0.05f;
    [SerializeField] private float goodWindow = 0.1f;
    [SerializeField] private float fairWindow = 0.18f;

    public event Action NoteHit;
    public event Action NoteMiss;
    // 수치 컴포넌트가 재시작과 곡 종료를 알 수 있게 합니다. 수동 정지는 종료 판정하지 않습니다.
    public event Action PlaybackStarted;
    public event Action PlaybackCompleted;

    public float LastNoteTime { get; private set; }
    public float PlaybackTime => audioSource != null ? audioSource.time : 0f;
    public bool IsPlaybackActive => isPlaybackActive;

    private readonly List<RuntimeChartNote> chartNotes = new List<RuntimeChartNote>();
    private int nextSpawnIndex;
    private bool isLoaded;
    private bool isPlaybackActive;

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
            inputManager.Pressed += HandlePressed;
            // 일반 노트는 누를 때만 판정합니다. 떼는 순간까지 판정하면 한 번의 조작으로 두 노트를 칠 수 있습니다.
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.Pressed -= HandlePressed;
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
        if (!isLoaded || audioSource == null || !isPlaybackActive)
        {
            return;
        }

        // 기존 코드는 음악이 끝나면 바로 return해서 결과를 계산할 기회가 없었습니다.
        if (!audioSource.isPlaying)
        {
            CompletePlayback();
            return;
        }

        SpawnPendingNotes();
        UpdateActiveNotes();
        MissExpiredNotes();
    }

    [ContextMenu("Load Chart")]
    public void LoadChart()
    {
        // 재생 중 채보를 바꾸면 이전 곡의 종료/노트 이벤트가 섞이지 않도록 먼저 정지합니다.
        if (isPlaybackActive)
        {
            StopPlayback();
        }

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
        LastNoteTime = chartNotes.Count > 0 ? chartNotes[chartNotes.Count - 1].NoteData.time : 0f;

        Debug.Log(string.Format("[ChartRuntime] Loaded chart '{0}' with {1} notes.", loadedChart.songName, chartNotes.Count));
    }

    [ContextMenu("Start Playback")]
    public void StartPlayback()
    {
        if (!isLoaded)
        {
            LoadChart();
        }

        if (!isLoaded || !ValidateRuntimeSetup())
        {
            return;
        }

        ResetRuntimeState();
        PlaybackStarted?.Invoke(); // 매번 새 시도는 수치 50부터 시작합니다.
        audioSource.Play();
        isPlaybackActive = true;
        Debug.Log("[ChartRuntime] Playback started.");
    }

    [ContextMenu("Stop Playback")]
    public void StopPlayback()
    {
        isPlaybackActive = false; // 사용자가 중단한 곡에는 클리어/실패 판정을 내리지 않습니다.
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }

        ResetRuntimeState();
        Debug.Log("[ChartRuntime] Playback stopped.");
    }
    [ContextMenu("Judge Note")]
    private void HandlePressed()
    {
        // 정지 상태에서 입력해도 노트나 수치가 바뀌지 않게 재생 상태까지 확인합니다.
        if (!isLoaded || !isPlaybackActive || audioSource == null || !audioSource.isPlaying)
        {
            return;
        }

        // 입력 Update가 먼저 실행된 프레임에도, 현재 시각까지 등장할 노트는 판정할 수 있어야 합니다.
        SpawnPendingNotes();
        // 만료된 노트의 -20을 새 성공의 +5보다 먼저 처리해 프레임 실행 순서에 따른 수치 차이를 막습니다.
        MissExpiredNotes();
        RuntimeChartNote candidate = FindBestCandidate();

        if (candidate == null)
        {
            Debug.Log("[ChartRuntime] Input -> no candidate");
            return;
        }

        float error = Mathf.Abs(audioSource.time - candidate.NoteData.time);
        JudgeResult result = EvaluateJudge(error);

        if (result == JudgeResult.None || result == JudgeResult.Miss)
        {
            Debug.Log(string.Format("[ChartRuntime] Input -> outside fair window ({0:F3}s)", error));
            return;
        }

        ConsumeNote(candidate, result);
        Debug.Log(string.Format("[ChartRuntime] {0} ({1:F3}s)", result, error));
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

        // ChartNoteData에는 이제 시간만 있습니다. 모든 노트가 같은 지점과 프리팹을 사용합니다.
        GameObject prefab = notePrefab;

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
                renderer.material.color = Color.cyan;
            }
        }

        noteObject.name = string.Format("ChartNote_{0:F3}", runtimeNote.NoteData.time);

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
            Debug.Log(string.Format("[ChartRuntime] {0:F3}s -> Miss", runtimeNote.NoteData.time));
        }
    }

    private RuntimeChartNote FindBestCandidate()
    {
        RuntimeChartNote best = null;
        float bestError = float.MaxValue;
        float currentTime = audioSource.time;

        for (int i = 0; i < chartNotes.Count; i++)
        {
            RuntimeChartNote runtimeNote = chartNotes[i];

            // 레인 필터 대신 전체 노트 중 아직 처리하지 않은 가장 가까운 노트를 찾습니다.
            if (runtimeNote.IsConsumed || !runtimeNote.IsSpawned)
            {
                continue;
            }

            if (currentTime > runtimeNote.NoteData.time + fairWindow)
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

        // Fair도 실패로 처리합니다. 실패 이벤트를 공통으로 보내 오염도 -20과 실패 애니메이션을 적용합니다.
        // 위에서 이미 소비 처리했으므로 Fair 노트가 나중에 시간 초과로 다시 감점되지는 않습니다.
        if (result == JudgeResult.Fair || result == JudgeResult.Miss)
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
        // 기존 씬/프리팹도 별도 수작업 없이 수치 규칙이 작동하도록 같은 오브젝트에 붙입니다.
        if (GetComponent<PollutionManager>() == null)
        {
            gameObject.AddComponent<PollutionManager>();
        }

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

        if (spawnPoint == null || judgePoint == null)
        {
            Debug.LogWarning("[ChartRuntime] Spawn/Judge point references are missing.");
            return false;
        }

        return true;
    }

    private void CompletePlayback()
    {
        // 먼저 닫아 두면 다음 프레임이나 이벤트 처리 중 종료 판정이 중복되지 않습니다.
        isPlaybackActive = false;

        // 마지막 노트 직후 곡이 끝나면 fairWindow를 기다릴 수 없으므로 남은 노트는 Miss로 확정합니다.
        // 수치 반영이 모두 끝난 뒤에만 최종 결과를 알립니다. 노트별 IsConsumed가 중복 감점을 막습니다.
        foreach (RuntimeChartNote note in chartNotes)
        {
            if (!note.IsConsumed)
            {
                ConsumeNote(note, JudgeResult.Miss);
            }
        }

        PlaybackCompleted?.Invoke();
        Debug.Log("[ChartRuntime] Playback completed.");
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
