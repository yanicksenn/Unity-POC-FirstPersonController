using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent, 
 RequireComponent(typeof(Looker)),
 RequireComponent(typeof(Hand)),
 RequireComponent(typeof(Mover))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions {
    private InputSystem_Actions _actions;
    private Looker _looker;
    private Hand _hand;
    private Mover _mover;

    private void Awake() {
        _actions = new InputSystem_Actions();
        _actions.Player.SetCallbacks(this);
        _looker = GetComponent<Looker>();
        _hand = GetComponent<Hand>();
        _mover = GetComponent<Mover>();
    }

    private void OnEnable() {
        _actions.Player.Enable();
    }

    private void OnDisable() {
        _actions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context) {
        _mover.MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        _looker.LookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context) {
        _mover.IsRunning = context.ReadValueAsButton();
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            _mover.Jump();
        }
    }

    public void OnGrab(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            _hand.Grab();
        }
    }

    public void OnRelease(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            _hand.Release();
        }
    }

    public void OnThrow(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            _hand.Throw();
        }
    }
}
