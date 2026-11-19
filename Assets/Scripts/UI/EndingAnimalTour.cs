using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Shows each ending animal from the front. The screen goes black while the camera changes.
/// </summary>
public class EndingAnimalTour : MonoBehaviour
{
    [Tooltip("비워 두면 씬에 놓인 동물을 계층 순서대로 찾습니다.")]
    [SerializeField] private Transform[] animals;

    [SerializeField] private Camera tourCamera;

    [SerializeField] private string titleSceneName = "Title";

    [Min(0.1f)]
    [SerializeField] private float secondsPerAnimal = 4f;

    [Min(0.01f)]
    [SerializeField] private float fadeDuration = 1.5f;

    [SerializeField] private float faceDistance = 14f;
    [SerializeField] private float faceHeight = 1.6f;

    [Tooltip("상어는 지금처럼 앞에서 비스듬히 내려다보되, 더 멀리서 봅니다.")]
    [SerializeField] private float sharkDistance = 28f;

    [Tooltip("늑대와 사슴은 옆에서 이 거리만큼 떨어져 봅니다.")]
    [SerializeField] private float sideDistance = 26f;

    private Image _black;

    private void Awake()
    {
        if (tourCamera == null)
            tourCamera = Camera.main;

        BuildFade();
        SetAlpha(1f);
        HidePathCubes();
    }

    private void Start()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        Transform[] targets = CollectAnimals();
        bool screenIsBlack = true;
        yield return Fade(0f);
        screenIsBlack = false;

        for (int i = 0; i < targets.Length; i++)
        {
            Transform animal = targets[i];
            if (animal == null || !animal.gameObject.activeInHierarchy)
                continue;

            if (!screenIsBlack)
                yield return Fade(1f);

            bool moves = TryGetPath(animal, out Vector3 start, out Vector3 end);
            bool hasCamera = TryGetCamera(animal, out Vector3 cameraPosition, out Quaternion cameraRotation);
            Transform partner = animal.name == "wolf" ? FindRoot("wolf (1)") : null;
            Vector3 partnerStart = start;
            Vector3 partnerEnd = end;
            if (moves && partner != null)
                SideBySide(start, end, out partnerStart, out partnerEnd);

            if (moves)
            {
                SnapToStart(animal, start, end);
                if (partner != null)
                    SnapToStart(partner, partnerStart, partnerEnd);
            }

            PlaceCamera(animal, hasCamera, cameraPosition, cameraRotation);
            if (moves)
            {
                yield return MoveBetween(animal, start, end, partner, partnerStart, partnerEnd);
                screenIsBlack = true;
            }
            else
            {
                yield return Fade(0f);
                float hold = Mathf.Max(0f, secondsPerAnimal - fadeDuration);
                yield return new WaitForSeconds(hold);
                yield return Fade(1f);
                screenIsBlack = true;
            }
        }

