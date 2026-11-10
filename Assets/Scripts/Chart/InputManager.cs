using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // 키가 눌렸다는 사실을 다른 스크립트에 알리는 이벤트입니다. 노트 판정은 받는 쪽에서 합니다.
    // 다른 스크립트는 += 함수이름으로 구독하고, -= 함수이름으로 구독을 해제합니다.
    public event Action Pressed;
    // 키가 떼졌다는 사실을 다른 스크립트에 알리는 이벤트입니다. 노트 판정은 받는 쪽에서 합니다.
    // 다른 스크립트는 += 함수이름으로 구독하고, -= 함수이름으로 구독을 해제합니다.
    public event Action Released;

    // 현재 누르고 있는지를 저장합니다. 외부에서는 읽을 수 있고, 값 변경은 이 클래스에서만 가능합니다.
    // 현재 코드는 아래 Update에서 이 값을 바꿉니다. 입력 시스템이 자동으로 채워주는 값은 아닙니다.
    public bool IsActionHeld { get; private set; }

    [Header("Keyboard Bindings")] // Inspector에 표시하는 항목 제목입니다.
    // 사용할 키 종류를 저장합니다. 기본값은 스페이스바이며, Inspector에서도 편집할 수 있습니다.
    // 이미 씬이나 프리팹에 저장한 값이 있으면 그 값이 사용됩니다. 실제 입력 감지는 하지 않습니다.
    [SerializeField] private Key keyboardAttackKey = Key.Space;
    // 사용자가 저장한 키를 우선 적용하고, 설정이 없으면 Inspector 값을 사용합니다.
    public Key AttackKey => KeyboardBindingSettings.GetAttackKey(keyboardAttackKey);
    private Key boundKey;

    // 어떤 버튼을 감지할지 설정하고, 눌림/뗌 상태를 조회하는 입력 객체를 담는 변수입니다.
    // 변수 이름이 Action일 뿐, 위 event에 사용한 System.Action(함수 형식)과는 다른 것입니다.
    private InputAction Action;

    // Unity가 이 컴포넌트의 초기화 때 호출합니다. 입력을 활성화하기 전에 입력 객체를 준비합니다.
    private void Awake()
    {
        BuildActions();
    }

    // 에디터에서 컴포넌트가 로드되거나 Inspector 값이 변경될 때 호출됩니다.
    // 여기서는 플레이 중 Inspector에서 키를 바꿨을 때 입력 설정을 다시 만듭니다.
    // 게임 내 설정 창의 키 변경/저장을 구현해주는 함수는 아닙니다.
    private void OnValidate()
    {
        if (!Application.isPlaying) // 플레이 중이 아니면 아래 작업을 하지 않고 함수를 끝냅니다.
        {
            return;
        }

        // 입력 객체가 존재하고 활성화되어 있었는지 기억합니다. &&는 두 조건이 모두 참이어야 참입니다.
        bool wasEnabled = Action != null && Action.enabled;
        BuildActions(); // 기존 객체를 정리하고 새 입력 객체를 만듭니다. 새 객체는 비활성 상태입니다.

        if (wasEnabled) // 이전에 활성화되어 있었다면 새 객체도 활성화합니다.
        {
            Action.Enable();
        }
    }

    // 컴포넌트가 활성화될 때 Unity가 호출합니다. 입력 감지를 켭니다.
    private void OnEnable()
    {
        Action.Enable();
    }

    // 컴포넌트가 비활성화될 때 Unity가 호출합니다. 객체를 버리지는 않고 입력 감지만 끕니다.
    private void OnDisable()
    {
        IsActionHeld = false; // 비활성화되면 누르고 있는 상태를 초기화합니다.
        Action.Disable();
    }

    // 활성화된 동안 매 프레임 실행하여 입력 상태를 확인합니다.
    private void Update()
    {
        // 설정이 변경되면 활성 씬에서도 즉시 반영하고 이전 홀드 상태를 해제합니다.
        if (boundKey != AttackKey)
        {
            if (IsActionHeld) Released?.Invoke();
            IsActionHeld = false;
            BuildActions();
            Action.Enable();
            return;
        }

        if (Action.WasPressedThisFrame()) // KeyDown 감지: 누르기 시작한 프레임입니다.
        {
            IsActionHeld = true; // 누르고 있는 상태로 기록합니다.
            Debug.Log($"[Key DOWN] {Action.activeControl?.path}");
            Pressed?.Invoke();
        }

        if (Action.WasReleasedThisFrame()) // KeyUp 감지: 이번 프레임에 버튼이 해제되었는지 확인합니다.
        {
            IsActionHeld = false; // 더 이상 누르고 있지 않은 상태로 기록합니다.
            Debug.Log($"[Key UP] {Action.activeControl?.path}");
            Released?.Invoke();
        }
    }

    // 컴포넌트가 제거될 때 입력 객체가 사용하는 자원을 정리합니다.
    private void OnDestroy()
    {
        Action?.Dispose(); // 객체가 있을 때만 정리합니다. Disable과 달리 다시 쓰려면 새로 만들어야 합니다.
    }

    // 이 클래스에서 정한 일반 함수입니다. Unity가 이름을 보고 자동 호출하지는 않습니다.
    // 키보드와 게임패드가 동일한 조작을 발생시키도록 입력 객체와 연결 정보를 만듭니다.
    private void BuildActions()
    {
        Action?.Dispose(); // 다시 만드는 경우 기존 입력 객체의 자원부터 정리합니다.

        // "Attack"은 액션 이름이고, Button은 버튼처럼 눌림/해제를 다루겠다는 입력 유형입니다.
        // 이름을 Attack으로 지었다고 공격 동작이 자동 실행되는 것은 아닙니다.
        Action = new InputAction("Attack", InputActionType.Button);

        // 저장된 키 종류를 입력 시스템의 경로 문자열로 바꿔 이 액션에 연결합니다.
        boundKey = AttackKey; // 현재 적용한 키를 기억하여 변경 여부를 감지합니다.
        Action.AddBinding(KeyboardBindingSettings.GetPath(boundKey));
        // 같은 액션에 게임패드 아래쪽 버튼도 연결합니다. 키보드와 게임패드 어느 쪽으로든 조작 가능합니다.
        Action.AddBinding("<Gamepad>/buttonSouth");
    }

}
