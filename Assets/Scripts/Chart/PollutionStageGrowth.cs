using UnityEngine;

/// <summary>
/// Swaps scene-placed stage-1 objects for stage 2, 3, and 4 prefabs as the pollution value rises.
/// Below the stage-2 threshold the placed objects stay visible.
/// </summary>
public class PollutionStageGrowth : MonoBehaviour
{
    [SerializeField] private PollutionManager pollution;

    [Header("1단계")]
    [Tooltip("씬에 배치해 둔 1단계 오브젝트. 여러 개 넣을 수 있습니다.")]
    [SerializeField] private GameObject[] stageOneObjects;

    [Header("프리팹")]
    [SerializeField] private GameObject stageTwoPrefab;
    [SerializeField] private GameObject stageThreePrefab;
    [SerializeField] private GameObject stageFourPrefab;

    [Header("기준 수치")]
    [SerializeField] private int stageTwoAt = 70;
    [SerializeField] private int stageThreeAt = 80;
    [SerializeField] private int stageFourAt = 90;

    private GameObject[] _spawned = System.Array.Empty<GameObject>();
    private int _stage = -1;

    private void Awake()
    {
        if (pollution == null)
            pollution = FindFirstObjectByType<PollutionManager>();
    }

    private void OnEnable()
    {
        if (pollution == null)
            return;

        pollution.ValueChanged += Apply;
        Apply(pollution.CurrentValue);
    }

    private void OnDisable()
    {
        if (pollution == null)
            return;

        pollution.ValueChanged -= Apply;
    }

    private void OnDestroy()
    {
        ClearSpawns();
    }

    private void Apply(int value)
    {
        int stage = StageFor(value);
        if (stage == _stage)
            return;

        _stage = stage;
        EnsureSpawnSlots();

        GameObject prefab = PrefabFor(stage);
        for (int i = 0; i < stageOneObjects.Length; i++)
        {
            GameObject placed = stageOneObjects[i];
            if (placed == null)
                continue;

            ClearSpawn(i);

            if (stage == 1 || prefab == null)
            {
                placed.SetActive(true);
                continue;
            }

            placed.SetActive(false);
            _spawned[i] = Instantiate(prefab, placed.transform.position, placed.transform.rotation, placed.transform.parent);
        }
    }

    private int StageFor(int value)
    {
        if (value >= stageFourAt)
            return 4;
        if (value >= stageThreeAt)
            return 3;
        if (value >= stageTwoAt)
            return 2;
        return 1;
    }

    private GameObject PrefabFor(int stage)
    {
        switch (stage)
        {
            case 2:
                return stageTwoPrefab;
            case 3:
                return stageThreePrefab;
            case 4:
                return stageFourPrefab;
            default:
                return null;
        }
    }

    private void EnsureSpawnSlots()
    {
        int count = stageOneObjects != null ? stageOneObjects.Length : 0;
        if (_spawned.Length == count)
            return;

        ClearSpawns();
        _spawned = new GameObject[count];
    }

    private void ClearSpawns()
    {
        for (int i = 0; i < _spawned.Length; i++)
            ClearSpawn(i);
    }

    private void ClearSpawn(int index)
    {
        if (_spawned[index] == null)
            return;

        Destroy(_spawned[index]);
        _spawned[index] = null;
    }
}
