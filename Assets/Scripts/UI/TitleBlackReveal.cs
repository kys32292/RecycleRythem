using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps a full-screen black image solid, then opens a circle from the center until the black is gone.
/// </summary>
public class TitleBlackReveal : MonoBehaviour
{
    private static readonly int HoleRadiusId = Shader.PropertyToID("_HoleRadius");
    private static readonly int AspectId = Shader.PropertyToID("_Aspect");

    [SerializeField] private Shader revealShader;

    [Header("타이밍")]
    [Tooltip("검은 화면이 유지되는 시간(초). 이 시간이 지난 뒤 가운데에서 원이 커집니다.")]
    [Min(0f)]
    [SerializeField] private float delay = 2f;

    [Tooltip("원이 화면 끝까지 커지는 데 걸리는 시간(초).")]
    [Min(0.01f)]
    [SerializeField] private float expandDuration = 1.2f;

    private Image _image;
    private Material _material;
    private float _radius;
    private float _endRadius;

    private void Start()
    {
        _image = GetComponent<Image>();
        if (_image == null || revealShader == null)
        {
            Debug.LogError("TitleBlackReveal needs an Image and the UI/CircularReveal shader.", this);
            return;
        }

        RectTransform rect = (RectTransform)transform;
        float worldWidth = rect.TransformVector(new Vector3(rect.rect.width, 0f, 0f)).magnitude;
        float worldHeight = rect.TransformVector(new Vector3(0f, rect.rect.height, 0f)).magnitude;
        if (worldHeight < 0.001f)
            worldHeight = 1f;

        _material = new Material(revealShader)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        _material.SetFloat(AspectId, worldWidth / worldHeight);
        _material.SetFloat(HoleRadiusId, 0f);
        _image.material = _material;
        _endRadius = MeasureCoverRadius(rect, worldHeight);

        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(true);
        sequence.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        if (delay > 0f)
            sequence.AppendInterval(delay);

        sequence.Append(DOTween.To(GetRadius, SetRadius, _endRadius, expandDuration).SetEase(Ease.InOutSine));
        sequence.OnComplete(ClearBlack);
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }

    private float GetRadius()
    {
        return _radius;
    }

    private void SetRadius(float value)
    {
        _radius = value;
        _material.SetFloat(HoleRadiusId, value);
    }

    private void ClearBlack()
    {
        if (_image != null)
            _image.enabled = false;
    }

    private float MeasureCoverRadius(RectTransform rect, float worldHeight)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform area = canvas != null ? canvas.transform as RectTransform : rect;
        Vector3[] corners = new Vector3[4];
        area.GetWorldCorners(corners);

        Vector3 center = rect.TransformPoint(rect.rect.center);
        float maxDistance = 0f;
        for (int i = 0; i < corners.Length; i++)
            maxDistance = Mathf.Max(maxDistance, Vector3.Distance(center, corners[i]));

        return maxDistance / worldHeight * 1.02f;
    }
}
