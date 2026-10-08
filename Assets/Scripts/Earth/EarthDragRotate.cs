using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Spins the earth while the left mouse button is dragged, and zooms the camera with the wheel.
/// </summary>
public class EarthDragRotate : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.28f;
    [SerializeField] private float zoomSpeed = 1.6f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 22f;

    private bool _dragging;
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return;

        Zoom(mouse);

        if (mouse.leftButton.wasPressedThisFrame)
            _dragging = !IsPointerOverUi();

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

    private void Zoom(Mouse mouse)
    {
        var scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f || IsPointerOverScreenUi(mouse))
            return;

        if (Mathf.Abs(scroll) > 10f)
            scroll /= 120f;

        var camera = Camera.main;
        if (camera == null)
            return;

        var offset = camera.transform.position - transform.position;
        var distance = offset.magnitude;
        if (distance < 0.001f)
            return;

        var nextDistance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        camera.transform.position = transform.position + offset.normalized * nextDistance;
    }

    private bool IsPointerOverScreenUi(Mouse mouse)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null || !eventSystem.IsPointerOverGameObject(PointerInputModule.kMouseLeftId))
            return false;

        var pointer = new PointerEventData(eventSystem)
        {
            position = mouse.position.ReadValue()
        };
        _raycastResults.Clear();
        eventSystem.RaycastAll(pointer, _raycastResults);
        for (var i = 0; i < _raycastResults.Count; i++)
        {
            var canvas = _raycastResults[i].gameObject.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                return true;
        }

        return false;
    }

    private static bool IsPointerOverUi()
    {
        var eventSystem = EventSystem.current;
        return eventSystem != null && eventSystem.IsPointerOverGameObject(PointerInputModule.kMouseLeftId);
    }
}
