using UnityEngine;

/// <summary>
/// Scales two building groups in opposite directions.
/// At the low HP mark the first group is large and the second is small.
/// At the high HP mark those sizes swap.
/// </summary>
public class PollutionOppositeScale : MonoBehaviour
{
    [SerializeField] private PollutionManager pollution;

    [Tooltip("HP가 낮을 때 크고, HP가 높을 때 작아지는 건물.")]
    [SerializeField] private Transform[] largeWhenHpIsLow;

    [Tooltip("HP가 높을 때 크고, HP가 낮을 때 작아지는 건물.")]
    [SerializeField] private Transform[] largeWhenHpIsHigh;

    [SerializeField] private int lowHp = 30;
    [SerializeField] private int highHp = 70;

    [Tooltip("작아졌을 때 원래 크기의 비율.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float smallScale = 0.35f;

    [SerializeField] private float leadSmoothTime = 0.4f;
    [SerializeField] private float trailSmoothTime = 1.05f;

    private Entry[] _entries;
    private float _weight;

    private struct Entry
    {
        public Transform Transform;
        public Vector3 BaseScale;
        public bool LargeWhenLow;
        public float Shown;
        public float Velocity;
        public int Order;
    }

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
        _weight = WeightFor(pollution.CurrentValue);
    }

    private void OnEnable()
    {
        CacheEntries();
        if (pollution == null)
            return;

        pollution.ValueChanged += OnValueChanged;
        _weight = WeightFor(pollution.CurrentValue);
        Snap();
    }

    private void OnDisable()
    {
        if (pollution != null)
            pollution.ValueChanged -= OnValueChanged;

        Restore();
    }

    private void Update()
    {
        if (_entries == null)
            return;

        int count = _entries.Length;
        for (int i = 0; i < count; i++)
        {
            Transform target = _entries[i].Transform;
            if (target == null)
                continue;

            float goal = ScaleFor(_entries[i].LargeWhenLow, _weight);
            float smooth = SmoothTimeFor(_entries[i].Order, count);
            _entries[i].Shown = Mathf.SmoothDamp(_entries[i].Shown, goal, ref _entries[i].Velocity, smooth);
            target.localScale = _entries[i].BaseScale * _entries[i].Shown;
        }
    }

    private void OnValueChanged(int value)
    {
        _weight = WeightFor(value);
    }

    private float WeightFor(int value)
    {
        if (highHp <= lowHp)
            return value >= highHp ? 1f : 0f;

        return Mathf.InverseLerp(lowHp, highHp, value);
    }

    private float ScaleFor(bool largeWhenLow, float weight)
    {
        float largeGroup = largeWhenLow ? 1f - weight : weight;
        return Mathf.Lerp(smallScale, 1f, largeGroup);
    }

    private float SmoothTimeFor(int order, int count)
    {
        if (count <= 1)
            return leadSmoothTime;

        return Mathf.Lerp(leadSmoothTime, trailSmoothTime, order / (float)(count - 1));
    }

    private void CacheEntries()
    {
        int lowCount = largeWhenHpIsLow != null ? largeWhenHpIsLow.Length : 0;
        int highCount = largeWhenHpIsHigh != null ? largeWhenHpIsHigh.Length : 0;
        _entries = new Entry[lowCount + highCount];
        int index = 0;
        index = AddGroup(largeWhenHpIsLow, true, index);
        AddGroup(largeWhenHpIsHigh, false, index);
    }

    private int AddGroup(Transform[] group, bool largeWhenLow, int index)
    {
        if (group == null)
            return index;

        for (int i = 0; i < group.Length; i++)
        {
            Transform item = group[i];
            _entries[index] = new Entry
            {
                Transform = item,
                BaseScale = item != null ? item.localScale : Vector3.one,
                LargeWhenLow = largeWhenLow,
                Order = index
            };
            index++;
        }

        return index;
    }

    private void Snap()
    {
        for (int i = 0; i < _entries.Length; i++)
        {
            _entries[i].Shown = ScaleFor(_entries[i].LargeWhenLow, _weight);
            _entries[i].Velocity = 0f;
            if (_entries[i].Transform != null)
                _entries[i].Transform.localScale = _entries[i].BaseScale * _entries[i].Shown;
        }
    }

    private void Restore()
    {
        if (_entries == null)
            return;

        for (int i = 0; i < _entries.Length; i++)
        {
            if (_entries[i].Transform == null)
                continue;

            _entries[i].Transform.localScale = _entries[i].BaseScale;
        }
    }
}
