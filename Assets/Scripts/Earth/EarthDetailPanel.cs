using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Screen-space rectangle that slides in from one side and, on Escape, leaves toward the opposite side.
/// </summary>
public class EarthDetailPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image detailImage;
    [SerializeField] private Color placeholderColor = new Color(0.96f, 0.94f, 0.88f, 1f);
    [Min(0.05f)]
    [SerializeField] private float duration = 0.45f;
    [Min(0f)]
    [SerializeField] private float offscreenDistance = 1600f;

    private RectTransform _rect;
    private Tween _tween;
    private SlideDirection _enterFrom;
    private string _sceneName;
    private Sprite _pendingSprite;
    private SlideDirection _pendingEnter;
    private string _pendingSceneName;
    private bool _visible;
    private bool _hiding;
    private bool _reopenAfterHide;

    private void Awake()
    {
        _rect = (RectTransform)transform;
    }

    private void Update()
    {
        if (!_visible || _hiding)
            return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            Hide();
    }

    public void Show(Sprite sprite, SlideDirection enterFrom, string sceneName)
    {
        _pendingSprite = sprite;
        _pendingEnter = enterFrom;
        _pendingSceneName = sceneName;
        CloseOthers();

        if (_visible)
        {
            _reopenAfterHide = true;
            if (!_hiding)
                PlayHide();
            return;
        }

        PlayShow(sprite, enterFrom, sceneName);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_visible || _hiding || string.IsNullOrEmpty(_sceneName))
            return;

        SceneManager.LoadScene(_sceneName);
    }

    public void HideImmediate()
    {
        _tween?.Kill();
        _tween = null;
        _visible = false;
        _hiding = false;
        _reopenAfterHide = false;
        gameObject.SetActive(false);
    }

    private void CloseOthers()
    {
        var panels = FindObjectsByType<EarthDetailPanel>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (var i = 0; i < panels.Length; i++)
        {
            if (panels[i] != this)
                panels[i].HideImmediate();
        }
    }

    public void Hide()
    {
        _reopenAfterHide = false;
        if (!_visible || _hiding)
            return;

        PlayHide();
    }

    private void PlayShow(Sprite sprite, SlideDirection enterFrom, string sceneName)
    {
        if (detailImage != null && sprite != null)
        {
            detailImage.sprite = sprite;
            detailImage.color = Color.white;
        }

        _enterFrom = enterFrom;
        _sceneName = sceneName;
        gameObject.SetActive(true);
        _rect.anchoredPosition = Offset(enterFrom);
        _visible = true;
        _hiding = false;

        _tween?.Kill();
        _tween = _rect
            .DOAnchorPos(Vector2.zero, duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => _tween = null);
    }

    private void PlayHide()
    {
        _hiding = true;
        var exitOffset = Offset(Opposite(_enterFrom));

        _tween?.Kill();
        _tween = _rect
            .DOAnchorPos(exitOffset, duration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _tween = null;
                _hiding = false;
                _visible = false;

                if (_reopenAfterHide)
                {
                    _reopenAfterHide = false;
                    PlayShow(_pendingSprite, _pendingEnter, _pendingSceneName);
                    return;
                }

                gameObject.SetActive(false);
            });
    }

    private Vector2 Offset(SlideDirection direction)
    {
        switch (direction)
        {
            case SlideDirection.Left:
                return new Vector2(-offscreenDistance, 0f);
            case SlideDirection.Right:
                return new Vector2(offscreenDistance, 0f);
            case SlideDirection.Up:
                return new Vector2(0f, offscreenDistance);
            default:
                return new Vector2(0f, -offscreenDistance);
        }
    }

    private static SlideDirection Opposite(SlideDirection direction)
    {
        switch (direction)
        {
            case SlideDirection.Left:
                return SlideDirection.Right;
            case SlideDirection.Right:
                return SlideDirection.Left;
            case SlideDirection.Up:
                return SlideDirection.Down;
            default:
                return SlideDirection.Up;
        }
    }
}
