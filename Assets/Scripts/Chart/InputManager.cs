using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event Action JumpPressed;
    public event Action AttackPressed;
    public event Action ActionPressed;
    public event Action ActionReleased;

    public bool IsActionHeld { get; private set; }

    [Header("Keyboard Bindings")]
    [SerializeField] private Key keyboardJumpKey = Key.Z;
    [SerializeField] private Key keyboardAttackKey = Key.X;

    private InputAction jumpAction;
    private InputAction attackAction;

    private void Awake()
    {
        BuildActions();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        bool wasEnabled = jumpAction != null && jumpAction.enabled;
        BuildActions();

        if (wasEnabled)
        {
            jumpAction.Enable();
            attackAction.Enable();
        }
    }

    private void OnEnable()
    {
        jumpAction.Enable();
        attackAction.Enable();
    }

    private void OnDisable()
    {
        jumpAction.Disable();
        attackAction.Disable();
    }

    private void Update()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            Debug.Log($"[JUMP] {jumpAction.activeControl?.path}");
            JumpPressed?.Invoke();
        }

        if (attackAction.WasPressedThisFrame())
        {
            IsActionHeld = true;
            Debug.Log($"[ATTACK DOWN] {attackAction.activeControl?.path}");
            AttackPressed?.Invoke();
            ActionPressed?.Invoke();
        }

        if (attackAction.WasReleasedThisFrame())
        {
            IsActionHeld = false;
            Debug.Log($"[ATTACK UP] {attackAction.activeControl?.path}");
            ActionReleased?.Invoke();
        }
    }

    private void OnDestroy()
    {
        jumpAction?.Dispose();
        attackAction?.Dispose();
    }

    private void BuildActions()
    {
        jumpAction?.Dispose();
        attackAction?.Dispose();

        jumpAction = new InputAction("Jump", InputActionType.Button);
        attackAction = new InputAction("Attack", InputActionType.Button);

        jumpAction.AddBinding(GetKeyboardBindingPath(keyboardJumpKey));
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        attackAction.AddBinding(GetKeyboardBindingPath(keyboardAttackKey));
        attackAction.AddBinding("<Gamepad>/buttonEast");
    }

    private static string GetKeyboardBindingPath(Key key)
    {
        return $"<Keyboard>/{key.ToString().ToLowerInvariant()}";
    }
}
