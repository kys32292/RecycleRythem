using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Shows the four-panel comic when Main테스트 is opened from the title.
/// The top-left panel drops in first. The other three stay black, then fall in
/// from the top-right, bottom-left, and bottom-right. Skip dismisses the story.
/// </summary>
public class FourCutIntro : MonoBehaviour
{
    public static bool ComingFromTitle;

    [SerializeField] private Sprite comic;
    [SerializeField] private TMP_FontAsset font;

    [Min(0.2f)]
    [SerializeField] private float gap = 5f;

    private readonly List<Piece> _pieces = new List<Piece>();
    private readonly List<Scrap> _scraps = new List<Scrap>();
    private RectTransform _frame;
    private CanvasGroup _group;
    private int _next;
    private float _wait;
    private bool _fading;

    private struct Piece
    {
        public RectTransform Rect;
        public Vector2 Velocity;
        public float Angle;
        public float Spin;
        public bool Active;
        public bool Landed;
        public bool Burst;
    }

    private struct Scrap
    {
        public RectTransform Rect;
        public Vector2 Velocity;
        public float Spin;
        public float Life;
    }

    private void Awake()
    {
        if (!ComingFromTitle || comic == null)
        {
            ComingFromTitle = false;
            Destroy(gameObject);
            return;
        }

        ComingFromTitle = false;
        Build();
        _wait = 0.15f;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (WasSkipPressed())
            BeginFade();

        if (_fading)
        {
            _group.alpha = Mathf.MoveTowards(_group.alpha, 0f, dt / 0.35f);
            if (_group.alpha <= 0.001f)
                Destroy(gameObject);
            return;
        }

        StepPieces(dt);
        StepScraps(dt);

        if (_next >= _pieces.Count)
        {
            if (AllLanded())
            {
                _wait -= dt;
                if (_wait <= 0f)
                    BeginFade();
            }

            return;
        }

        _wait -= dt;
        if (_wait > 0f)
            return;

        Piece piece = _pieces[_next];
        piece.Active = true;
        piece.Rect.gameObject.SetActive(true);
        GameFeedback.Current?.PlayImageArrival();
        _pieces[_next] = piece;
        _next++;
        _wait = gap;
    }

    private void StepPieces(float dt)
    {
        const float gravity = 3400f;
        for (int i = 0; i < _pieces.Count; i++)
        {
            Piece piece = _pieces[i];
            if (!piece.Active || piece.Landed)
                continue;

            piece.Velocity.y -= gravity * dt;
            Vector2 position = piece.Rect.anchoredPosition + piece.Velocity * dt;
            piece.Angle += piece.Spin * dt;

            if (position.y <= 0f)
            {
                position.y = 0f;
                piece.Velocity.y = -piece.Velocity.y * 0.34f;
                piece.Velocity.x *= 0.45f;
                piece.Spin *= -0.4f;
                if (!piece.Burst)
                {
                    piece.Burst = true;
                    Burst(piece.Rect);
                }

                if (Mathf.Abs(piece.Velocity.y) < 80f)
                {
                    position = Vector2.zero;
                    piece.Angle = 0f;
                    piece.Spin = 0f;
                    piece.Velocity = Vector2.zero;
                    piece.Landed = true;
                    if (i == _pieces.Count - 1)
                        _wait = 1.4f;
                }
            }

            piece.Rect.anchoredPosition = position;
            piece.Rect.localEulerAngles = new Vector3(0f, 0f, piece.Angle);
            _pieces[i] = piece;
        }
    }

