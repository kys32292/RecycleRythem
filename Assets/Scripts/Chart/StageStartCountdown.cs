using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen 3, 2, 1 shown for one second each before a stage begins.
/// </summary>
public class StageStartCountdown
{
    private readonly GameObject _root;
    private readonly Text _number;

    private StageStartCountdown(GameObject root, Text number)
    {
        _root = root;
        _number = number;
    }

    public static StageStartCountdown Create()
    {
        var canvasObject = new GameObject("StageCountdown", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var blackObject = new GameObject("Black", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        blackObject.transform.SetParent(canvasObject.transform, false);
        Stretch(blackObject.GetComponent<RectTransform>());
        var black = blackObject.GetComponent<Image>();
        black.color = new Color(0f, 0f, 0f, 0.72f);
        black.raycastTarget = true;

        var numberObject = new GameObject("Number", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        numberObject.transform.SetParent(canvasObject.transform, false);
        var rect = numberObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 700f);
        var number = numberObject.GetComponent<Text>();
        number.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        number.fontSize = 320;
        number.alignment = TextAnchor.MiddleCenter;
        number.color = new Color(1f, 0.82f, 0.15f, 1f);
        number.raycastTarget = false;
        number.horizontalOverflow = HorizontalWrapMode.Overflow;
        number.verticalOverflow = VerticalWrapMode.Overflow;

        return new StageStartCountdown(canvasObject, number);
    }

    public void Show(int number)
    {
        if (_number == null)
            return;

        _number.text = number.ToString();
    }

    public void Close()
    {
        if (_root != null)
            Object.Destroy(_root);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
