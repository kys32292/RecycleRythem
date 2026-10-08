using DG.Tweening;
using UnityEngine;

/// <summary>
/// One title image that stays small, then pops in after its own delay:
/// small, overshoot large, then settle to the medium size set on this RectTransform.
/// </summary>
public class TitleIntroImage : MonoBehaviour
{
    [Header("타이밍")]
    [Tooltip("게임 시작 후 이 이미지가 나타나기까지 기다리는 시간(초).")]
    [Min(0f)]
    [SerializeField] private float appearDelay = 2f;

    [Tooltip("켜 두면 기다리는 동안 오브젝트를 꺼 두었다가, 시간이 지난 뒤 켜고 커지게 합니다.")]
    [SerializeField] private bool startInactive = true;

    [Header("크기")]
    [Tooltip("시작할 때 배율. 씬에 배치한 크기가 1입니다.")]
    [Min(0f)]
    [SerializeField] private float startScale = 0.2f;

    [Tooltip("한 번 커질 때의 배율. 중간 크기보다 크게 둡니다.")]
    [Min(0f)]
    [SerializeField] private float peakScale = 1.22f;

    [Tooltip("마지막으로 자리 잡는 중간 크기 배율. 1이면 씬에 배치한 크기입니다.")]
    [Min(0f)]
    [SerializeField] private float settleScale = 1f;

    [Header("재생")]
    [Min(0.01f)]
    [SerializeField] private float growDuration = 0.32f;

    [Min(0.01f)]
    [SerializeField] private float settleDuration = 0.18f;

    private Vector3 _authoredScale;
    private bool _activateAfterDelay;

    private void Awake()
    {
        _authoredScale = transform.localScale;
        transform.localScale = _authoredScale * startScale;

        if (!startInactive)
            return;

        _activateAfterDelay = true;
        DOVirtual.DelayedCall(appearDelay, Activate, false).SetUpdate(true);
        gameObject.SetActive(false);
    }

    private void Activate()
    {
        if (this == null)
            return;

        gameObject.SetActive(true);
    }

    private void Start()
    {
        var peak = _authoredScale * peakScale;
        var settle = _authoredScale * settleScale;
        var delay = _activateAfterDelay ? 0f : appearDelay;

        var sequence = DOTween.Sequence();
        sequence.SetUpdate(true);
        sequence.SetLink(gameObject);
        if (delay > 0f)
            sequence.AppendInterval(delay);
        sequence.Append(transform.DOScale(peak, growDuration).SetEase(Ease.OutCubic));
        sequence.Append(transform.DOScale(settle, settleDuration).SetEase(Ease.OutQuad));
    }
}