        if (!screenIsBlack)
            yield return Fade(1f);
        SceneManager.LoadScene(titleSceneName);
    }

    private void PlaceCamera(Transform animal, bool hasCamera, Vector3 cameraPosition, Quaternion cameraRotation)
    {
        if (tourCamera == null)
            return;

        if (hasCamera)
        {
            tourCamera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            return;
        }

        Vector3 look = animal.position + Vector3.up * faceHeight;
        Vector3 destination = look + CameraOffset(animal);
        tourCamera.transform.SetPositionAndRotation(destination, Quaternion.LookRotation(look - destination));
    }

    private static bool TryGetCamera(Transform animal, out Vector3 position, out Quaternion rotation)
    {
        Transform cameraPoint = animal.Find("카메라");
        if (cameraPoint == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        position = cameraPoint.position;
        rotation = cameraPoint.rotation;
        return true;
    }

    private static void SideBySide(Vector3 start, Vector3 end, out Vector3 partnerStart, out Vector3 partnerEnd)
    {
        Vector3 direction = end - start;
        Vector3 side = Vector3.Cross(Vector3.up, direction);
        if (side.sqrMagnitude < 0.001f)
            side = Vector3.right;
        side = side.normalized * 3f;
        partnerStart = start + side;
        partnerEnd = end + side;
    }

    private Transform FindRoot(string objectName)
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == objectName)
                return roots[i].transform;
        }

        return null;
    }

    private Vector3 CameraOffset(Transform animal)
    {
        string objectName = animal.name;
        if (objectName.StartsWith("shark"))
        {
            Vector3 front = animal.forward * faceDistance + Vector3.up * 0.35f;
            return front.normalized * sharkDistance;
        }

        if (objectName.StartsWith("wolf") || objectName.StartsWith("Deear") || objectName.StartsWith("Deer"))
        {
            Vector3 side = Vector3.Cross(Vector3.up, animal.forward);
            if (side.sqrMagnitude < 0.001f)
                side = animal.right;
            return side.normalized * sideDistance + Vector3.up * 3f;
        }

        return animal.forward * faceDistance + Vector3.up * 0.35f;
    }

    private static void SnapToStart(Transform animal, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        Quaternion facing = direction.sqrMagnitude > 0.01f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : animal.rotation;
        animal.SetPositionAndRotation(start, facing);
    }

    private IEnumerator MoveBetween(
        Transform animal,
        Vector3 start,
        Vector3 end,
        Transform partner,
        Vector3 partnerStart,
        Vector3 partnerEnd)
    {
        Vector3 direction = end - start;
        Quaternion facing = direction.sqrMagnitude > 0.01f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : animal.rotation;
        float darkBeforeStop = 0.25f;
        float fadeOutStart = Mathf.Max(0f, secondsPerAnimal - fadeDuration - darkBeforeStop);
        float elapsed = 0f;
        while (elapsed < secondsPerAnimal)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / secondsPerAnimal);
            animal.SetPositionAndRotation(Vector3.Lerp(start, end, t), facing);
            if (partner != null)
                partner.SetPositionAndRotation(Vector3.Lerp(partnerStart, partnerEnd, t), facing);

            if (elapsed < fadeDuration)
                SetAlpha(Mathf.Lerp(1f, 0f, Mathf.Clamp01(elapsed / fadeDuration)));
            else if (elapsed >= fadeOutStart)
                SetAlpha(Mathf.Lerp(0f, 1f, Mathf.Clamp01((elapsed - fadeOutStart) / fadeDuration)));

            yield return null;
        }

        SetAlpha(1f);
        animal.SetPositionAndRotation(end, facing);
        if (partner != null)
            partner.SetPositionAndRotation(partnerEnd, facing);
    }

    private static bool TryGetPath(Transform animal, out Vector3 start, out Vector3 end)
    {
        Transform startPoint = animal.Find("시작");
        Transform endPoint = animal.Find("도착");
        if (startPoint == null || endPoint == null)
        {
            start = end = Vector3.zero;
            return false;
        }

        start = startPoint.position;
        end = endPoint.position;
        return true;
    }

    private void HidePathCubes()
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (!IsAnimal(roots[i].name))
                continue;

            HideCube(roots[i].transform.Find("시작"));
            HideCube(roots[i].transform.Find("도착"));
            HideCube(roots[i].transform.Find("카메라"));
        }
    }

    private static void HideCube(Transform cube)
    {
        if (cube == null)
            return;

        Renderer[] renderers = cube.GetComponents<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = false;
    }

    private Transform[] CollectAnimals()
    {
        if (animals != null && animals.Length > 0)
        {
            var assigned = new List<Transform>();
            for (int i = 0; i < animals.Length; i++)
            {
                if (animals[i] != null && IsShown(animals[i].name))
                    assigned.Add(animals[i]);
            }

            if (assigned.Count > 0)
                return assigned.ToArray();
        }

        var found = new List<Transform>();
        GameObject[] roots = gameObject.scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (IsShown(roots[i].name))
                found.Add(roots[i].transform);
        }

        return found.ToArray();
    }

    private static bool IsShown(string objectName)
    {
        return objectName != "wolf (1)" && IsAnimal(objectName);
    }

    private static bool IsAnimal(string objectName)
    {
        return objectName.StartsWith("wolf")
            || objectName.StartsWith("Fox")
            || objectName.StartsWith("shark")
            || objectName.StartsWith("Deear")
            || objectName.StartsWith("Deer");
    }

    private IEnumerator Fade(float target)
    {
        float start = _black != null ? _black.color.a : 0f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / fadeDuration)));
            yield return null;
        }

        SetAlpha(target);
    }

    private void BuildFade()
    {
        GameObject canvasObject = new GameObject("EndingFade");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        canvasObject.AddComponent<CanvasScaler>();

        GameObject imageObject = new GameObject("Black");
        imageObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        _black = imageObject.AddComponent<Image>();
        _black.color = Color.black;
        _black.raycastTarget = false;
    }

    private void SetAlpha(float alpha)
    {
        if (_black == null)
            return;

        Color color = _black.color;
        color.a = alpha;
        _black.color = color;
    }
}
