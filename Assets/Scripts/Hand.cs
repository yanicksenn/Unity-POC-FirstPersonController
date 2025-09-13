using System;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(Looker))]
public class Hand : MonoBehaviour {
    [SerializeField] private Transform handAnchor;
    [SerializeField] private float maxGrabbingDistance = 3;
    [SerializeField] private float throwForce = 80;

    private readonly Vector3Pid _velocityPidController = 
        new(5f, 0.2f, 0.25f, new Vector3Pid.Unclamped());

    private IState _currentState = new Idle();
    private Looker _looker;

    private void Awake() {
        _looker = GetComponent<Looker>();
    }

    private void FixedUpdate() {
        switch (_currentState) {
            case Holding grabbing:
                MoveGrabbableTowardsHand(grabbing.Grabbable);
                RotateGrabbableInHand(grabbing.Grabbable, grabbing.RotationOffset);
                break;
        }
    }

    public void Grab() {
        if (_currentState is Holding) return;
        var origin = _looker.LookOrigin;
        var direction = _looker.LookDirection;
    
        if (!Physics.Raycast(origin, direction, out var hit, maxGrabbingDistance)) {
            return;
        }
    
        if (!hit.transform.TryGetComponent(out Grabbable grabbable)) {
            return;
        }
    
        _currentState = new Holding(grabbable, Quaternion.Inverse(_looker.transform.rotation) * grabbable.transform.rotation);
        grabbable.Grab(this);
    }

    public void Release() {
        DropWithVelocity(Vector3.zero);
    }

    public void Throw() {
        DropWithVelocity(throwForce * _looker.LookDirection);
    }

    private void DropWithVelocity(Vector3 force) {
        if (_currentState is not Holding holding) return;
        _currentState = new Idle();
        holding.Grabbable.ReleaseWithForce(force);
        _velocityPidController.Reset();
    }

    private void MoveGrabbableTowardsHand(Grabbable grabbable) {
        var currentPosition = grabbable.transform.position;
        _velocityPidController.Calculate(currentPosition, handAnchor.position, Time.fixedDeltaTime);
        grabbable.Rigidbody.AddForce(_velocityPidController.Value, ForceMode.VelocityChange);
    }

    private void RotateGrabbableInHand(Grabbable grabbable, Quaternion rotationOffset) {
        grabbable.transform.rotation = _looker.transform.rotation * rotationOffset;
    }
    
    private interface IState {}
    
    private class Idle : IState {}

    private class Holding : IState {
        public Grabbable Grabbable { get; }
        public Quaternion RotationOffset { get; }
        public Holding(Grabbable grabbable, Quaternion rotationOffset) {
            Grabbable = grabbable;
            RotationOffset = rotationOffset;
        }
    }
}