using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows PollutionManager's value as a filled gauge.
/// </summary>
public class PollutionGaugeView : MonoBehaviour
{
    [SerializeField] private PollutionManager pollution;
    [SerializeField] private Image fill;

    private void OnEnable()
    {
        if (pollution == null)
            pollution = FindFirstObjectByType<PollutionManager>();

        if (pollution == null || fill == null)
            return;

        pollution.ValueChanged += Refresh;
        Refresh(pollution.CurrentValue);
    }

    private void OnDisable()
    {
        if (pollution == null)
            return;

        pollution.ValueChanged -= Refresh;
    }

    private void Refresh(int value)
    {
        if (fill == null)
            return;

        fill.fillAmount = (float)value / PollutionManager.MaxValue;
    }
}
