using UnityEngine;

/// <summary>
/// Places a world canvas on the earth sphere from latitude and longitude.
/// </summary>
[ExecuteAlways]
public class EarthSite : MonoBehaviour
{
    [SerializeField] private float latitude = 35f;
    [SerializeField] private float longitude = 104f;
    [SerializeField] private float distanceFromCenter = 2.45f;

    private void OnEnable()
    {
        Place();
    }

    private void OnValidate()
    {
        Place();
    }

    private void Place()
    {
        if (transform.parent == null)
            return;

        var direction = Direction(latitude, longitude);
        transform.localPosition = direction * distanceFromCenter;
        var up = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.92f ? Vector3.forward : Vector3.up;
        transform.localRotation = Quaternion.LookRotation(-direction, up);
    }

    private static Vector3 Direction(float lat, float lon)
    {
        var latRad = lat * Mathf.Deg2Rad;
        var lonRad = lon * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Cos(latRad) * Mathf.Sin(lonRad),
            Mathf.Sin(latRad),
            Mathf.Cos(latRad) * Mathf.Cos(lonRad));
    }
}
