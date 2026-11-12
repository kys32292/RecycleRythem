using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Spins this body like the earth, only while the pointer is near it.
/// </summary>
public class MoonProximityRotate : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.28f;
    [Tooltip("달 모델의 로컬 반지름. 이 거리 안에 포인터가 있을 때만 돌아갑니다.")]
    [SerializeField] private float localRadius = 2.6f;
    [SerializeField] private float nearPadding = 1.2f;

    private static MoonProximityRotate _active;
    private bool _dragging;

    private void OnEnable()
    {
        _active = this;
    }

    private void OnDisable()
    {
        if (_active == this)
            _active = null;
        _dragging = false;
    }

    public static bool IsPointerNear(Mouse mouse)
    {
        return _active != null && _active.PointerIsNear(mouse);
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
            _dragging = PointerIsNear(mouse);

        if (!mouse.leftButton.isPressed)
            _dragging = false;

        if (!_dragging)
            return;

        var delta = mouse.delta.ReadValue();
        var camera = Camera.main;
        var pitchAxis = camera != null ? camera.transform.right : Vector3.right;
        transform.Rotate(Vector3.up, -delta.x * sensitivity, Space.World);
        transform.Rotate(pitchAxis, delta.y * sensitivity, Space.World);
    }

    private bool PointerIsNear(Mouse mouse)
    {
        var camera = Camera.main;
        if (camera == null)
            return false;

        var scale = transform.lossyScale.x;
        var radius = localRadius * Mathf.Abs(scale) * nearPadding;
        var center = camera.WorldToScreenPoint(transform.position);
        if (center.z <= 0f)
            return false;

        var edgeWorld = transform.position + camera.transform.right * radius;
        var edge = camera.WorldToScreenPoint(edgeWorld);
        var pixelRadius = Vector2.Distance(new Vector2(center.x, center.y), new Vector2(edge.x, edge.y));
        var pointer = mouse.position.ReadValue();
        return Vector2.Distance(new Vector2(center.x, center.y), pointer) <= pixelRadius;
    }
}
