using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Loads stages 1 to 4 into the remix stage and walks the camera through them twice.
/// One chart is shared. Notes leave from the spawner of the stage currently on screen.
/// </summary>
public class RemixStageTour : MonoBehaviour
{
    public static bool ShouldStaySilent(Scene scene)
    {
        return _holdBackground && scene.name != _hostSceneName;
    }

    private static bool _holdBackground;
    private static string _hostSceneName = "Stage5";

    [SerializeField] private ChartRuntimeManager chart;
    [SerializeField] private Camera tourCamera;
    [SerializeField] private string[] stageScenes = { "Stage1", "Stage2", "Stage3", "Stage4" };
    [SerializeField] private int laps = 2;
    [SerializeField] private float fadeDuration = 0.08f;
    [SerializeField] private Vector3 stageSeparation = new Vector3(700f, 0f, 0f);

    private readonly List<LoadedStage> _stages = new List<LoadedStage>();
    private Image _fade;
    private GraphicRaycaster _fadeRaycaster;
    private Coroutine _fadeRoutine;
    private int _shown = -1;
    private int _transitionTarget = -1;
    private bool _ready;
    private PollutionManager _pollution;
    private float _segment = 1f;
    private int _visits = 8;

    private struct LoadedStage
    {
        public Scene Scene;
        public Transform Viewpoint;
        public Camera SourceCamera;
        public Transform Spawn;
        public Transform Judge;
    }

    private void Awake()
    {
        _hostSceneName = gameObject.scene.name;
        _holdBackground = true;
        if (chart == null)
            chart = FindFirstObjectByType<ChartRuntimeManager>();
        if (tourCamera == null)
            tourCamera = Camera.main;
        BuildFade();
        EnsureEventSystem();
    }

    private void Start()
    {
        StartCoroutine(LoadAndPlay());
    }

    private void Update()
    {
        if (!_ready || chart == null || !chart.IsPlaybackActive || _segment <= 0f)
            return;

        int visit = Mathf.Clamp(Mathf.FloorToInt(chart.PlaybackTime / _segment), 0, _visits - 1);
        int stage = visit % _stages.Count;
        if (stage == _shown || _fadeRoutine != null)
            return;

        if (!chart.IsReadyToSwitch(fadeDuration * 2f + 0.2f))
            return;

        BeginVisit(stage);
    }

    private IEnumerator LoadAndPlay()
    {
        SetFade(1f);
        for (int i = 0; i < stageScenes.Length; i++)
        {
            string sceneName = stageScenes[i];
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (load == null)
            {
                Debug.LogWarning("[RemixStage] Could not load " + sceneName);
                continue;
            }

            while (!load.isDone)
                yield return null;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
                continue;

            Vector3 shift = stageSeparation * i;
            MoveScene(scene, shift);
            SilenceStage(scene);
            _stages.Add(Capture(scene, shift));
        }

        if (chart == null || _stages.Count == 0)
        {
            SetFade(0f);
            yield break;
        }

        var spawns = new List<Transform>(_stages.Count);
        var judges = new List<Transform>(_stages.Count);
        for (int i = 0; i < _stages.Count; i++)
        {
            spawns.Add(_stages[i].Spawn);
            judges.Add(_stages[i].Judge);
        }

        PollutionManager pollution = chart.GetComponent<PollutionManager>();
        _pollution = pollution;
        chart.ConfigureRemixLanes(spawns, judges);
        chart.AssignRoundRobinLanes(_stages.Count, laps);
        _visits = Mathf.Max(1, _stages.Count * Mathf.Max(1, laps));
        _segment = chart.SegmentLength(_stages.Count, laps);
        DisableHostLights();
        ShowStage(0, true);
        SetFade(0f);
        _ready = true;
        chart.StartAfterCountdown();
    }