    private void StepScraps(float dt)
    {
        for (int i = _scraps.Count - 1; i >= 0; i--)
        {
            Scrap scrap = _scraps[i];
            scrap.Velocity.y -= 2200f * dt;
            scrap.Rect.anchoredPosition += scrap.Velocity * dt;
            scrap.Rect.Rotate(0f, 0f, scrap.Spin * dt);
            scrap.Life -= dt;
            var image = scrap.Rect.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = Mathf.Clamp01(scrap.Life / 0.35f);
                image.color = color;
            }

            if (scrap.Life <= 0f)
            {
                Destroy(scrap.Rect.gameObject);
                _scraps.RemoveAt(i);
                continue;
            }

            _scraps[i] = scrap;
        }
    }

    private bool AllLanded()
    {
        for (int i = 0; i < _pieces.Count; i++)
        {
            if (!_pieces[i].Landed)
                return false;
        }

        return _pieces.Count > 0;
    }

    private void BeginFade()
    {
        _fading = true;
    }

    private static bool WasSkipPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
    }

    private void Burst(RectTransform panel)
    {
        for (int i = 0; i < 5; i++)
        {
            var scrapObject = new GameObject("Scrap", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = scrapObject.GetComponent<RectTransform>();
            rect.SetParent(_frame, false);
            rect.anchorMin = panel.anchorMin;
            rect.anchorMax = panel.anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Random.Range(18f, 42f), Random.Range(10f, 24f));
            rect.anchoredPosition = new Vector2(Random.Range(-40f, 40f), Random.Range(-20f, 30f));
            scrapObject.transform.SetAsLastSibling();
            var image = scrapObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = new Color(0.15f, 0.12f, 0.1f, 0.9f);
            _scraps.Add(new Scrap
            {
                Rect = rect,
                Velocity = new Vector2(Random.Range(-220f, 220f), Random.Range(180f, 520f)),
                Spin = Random.Range(-240f, 240f),
                Life = Random.Range(0.7f, 1.15f)
            });
        }
    }

    private void Build()
    {
        var canvasObject = new GameObject("FourCutCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        _group = canvasObject.GetComponent<CanvasGroup>();

        var black = CreateImage(canvasObject.transform, "Black", null, Color.black);
        Stretch(black.rectTransform);

        float aspect = comic.rect.height <= 0f ? 1.6f : comic.rect.width / comic.rect.height;
        float width = 1500f;
        float height = width / aspect;
        if (height > 860f)
        {
            height = 860f;
            width = height * aspect;
        }

        var frameObject = new GameObject("Frame", typeof(RectTransform));
        _frame = frameObject.GetComponent<RectTransform>();
        _frame.SetParent(canvasObject.transform, false);
        _frame.anchorMin = _frame.anchorMax = new Vector2(0.5f, 0.5f);
        _frame.pivot = new Vector2(0.5f, 0.5f);
        _frame.sizeDelta = new Vector2(width, height);

        AddPiece(0, 1, 40f);
        AddPiece(1, 1, -30f);
        AddPiece(0, 0, 24f);
        AddPiece(1, 0, -18f);

        CreateSkip(canvasObject.transform);
    }

    private void AddPiece(int column, int row, float tilt)
    {
        var maskObject = new GameObject("Panel", typeof(RectTransform), typeof(RectMask2D));
        var mask = maskObject.GetComponent<RectTransform>();
        mask.SetParent(_frame, false);
        mask.anchorMin = new Vector2(column * 0.5f, row * 0.5f);
        mask.anchorMax = new Vector2((column + 1) * 0.5f, (row + 1) * 0.5f);
        mask.offsetMin = Vector2.zero;
        mask.offsetMax = Vector2.zero;
        mask.pivot = new Vector2(0.5f, 0.5f);

        var art = CreateImage(mask, "Art", comic, Color.white);
        float xMin = column == 0 ? 0f : -1f;
        float xMax = column == 0 ? 2f : 1f;
        float yMin = row == 1 ? -1f : 0f;
        float yMax = row == 1 ? 1f : 2f;
        art.rectTransform.anchorMin = new Vector2(xMin, yMin);
        art.rectTransform.anchorMax = new Vector2(xMax, yMax);
        art.rectTransform.offsetMin = Vector2.zero;
        art.rectTransform.offsetMax = Vector2.zero;

        mask.anchoredPosition = new Vector2(Random.Range(-70f, 70f), 980f);
        mask.localEulerAngles = new Vector3(0f, 0f, tilt);
        maskObject.SetActive(false);

        _pieces.Add(new Piece
        {
            Rect = mask,
            Velocity = new Vector2(Random.Range(-60f, 60f), 0f),
            Angle = tilt,
            Spin = Random.Range(-50f, 50f)
        });
    }

    private void CreateSkip(Transform parent)
    {
        var buttonObject = new GameObject("Skip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-48f, 36f);
        rect.sizeDelta = new Vector2(220f, 64f);
        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.92f);
        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => GameFeedback.Current?.PlayClick());
        button.onClick.AddListener(BeginFade);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(rect, false);
        Stretch(labelRect);
        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "건너뛰기";
        label.fontSize = 30f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.12f, 0.1f, 0.08f, 1f);
        label.raycastTarget = false;
        if (font != null)
            label.font = font;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        var image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = sprite == null;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
