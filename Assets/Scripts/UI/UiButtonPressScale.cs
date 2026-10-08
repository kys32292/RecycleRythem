using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Grows a UI button while the pointer is over it, and a little more while it is held.
/// </summary>
public class UiButtonPressScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("마우스를 올렸을 때의 배율. 1이면 씬에 배치한 크기입니다.")]
    [Min(0.01f)]
    [SerializeField] private float hoverScale = 1.12f;

    [Tooltip("누르고 있는 동안의 배율.")]
    [Min(0.01f)]
    [SerializeField] private float pressedScale = 1.18f;

    [Min(0.01f)]
    [SerializeField] private float duration = 0.16f;

    private Vector3 _restScale;
    private bool _restCaptured;
    private bool _hovered;
    private bool _pressed;
    private Tween _tween;

    private void Awake()
    {
        _restScale = transform.localScale;
        _restCaptured = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        if (!_pressed)
            Play(_restScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsPointerStillOverButton(eventData))
            return;

        _hovered = false;
        _pressed = false;
        Play(_restScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        Play(_restScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        Play(_hovered ? _restScale * hoverScale : _restScale);
    }

    private void OnDisable()
    {
        _tween?.Kill();
        _hovered = false;
        _pressed = false;
        if (_restCaptured)
            transform.localScale = _restScale;
    }

    private bool IsPointerStillOverButton(PointerEventData eventData)
    {
        var current = eventData.pointerCurrentRaycast.gameObject;
        if (current == null)
            return false;

        var currentTransform = current.transform;
        return currentTransform == transform || currentTransform.IsChildOf(transform);
    }

    private void Play(Vector3 scale)
    {
        _tween?.Kill();
        _tween = transform
            .DOScale(scale, duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(gameObject);
    }
}
