using UnityEngine;

/// <summary>
/// Shows fewer factories as HP rises.
/// 100 hides all of them, 70 shows one, 50 shows two, 30 shows three, and 10 or below shows four.
/// </summary>
public class PollutionFactoryVisibility : MonoBehaviour
{
    [SerializeField] private PollutionManager pollution;

    [Tooltip("앞에 있는 공장부터 남습니다. HP 70에는 첫 공장만 보입니다.")]
    [SerializeField] private GameObject[] factories;

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
            pollution.ValueChanged -= Apply;

        pollution = manager;
        if (!isActiveAndEnabled || pollution == null)
            return;

        pollution.ValueChanged += Apply;
        Apply(pollution.CurrentValue);
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
        if (pollution != null)
            pollution.ValueChanged -= Apply;

        SetVisible(factories != null ? factories.Length : 0);
    }

    private void Apply(int hp)
    {
        SetVisible(CountFor(hp));
    }

    private static int CountFor(int hp)
    {
        if (hp >= 100)
            return 0;
        if (hp >= 70)
            return 1;
        if (hp >= 50)
            return 2;
        if (hp >= 30)
            return 3;
        return 4;
    }

    private void SetVisible(int count)
    {
        if (factories == null)
            return;

        for (int i = 0; i < factories.Length; i++)
        {
            if (factories[i] != null)
                factories[i].SetActive(i < count);
        }
    }
}
