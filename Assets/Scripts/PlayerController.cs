using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent, 
 RequireComponent(typeof(Looker)),
 RequireComponent(typeof(Mover))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions {
    
    [SerializeField]
    private Hand hand;
    
    private InputSystem_Actions actions;
    private Looker looker;
    private Mover mover;
    private bool isSwinging;

    private void Awake() {
        actions = new InputSystem_Actions();
        actions.Player.SetCallbacks(this);
        looker = GetComponent<Looker>();
        mover = GetComponent<Mover>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() {
        actions.Player.Enable();
    }

    private void OnDisable() {
        actions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context) {
        mover.MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        if (hand.IsSwinging) {
            looker.LookInput = Vector2.zero;
            hand.SwingHand(context.ReadValue<Vector2>());
        } else {
            looker.LookInput = context.ReadValue<Vector2>();
        }
    }

    public void OnSprint(InputAction.CallbackContext context) {
        mover.IsRunning = context.ReadValueAsButton();
    }

    public void OnGrab(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            hand.Grab();
        }
    }

    public void OnRelease(InputAction.CallbackContext context) {
        if (context.phase != InputActionPhase.Started) return;
        if (hand.IsSwinging) {
            hand.EndSwinging();
        } else {
            hand.Release();
        }
    }

    public void OnSwing(InputAction.CallbackContext context) {
        switch (context.phase) {
            case InputActionPhase.Performed:
                hand.BeginSwinging();
                break;
            case InputActionPhase.Canceled:
                if (hand.Grabbable) {
                    if (hand.Grabbable.InterpolatedVelocity.sqrMagnitude < 4 * 4) {
                        hand.EndSwinging();
                    } else {
                        hand.Release();
                    }
                }
                break;
        }
    }
}
