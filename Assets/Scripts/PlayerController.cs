using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent, 
 RequireComponent(typeof(Looker)),
 RequireComponent(typeof(Hand)),
 RequireComponent(typeof(Mover))]
public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions {
    private InputSystem_Actions actions;
    private Looker looker;
    private Hand hand;
    private Mover mover;
    private bool isSwinging;

    private void Awake() {
        actions = new InputSystem_Actions();
        actions.Player.SetCallbacks(this);
        looker = GetComponent<Looker>();
        hand = GetComponent<Hand>();
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
        looker.LookInput = context.ReadValue<Vector2>();
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
        if (context.phase == InputActionPhase.Started) {
            hand.Release();
        }
    }

    public void OnSwing(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            hand.Throw();
        }
    }
}
