using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class TitleScreenBuilder
{
    private const string ScenePath = "Assets/Scenes/Title.unity";
    private const string OvalSpritePath = "Assets/Art/UI/ButtonOval.png";
    private const string PanelSpritePath = "Assets/Art/UI/PanelRounded.png";
    private const string TmpFontPath = "Assets/Art/UI/Fonts/MalgunGothic SDF.asset";
    private const string ObsoleteLegacyFontPath = "Assets/Art/UI/Fonts/MalgunGothic.fontsettings";

    [MenuItem("Tools/RecycleRythem/Import TMP Essentials (Run Once)")]
    public static void ImportTmpEssentialsAndExit()
    {
        if (Shader.Find("TextMeshPro/Mobile/Distance Field") != null)
        {
            Debug.Log("[TitleScreenBuilder] TMP shaders already present.");
            EditorApplication.Exit(0);
            return;
        }

        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_FontAsset).Assembly);
        var unityPackagePath = Path.Combine(packageInfo.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");

        // AssetDatabase.ImportPackage is asynchronous; wait for its completion callback before exiting
        // the batch process, otherwise the import never gets a chance to run before Unity quits.
        AssetDatabase.importPackageCompleted += _ =>
        {
            Debug.Log("[TitleScreenBuilder] TMP essential resources import completed.");
            EditorApplication.Exit(0);
        };
        AssetDatabase.importPackageFailed += (packageName, errorMessage) =>
        {
            Debug.LogError($"[TitleScreenBuilder] TMP essential resources import failed: {errorMessage}");
            EditorApplication.Exit(1);
        };

        AssetDatabase.ImportPackage(unityPackagePath, false);
    }

    [MenuItem("Tools/RecycleRythem/Build Title Screen")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<Font>(ObsoleteLegacyFontPath) != null)
            AssetDatabase.DeleteAsset(ObsoleteLegacyFontPath);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null) Object.DestroyImmediate(existingCanvas.gameObject);
        var existingEventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (existingEventSystem != null) Object.DestroyImmediate(existingEventSystem.gameObject);
        var existingController = Object.FindFirstObjectByType<TitleScreenController>();
        if (existingController != null) Object.DestroyImmediate(existingController.gameObject);

        EnsureTmpEssentialResourcesImported();
        Sprite ovalSprite = CreateOvalSprite();
        Sprite panelSprite = CreatePanelSprite();
        TMP_FontAsset font = GetKoreanFontAsset();

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var controllerGO = new GameObject("TitleScreenController");
        var controller = controllerGO.AddComponent<TitleScreenController>();

        // Button stack, centered, lower-middle of the screen (title is expected to come from background art now).
        var containerGO = CreateUIObject("ButtonContainer", canvasGO.transform);
        var containerRt = containerGO.GetComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, 0.5f);
        containerRt.anchorMax = new Vector2(0.5f, 0.5f);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.anchoredPosition = new Vector2(0f, -160f);
        containerRt.sizeDelta = new Vector2(440, 360);
        var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 28f;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        CreateOvalButton(containerGO.transform, "StartButton", "게임 시작", ovalSprite, font,
            new Color(0.30f, 0.62f, 0.32f), controller.OnClickStart);
        CreateOvalButton(containerGO.transform, "SettingsButton", "설정", ovalSprite, font,
            new Color(0.36f, 0.54f, 0.40f), controller.OnClickSettings);
        CreateOvalButton(containerGO.transform, "QuitButton", "게임 종료", ovalSprite, font,
            new Color(0.55f, 0.30f, 0.28f), controller.OnClickQuit);

        var settingsPanel = CreateSettingsPanel(canvasGO.transform, ovalSprite, panelSprite, font, controller);

        var controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        controllerSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Title screen build complete.");
    }

    private static GameObject CreateSettingsPanel(Transform canvasTransform, Sprite ovalSprite, Sprite panelSprite,
        TMP_FontAsset font, TitleScreenController controller)
    {
        // Backdrop: dims the screen, blocks clicks to what's behind, and closes the panel if clicked outside the window.
        var backdropGO = CreateUIObject("SettingsPanel", canvasTransform);
        var backdropRt = backdropGO.GetComponent<RectTransform>();
        backdropRt.anchorMin = Vector2.zero;
        backdropRt.anchorMax = Vector2.one;
        backdropRt.offsetMin = Vector2.zero;
        backdropRt.offsetMax = Vector2.zero;
        var backdropImg = backdropGO.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.6f);
        var backdropBtn = backdropGO.AddComponent<Button>();
        backdropBtn.targetGraphic = backdropImg;
        backdropBtn.transition = Selectable.Transition.None;
        UnityEventTools.AddPersistentListener(backdropBtn.onClick, controller.CloseSettingsPanel);

        var windowGO = CreateUIObject("Window", backdropGO.transform);
        var windowRt = windowGO.GetComponent<RectTransform>();
        windowRt.anchorMin = new Vector2(0.5f, 0.5f);
        windowRt.anchorMax = new Vector2(0.5f, 0.5f);
        windowRt.pivot = new Vector2(0.5f, 0.5f);
        windowRt.sizeDelta = new Vector2(640, 560);
        var windowImg = windowGO.AddComponent<Image>();
        windowImg.sprite = panelSprite;
        windowImg.type = Image.Type.Sliced;
        windowImg.color = new Color(0.09f, 0.14f, 0.11f, 0.97f);

        var headerGO = CreateUIObject("Header", windowGO.transform);
        var headerRt = headerGO.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0f, -28f);
        headerRt.sizeDelta = new Vector2(-80f, 60f);
        var headerText = headerGO.AddComponent<TextMeshProUGUI>();
        headerText.text = "설정";
        headerText.font = font;
        headerText.fontSize = 40;
        headerText.fontStyle = FontStyles.Bold;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;

        var closeGO = CreateUIObject("CloseButton", windowGO.transform);
        var closeRt = closeGO.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1f, 1f);
        closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot = new Vector2(1f, 1f);
        closeRt.anchoredPosition = new Vector2(-18f, -18f);
        closeRt.sizeDelta = new Vector2(44f, 44f);
        var closeImg = closeGO.AddComponent<Image>();
        closeImg.sprite = ovalSprite;
        closeImg.color = new Color(0.55f, 0.30f, 0.28f);
        var closeBtn = closeGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        UnityEventTools.AddPersistentListener(closeBtn.onClick, controller.CloseSettingsPanel);

        var closeLabelGO = CreateUIObject("Label", closeGO.transform);
        var closeLabelRt = closeLabelGO.GetComponent<RectTransform>();
        closeLabelRt.anchorMin = Vector2.zero;
        closeLabelRt.anchorMax = Vector2.one;
        closeLabelRt.offsetMin = Vector2.zero;
        closeLabelRt.offsetMax = Vector2.zero;
        var closeLabelText = closeLabelGO.AddComponent<TextMeshProUGUI>();
        closeLabelText.text = "X";
        closeLabelText.font = font;
        closeLabelText.fontSize = 24;
        closeLabelText.fontStyle = FontStyles.Bold;
        closeLabelText.alignment = TextAlignmentOptions.Center;
        closeLabelText.color = Color.white;

        // Scroll area that future settings get added into — see Content below.
        var scrollGO = CreateUIObject("SettingsScrollView", windowGO.transform);
        var scrollRt = scrollGO.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.06f, 0.09f);
        scrollRt.anchorMax = new Vector2(0.94f, 0.80f);
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = Vector2.zero;
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        var viewportGO = CreateUIObject("Viewport", scrollGO.transform);
        var viewportRt = viewportGO.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;
        viewportGO.AddComponent<RectMask2D>();

        var contentGO = CreateUIObject("Content", viewportGO.transform);
        var contentRt = contentGO.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        var contentVlg = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVlg.spacing = 18f;
        contentVlg.padding = new RectOffset(6, 6, 6, 6);
        contentVlg.childControlWidth = true;
        contentVlg.childControlHeight = false;
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;
        var contentFitter = contentGO.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;

        // Add more setting rows here as children of "Content" — CreateVolumeSettingRow shows the pattern.
        CreateVolumeSettingRow(contentGO.transform, font, ovalSprite, controller);

        backdropGO.SetActive(false);
        return backdropGO;
    }

    private static void CreateVolumeSettingRow(Transform parent, TMP_FontAsset font, Sprite handleSprite,
        TitleScreenController controller)
    {
        var rowGO = CreateUIObject("SettingRow_Volume", parent);
        var rowRt = rowGO.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(0f, 56f);
        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.spacing = 28f;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var labelGO = CreateUIObject("Label", rowGO.transform);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.sizeDelta = new Vector2(170f, 40f);
        var labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = "음량";
        labelText.font = font;
        labelText.fontSize = 28;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        var slider = CreateVolumeSlider(rowGO.transform, handleSprite);
        slider.value = AudioListener.volume;
        UnityEventTools.AddPersistentListener(slider.onValueChanged, controller.SetMasterVolume);
    }

    private static Slider CreateVolumeSlider(Transform parent, Sprite handleSprite)
    {
        var sliderGO = CreateUIObject("VolumeSlider", parent);
        var sliderRt = sliderGO.GetComponent<RectTransform>();
        sliderRt.sizeDelta = new Vector2(320f, 24f);
        var slider = sliderGO.AddComponent<Slider>();

        var bgGO = CreateUIObject("Background", sliderGO.transform);
        var bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.15f);

        var fillAreaGO = CreateUIObject("Fill Area", sliderGO.transform);
        var fillAreaRt = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(4f, 0f);
        fillAreaRt.offsetMax = new Vector2(-4f, 0f);

        var fillGO = CreateUIObject("Fill", fillAreaGO.transform);
        var fillRt = fillGO.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.sizeDelta = new Vector2(10f, 0f);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.36f, 0.75f, 0.40f);

        var handleAreaGO = CreateUIObject("Handle Slide Area", sliderGO.transform);
        var handleAreaRt = handleAreaGO.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = new Vector2(0f, 0f);
        handleAreaRt.anchorMax = new Vector2(1f, 1f);
        handleAreaRt.offsetMin = new Vector2(10f, 0f);
        handleAreaRt.offsetMax = new Vector2(-10f, 0f);

        var handleGO = CreateUIObject("Handle", handleAreaGO.transform);
        var handleRt = handleGO.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(24f, 24f);
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.sprite = handleSprite;
        handleImg.color = Color.white;

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        return slider;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void CreateOvalButton(Transform parent, string name, string label, Sprite sprite,
        TMP_FontAsset font, Color color, UnityAction callback)
    {
        var go = CreateUIObject(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(380, 92);

        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.color = color;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        UnityEventTools.AddPersistentListener(btn.onClick, callback);

        var labelGO = CreateUIObject("Label", go.transform);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        var labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.font = font;
        labelText.fontSize = 34;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
    }

    private static void EnsureTmpEssentialResourcesImported()
    {
        // Dynamic-OS-font TMP_FontAsset creation needs TMP's SDF shader, which only exists in the
        // project after this bundled package is imported (the "Import TMP Essential Resources" step).
        if (Shader.Find("TextMeshPro/Mobile/Distance Field") != null) return;

        var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_FontAsset).Assembly);
        if (packageInfo == null)
        {
            Debug.LogWarning("[TitleScreenBuilder] Could not resolve the TMP package path to import essential resources.");
            return;
        }

        var unityPackagePath = Path.Combine(packageInfo.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
        if (!File.Exists(unityPackagePath))
        {
            Debug.LogWarning($"[TitleScreenBuilder] TMP Essential Resources not found at {unityPackagePath}");
            return;
        }

        AssetDatabase.ImportPackage(unityPackagePath, false);
        AssetDatabase.Refresh();
    }

    private static TMP_FontAsset GetKoreanFontAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        if (existing != null) return existing;

        EnsureFolder("Assets/Art/UI/Fonts");
        var fontAsset = TMP_FontAsset.CreateFontAsset("Malgun Gothic", "Regular");
        if (fontAsset == null)
            fontAsset = TMP_FontAsset.CreateFontAsset("Arial", "Regular");

        AssetDatabase.CreateAsset(fontAsset, TmpFontPath);
        // The atlas texture and material are separate objects TMP creates in memory; without adding
        // them as sub-assets here they aren't persisted and the font breaks on the next editor session.
        AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
    }

    private static Sprite CreateOvalSprite()
    {
        EnsureFolder("Assets/Art/UI");

        const int w = 512;
        const int h = 192;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];

        for (var y = 0; y < h; y++)
        {
            var ny = (y + 0.5f) / h * 2f - 1f;
            for (var x = 0; x < w; x++)
            {
                var nx = (x + 0.5f) / w * 2f - 1f;
                var d = nx * nx + ny * ny;

                var aa = 4f / Mathf.Min(w, h);
                var alpha = 1f - Mathf.Clamp01((d - (1f - aa)) / aa);

                var rim = Mathf.Clamp01((d - 0.78f) / 0.20f);
                var shade = Mathf.Lerp(1f, 0.80f, rim);

                var topHighlight = Mathf.Clamp01((-ny - 0.15f) / 0.65f) * (1f - rim);
                shade = Mathf.Min(1.18f, shade + topHighlight * 0.18f);

                var c = (byte)Mathf.Clamp(shade * 255f, 0, 255);
                var a = (byte)Mathf.Clamp(alpha * 255f, 0, 255);
                pixels[y * w + x] = new Color32(c, c, c, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        File.WriteAllBytes(OvalSpritePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(OvalSpritePath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(OvalSpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(OvalSpritePath);
    }

    private static Sprite CreatePanelSprite()
    {
        EnsureFolder("Assets/Art/UI");

        const int size = 128;
        const int radius = 40;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = Mathf.Max(Mathf.Abs(x + 0.5f - size / 2f) - (size / 2f - radius), 0f);
                var dy = Mathf.Max(Mathf.Abs(y + 0.5f - size / 2f) - (size / 2f - radius), 0f);
                var dist = Mathf.Sqrt(dx * dx + dy * dy) - radius;

                const float aa = 1.5f;
                var alpha = 1f - Mathf.Clamp01((dist + aa) / (2f * aa));
                var shade = Mathf.Lerp(1.04f, 0.94f, (float)y / size);

                var c = (byte)Mathf.Clamp(shade * 255f, 0, 255);
                var a = (byte)Mathf.Clamp(alpha * 255f, 0, 255);
                pixels[y * size + x] = new Color32(c, c, c, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        File.WriteAllBytes(PanelSpritePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(PanelSpritePath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(PanelSpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteBorder = new Vector4(radius + 4, radius + 4, radius + 4, radius + 4);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(PanelSpritePath);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