    private void BeginVisit(int stage)
    {
        if (_transitionTarget == stage)
            return;

        _transitionTarget = stage;
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossFade(stage));
    }

    private IEnumerator CrossFade(int stage)
    {
        if (chart != null)
            chart.SetSpawnHold(true);

        yield return Fade(1f);
        if (_transitionTarget != stage)
        {
            if (chart != null)
                chart.SetSpawnHold(false);
            yield break;
        }

        ShowStage(stage, false);
        yield return Fade(0f);
        if (chart != null)
            chart.SetSpawnHold(false);
        if (_transitionTarget == stage)
            _fadeRoutine = null;
    }

    private IEnumerator Fade(float target)
    {
        if (_fade == null)
            yield break;

        float start = _fade.color.a;
        float time = 0f;
        float duration = Mathf.Max(0.02f, fadeDuration);
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            SetFade(Mathf.Lerp(start, target, Mathf.Clamp01(time / duration)));
            yield return null;
        }

        SetFade(target);
    }

    private void ShowStage(int stage, bool immediate)
    {
        _shown = stage;
        if (chart != null)
            chart.SetActiveLane(stage);

        for (int i = 0; i < _stages.Count; i++)
        {
            SetStageRootsActive(_stages[i].Scene, i == stage);
            SetPresentation(_stages[i].Scene, i == stage);
        }

        BindStage(_stages[stage].Scene, chart, _pollution);

        LoadedStage visit = _stages[stage];
        if (tourCamera == null || visit.Viewpoint == null)
            return;

        if (visit.SourceCamera != null)
        {
            tourCamera.fieldOfView = visit.SourceCamera.fieldOfView;
            tourCamera.nearClipPlane = visit.SourceCamera.nearClipPlane;
            tourCamera.farClipPlane = visit.SourceCamera.farClipPlane;
        }

        tourCamera.transform.SetPositionAndRotation(visit.Viewpoint.position, visit.Viewpoint.rotation);
        if (immediate)
            SetFade(0f);
    }

    private void DisableHostLights()
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Light[] lights = roots[i].GetComponentsInChildren<Light>(true);
            for (int c = 0; c < lights.Length; c++)
                lights[c].enabled = false;
        }
    }

    private static void SetStageRootsActive(Scene scene, bool active)
    {
        if (!scene.IsValid())
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (IsForeignStageUi(root.name))
            {
                root.SetActive(false);
                continue;
            }

            root.SetActive(active);
        }
    }

    private static bool IsForeignStageUi(string objectName)
    {
        return objectName == "HpCanvas"
            || objectName == "EventSystem"
            || objectName == "ChartManager"
            || objectName == "InputManger"
            || objectName == "InputManager";
    }

    private static void MoveScene(Scene scene, Vector3 shift)
    {
        if (shift.sqrMagnitude < 0.0001f)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
            roots[i].transform.position += shift;

        PollutionSeaLevel[] seas = Object.FindObjectsByType<PollutionSeaLevel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < seas.Length; i++)
        {
            if (seas[i].gameObject.scene == scene)
                seas[i].ShiftBases(shift);
        }
    }

    private static void SilenceStage(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root.name == "HpCanvas")
                root.SetActive(false);

            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
            for (int c = 0; c < cameras.Length; c++)
                cameras[c].enabled = false;

            AudioListener[] listeners = root.GetComponentsInChildren<AudioListener>(true);
            for (int c = 0; c < listeners.Length; c++)
                listeners[c].enabled = false;

            AudioSource[] sources = root.GetComponentsInChildren<AudioSource>(true);
            for (int c = 0; c < sources.Length; c++)
            {
                sources[c].Stop();
                sources[c].playOnAwake = false;
                sources[c].enabled = false;
            }

            EventSystem[] events = root.GetComponentsInChildren<EventSystem>(true);
            for (int c = 0; c < events.Length; c++)
                events[c].gameObject.SetActive(false);

            InputManager[] inputs = root.GetComponentsInChildren<InputManager>(true);
            for (int c = 0; c < inputs.Length; c++)
                inputs[c].enabled = false;

            ChartRuntimeManager[] charts = root.GetComponentsInChildren<ChartRuntimeManager>(true);
            for (int c = 0; c < charts.Length; c++)
            {
                charts[c].StopPlayback();
                charts[c].enabled = false;
            }
        }
    }

    private static LoadedStage Capture(Scene scene, Vector3 shift)
    {
        Camera source = FindStageCamera(scene);
        Transform viewpoint = source != null ? source.transform : null;
        Transform spawner = FindNamed(scene, "ChartSpawner");
        Transform spawn = spawner;
        Transform judge = null;
        if (spawner != null)
        {
            for (int i = 0; i < spawner.childCount; i++)
            {
                Transform child = spawner.GetChild(i);
                if (child.name == "HitChek")
                    judge = child;
                else if (child.name == "Cube")
                    spawn = child;
            }
        }

        if (spawn == null || judge == null)
        {
            Transform anchor = viewpoint != null ? viewpoint : NewRoot(scene, "RemixAnchor", shift);
            if (spawn == null)
                spawn = CreatePoint(anchor, "RemixSpawn", new Vector3(0f, -0.2f, 14f));
            if (judge == null)
                judge = CreatePoint(anchor, "RemixJudge", new Vector3(0f, -0.45f, 3.2f));
        }

        return new LoadedStage
        {
            Scene = scene,
            Viewpoint = viewpoint,
            SourceCamera = source,
            Spawn = spawn,
            Judge = judge
        };
    }

    private static void BindStage(Scene scene, ChartRuntimeManager chart, PollutionManager pollution)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            RhythmAnimalAnimator[] animals = roots[i].GetComponentsInChildren<RhythmAnimalAnimator>(true);
            for (int a = 0; a < animals.Length; a++)
                animals[a].Use(chart);

            if (pollution == null)
                continue;

            PollutionSeaLevel[] seas = roots[i].GetComponentsInChildren<PollutionSeaLevel>(true);
            for (int a = 0; a < seas.Length; a++)
                seas[a].Use(pollution);

            PollutionOppositeScale[] scales = roots[i].GetComponentsInChildren<PollutionOppositeScale>(true);
            for (int a = 0; a < scales.Length; a++)
                scales[a].Use(pollution);

            PollutionStageGrowth[] growths = roots[i].GetComponentsInChildren<PollutionStageGrowth>(true);
            for (int a = 0; a < growths.Length; a++)
                growths[a].Use(pollution);
        }
    }

    private static void SetPresentation(Scene scene, bool active)
    {
        if (!scene.IsValid())
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Light[] lights = roots[i].GetComponentsInChildren<Light>(true);
            for (int c = 0; c < lights.Length; c++)
                lights[c].enabled = active;

            Volume[] volumes = roots[i].GetComponentsInChildren<Volume>(true);
            for (int c = 0; c < volumes.Length; c++)
                volumes[c].enabled = active;
        }
    }

    private static Camera FindStageCamera(Scene scene)
    {
        Camera named = null;
        Camera any = null;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Camera[] cameras = roots[i].GetComponentsInChildren<Camera>(true);
            for (int c = 0; c < cameras.Length; c++)
            {
                Camera camera = cameras[c];
                if (any == null)
                    any = camera;
                if (camera.CompareTag("MainCamera") || camera.name == "Camera" || camera.name == "Main Camera")
                    named = camera;
            }
        }

        return named != null ? named : any;
    }

    private static Transform FindNamed(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int t = 0; t < transforms.Length; t++)
            {
                if (transforms[t].name == objectName)
                    return transforms[t];
            }
        }

        return null;
    }

    private static Transform NewRoot(Scene scene, string objectName, Vector3 position)
    {
        var root = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = position;
        return root.transform;
    }

    private static Transform CreatePoint(Transform parent, string objectName, Vector3 localPosition)
    {
        var point = new GameObject(objectName);
        point.transform.SetParent(parent, false);
        point.transform.localPosition = localPosition;
        point.transform.localRotation = Quaternion.identity;
        return point.transform;
    }

    private void BuildFade()
    {
        var canvasObject = new GameObject("RemixFade", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        _fadeRaycaster = canvasObject.GetComponent<GraphicRaycaster>();
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        var imageObject = new GameObject("Black", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);
        var rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        _fade = imageObject.GetComponent<Image>();
        _fade.color = new Color(0f, 0f, 0f, 0f);
        _fade.raycastTarget = false;
    }

    private void SetFade(float alpha)
    {
        if (_fade == null)
            return;

        Color color = _fade.color;
        color.a = alpha;
        _fade.color = color;
        if (_fadeRaycaster != null)
            _fadeRaycaster.enabled = alpha > 0.01f;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var eventObject = new GameObject("EventSystem");
        eventObject.transform.SetParent(transform, false);
        eventObject.AddComponent<EventSystem>();
        eventObject.AddComponent<InputSystemUIInputModule>();
    }
}
