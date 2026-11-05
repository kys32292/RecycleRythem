using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fades a black overlay in after the loaded chart's last note, plus a delay.
/// </summary>
public class SongEndFade : MonoBehaviour
{
    [SerializeField] private ChartRuntimeManager chart;
    [SerializeField] private Image black;
    [SerializeField] private SongEndResultTexts resultTexts;

    [Tooltip("불러온 JSON의 마지막 노트가 지난 뒤, 어두워지기까지 기다리는 시간(초).")]
    [Min(0f)]
    [SerializeField] private float delayAfterLastNote = 5f;

    [Tooltip("검은 화면이 완전히 어두워지는 데 걸리는 시간(초).")]
    [Min(0.01f)]
    [SerializeField] private float fadeDuration = 1.5f;

    private Tween _tween;
    private bool _scheduled;

    private void Awake()
    {
        if (black == null)
            black = GetComponent<Image>();

        if (chart == null)
            chart = FindFirstObjectByType<ChartRuntimeManager>();

        if (resultTexts == null)
            resultTexts = FindFirstObjectByType<SongEndResultTexts>();

        SetAlpha(0f);
    }

    private void OnEnable()
    {
        if (chart == null)
            return;

        chart.PlaybackStarted += HandlePlaybackStarted;
        chart.PlaybackCompleted += HandlePlaybackCompleted;
    }

    private void OnDisable()
    {
        if (chart != null)
        {
            chart.PlaybackStarted -= HandlePlaybackStarted;
            chart.PlaybackCompleted -= HandlePlaybackCompleted;
        }

        _tween?.Kill();
    }

    private void Update()
    {
        if (_scheduled || chart == null || !chart.IsPlaybackActive)
            return;

        if (chart.PlaybackTime < chart.LastNoteTime)
            return;

        ScheduleFade();
    }

    private void HandlePlaybackStarted()
    {
        _scheduled = false;
        _tween?.Kill();
        SetAlpha(0f);
        if (resultTexts != null)
            resultTexts.HideAll();
    }

    private void HandlePlaybackCompleted()
    {
        if (_scheduled)
            return;

        ScheduleFade();
    }

    private void ScheduleFade()
    {
        _scheduled = true;
        _tween?.Kill();
        _tween = DOVirtual.DelayedCall(delayAfterLastNote, BeginFade, false).SetLink(gameObject);
    }

    private void BeginFade()
    {
        if (black == null)
            return;

        _tween = black.DOFade(1f, fadeDuration).SetLink(gameObject).OnComplete(ShowResult);
    }

    private void ShowResult()
    {
        if (resultTexts != null)
            resultTexts.Play();
    }

    private void SetAlpha(float alpha)
    {
        if (black == null)
            return;

        if (!black.enabled)
            black.enabled = true;

        Color color = black.color;
        color.a = alpha;
        black.color = color;
    }
}
