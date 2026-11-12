using UnityEngine;

/// <summary>
/// Lowers sea objects as HP rises and raises them as HP falls.
/// HP 50 keeps the height they were placed at.
/// Objects later in the list follow more slowly, so the surface does not jump together.
/// </summary>
public class PollutionSeaLevel : MonoBehaviour
{
    [SerializeField] private PollutionManager pollution;

    [Tooltip("수면처럼 움직일 오브젝트. 목록 위쪽부터 먼저 반응합니다.")]
    [SerializeField] private Transform[] objects;

    [Tooltip("HP 100일 때, 놓아 둔 높이보다 얼마나 내려갈지.")]
    [SerializeField] private float dropAtZero = 3f;

    [Tooltip("HP 0일 때, 놓아 둔 높이보다 얼마나 올라갈지.")]
    [SerializeField] private float riseAtFull = 3f;

    [Tooltip("가장 빠른 오브젝트가 목표 높이에 붙는 시간.")]
    [SerializeField] private float leadSmoothTime = 0.35f;

    [Tooltip("가장 느린 오브젝트가 목표 높이에 붙는 시간. 길수록 뒤늦게 따라옵니다.")]
    [SerializeField] private float trailSmoothTime = 1.15f;

    [Tooltip("높이가 바뀔 때 남는 물결 크기.")]
    [SerializeField] private float ripple = 0.35f;

    private Vector3[] _basePositions;
    private float[] _offset;
    private float[] _velocity;
    private float[] _phase;
    private float _target;
    private float _ripple;

    private void Awake()
    {
        if (pollution == null)
            pollution = FindFirstObjectByType<PollutionManager>();
    }

    public void Use(PollutionManager manager)
    {
        if (pollution == manager)
            return;

        if (isActiveAndEnabled && pollution != null)
            pollution.ValueChanged -= OnValueChanged;

        pollution = manager;
        if (!isActiveAndEnabled || pollution == null)
            return;

        pollution.ValueChanged += OnValueChanged;
        _target = HeightFor(pollution.CurrentValue);
    }

    public void ShiftBases(Vector3 delta)
    {
        if (_basePositions == null)
            return;

        for (int i = 0; i < _basePositions.Length; i++)
            _basePositions[i] += delta;
    }

    private void OnEnable()
    {
        CacheBases();
        if (pollution == null)
            return;

        pollution.ValueChanged += OnValueChanged;
        _target = HeightFor(pollution.CurrentValue);
        SnapToTarget();
    }

    private void OnDisable()
    {
        if (pollution != null)
            pollution.ValueChanged -= OnValueChanged;

        RestoreBases();
    }

    private void Update()
    {
        if (_offset == null)
            return;

        _ripple = Mathf.MoveTowards(_ripple, 0f, Time.deltaTime * 0.65f);
        int count = _offset.Length;
        for (int i = 0; i < count; i++)
        {
            if (objects[i] == null)
                continue;

            float smooth = SmoothTimeFor(i, count);
            _offset[i] = Mathf.SmoothDamp(_offset[i], _target, ref _velocity[i], smooth);
            float wave = Mathf.Sin(Time.time * 2.4f + _phase[i]) * ripple * _ripple;
            Vector3 position = _basePositions[i];
            position.y += _offset[i] + wave;
            objects[i].position = position;
        }
    }

    private void OnValueChanged(int value)
    {
        float next = HeightFor(value);
        if (!Mathf.Approximately(next, _target))
            _ripple = 1f;
        _target = next;
    }

    private float HeightFor(int value)
    {
        float t = Mathf.Clamp01(value / (float)PollutionManager.MaxValue);
        return Mathf.Lerp(riseAtFull, -dropAtZero, t);
    }

    private float SmoothTimeFor(int index, int count)
    {
        if (count <= 1)
            return leadSmoothTime;

        float t = index / (float)(count - 1);
        return Mathf.Lerp(leadSmoothTime, trailSmoothTime, t);
    }

    private void CacheBases()
    {
        int count = objects != null ? objects.Length : 0;
        _basePositions = new Vector3[count];
        _offset = new float[count];
        _velocity = new float[count];
        _phase = new float[count];
        for (int i = 0; i < count; i++)
        {
            _phase[i] = i * 0.9f;
            if (objects[i] == null)
                continue;

            _basePositions[i] = objects[i].position;
        }
    }

    private void SnapToTarget()
    {
        for (int i = 0; i < _offset.Length; i++)
        {
            _offset[i] = _target;
            _velocity[i] = 0f;
        }
    }

    private void RestoreBases()
    {
        if (objects == null || _basePositions == null)
            return;

        int count = Mathf.Min(objects.Length, _basePositions.Length);
        for (int i = 0; i < count; i++)
        {
            if (objects[i] == null)
                continue;

            objects[i].position = _basePositions[i];
        }
    }
}
