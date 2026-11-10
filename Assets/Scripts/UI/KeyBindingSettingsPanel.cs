using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 기존 타이틀 프리팹과 씬의 설정 창에 키 변경 UI를 한 번만 추가합니다.
public class KeyBindingSettingsPanel : MonoBehaviour
{
    private TMP_Text keyLabel;
    private GameObject prompt;
    private CanvasGroup settingsGroup;
    private bool waiting;
    private int startedFrame;
    private bool previousInteractable;

    private void Awake()
    {
        Transform content = transform.Find("Window/SettingsScrollView/Viewport/Content");
        if (content == null) return;
        TMP_Text existingLabel = GetComponentInChildren<TMP_Text>(true);
        TMP_FontAsset font = existingLabel != null ? existingLabel.font : TMP_Settings.defaultFontAsset;

        // 기존 음량 항목 아래에 현재 키와 변경 버튼을 표시합니다.
        RectTransform row = CreateRect("SettingRow_AttackKey", content);
        row.sizeDelta = new Vector2(0f, 64f);
        keyLabel = CreateText("CurrentKey", row, font, 26f);
        keyLabel.rectTransform.anchorMax = new Vector2(0.62f, 1f);
        RectTransform buttonRect = CreateRect("ChangeKeyButton", row);
        Stretch(buttonRect);
        buttonRect.anchorMin = new Vector2(0.65f, 0f);
        Image image = buttonRect.gameObject.AddComponent<Image>();
        image.color = new Color(0.36f, 0.54f, 0.40f);
        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(BeginRebind);
        CreateText("Label", buttonRect, font, 26f).text = "키 변경";

        // 전체 설정 창을 덮는 모달로 클릭을 막고 안내 문구를 화면 중앙에 배치합니다.
        settingsGroup = GetComponent<CanvasGroup>();
        if (settingsGroup == null) settingsGroup = gameObject.AddComponent<CanvasGroup>();
        RectTransform overlay = CreateRect("KeyChangePrompt", transform);
        Stretch(overlay);
        prompt = overlay.gameObject;
        Image backdrop = prompt.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.88f);
        TMP_Text message = CreateText("Message", overlay, font, 32f);
        message.text = "변경을 원하는 키로 누르세요:\n<size=24>Esc: 취소</size>";
        prompt.SetActive(false);
        RefreshLabel();
    }

    private void OnEnable()
    {
        RefreshLabel();
    }

    // 버튼을 누른 프레임은 제외하여 Enter/Space로 버튼을 선택해도 즉시 재지정되지 않습니다.
    public void BeginRebind()
    {
        if (prompt == null || waiting) return;
        waiting = true;
        startedFrame = Time.frameCount;
        previousInteractable = settingsGroup.interactable;
        settingsGroup.interactable = false;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        prompt.SetActive(true);
        prompt.transform.SetAsLastSibling();
    }

    private void Update()
    {
        if (!waiting || Time.frameCount <= startedFrame || Keyboard.current == null) return;
        Keyboard keyboard = Keyboard.current;

        // 여러 키가 동시에 눌려도 Esc 취소를 가장 먼저 처리하고 기존 설정은 유지합니다.
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelRebind();
            return;
        }

        // 마우스와 게임패드는 제외하고 새로 누른 키보드 키만 저장합니다.
        foreach (var key in keyboard.allKeys)
        {
            // IME 상태처럼 물리 키가 아닌 합성 컨트롤은 키 지정 대상에서 제외합니다.
            if (key.synthetic || !key.wasPressedThisFrame || !KeyboardBindingSettings.SetAttackKey(key.keyCode)) continue;
            CancelRebind();
            RefreshLabel();
            return;
        }
    }

    // 설정 창을 닫거나 오브젝트가 비활성화되면 입력 대기도 함께 해제합니다.
    public void CancelRebind()
    {
        if (waiting && settingsGroup != null) settingsGroup.interactable = previousInteractable;
        waiting = false;
        if (prompt != null) prompt.SetActive(false);
    }

    private void OnDisable()
    {
        CancelRebind();
    }

    private void RefreshLabel()
    {
        if (keyLabel == null) return;
        Key key = KeyboardBindingSettings.GetAttackKey();
        string name = Keyboard.current != null ? Keyboard.current[key].displayName : key.ToString();
        keyLabel.text = "입력 키: " + name;
    }

    // 생성하는 UI는 부모의 좌표계를 사용하여 기존 Canvas 배율을 따릅니다.
    private static RectTransform CreateRect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // 기존 한글 폰트를 재사용하며 텍스트가 버튼 클릭을 가로채지 않게 합니다.
    private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, float size)
    {
        RectTransform rect = CreateRect(name, parent);
        Stretch(rect);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }
}
