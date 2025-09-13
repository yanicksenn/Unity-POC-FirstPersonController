using System;
using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class Hand : MonoBehaviour {
    [SerializeField] private Transform idleAnchor;
    [SerializeField] private Transform swingingAnchor;
    
    [SerializeField] private float maxGrabbingDistance = 3;
    [SerializeField] private float maxSwingDistance;
    [SerializeField] private float autoReleaseVelocityThreshold = 8;

    [SerializeField] private Looker looker;

    private readonly Vector3Pid _velocityPidController = 
        new(5f, 0.2f, 0.25f, new Vector3Pid.Clamped(3f));

    private IState _currentState = new Idle();
    
    public bool IsIdle => _currentState is Idle;
    public bool IsHolding => _currentState is Holding;
    public bool IsSwinging => _currentState is Swinging;

    [CanBeNull]
    public Grabbable Grabbable => _currentState switch {
        Idle => null,
        Holding holding => holding.Grabbable,
        Swinging swinging => swinging.Grabbable,
        _ => throw new ArgumentNullException(nameof(_currentState))
    };

    private void FixedUpdate() {
        switch (_currentState) {
            case Idle:
                MoveHandToIdlePosition();
                break;
            case Holding grabbing:
                MoveGrabbableTowardsHand(grabbing.Grabbable);
                RotateGrabbableInHand(grabbing.Grabbable, grabbing.RotationOffset);
                MoveHandToIdlePosition();
                break;
            case Swinging swinging:
                MoveGrabbableTowardsHand(swinging.Grabbable);
                RotateGrabbableInHand(swinging.Grabbable, swinging.RotationOffset);
                MoveHandToSwingingPosition(swinging.GetSwingStrength());
                AutoRelease(swinging.Grabbable, swinging.GetSwingStrength());
                break;
        }
    }

    public void Grab() {
        if (_currentState is Holding) return;
        var origin = looker.LookOrigin;
        var direction = looker.LookDirection;
    
        if (!Physics.Raycast(origin, direction, out var hit, maxGrabbingDistance)) {
            return;
        }
    
        if (!hit.transform.TryGetComponent(out Grabbable grabbable)) {
            return;
        }
    
        _currentState = new Holding(grabbable, Quaternion.Inverse(looker.transform.rotation) * grabbable.transform.rotation);
        grabbable.Grab(this);
    }

    public void BeginSwinging() {
        if (_currentState is not Holding grabbing) return;
        _currentState = new Swinging(
            grabbing.Grabbable,
            grabbing.RotationOffset,
            maxSwingDistance, 
            0);
    }

    public void SwingHand(Vector3 handPositionDelta) {
        if (_currentState is not Swinging swinging) return;
        var newSwingDistance = Mathf.Clamp(swinging.CurrentSwingDistance + -handPositionDelta.y, 0, maxSwingDistance);
        _currentState = new Swinging(
            swinging.Grabbable,
            swinging.RotationOffset,
            swinging.MaxSwingDistance,
            newSwingDistance
        );
    }
    
    public void EndSwinging() {
        if (_currentState is not Swinging swinging) return;
        _currentState = new Holding(
            swinging.Grabbable, 
            swinging.RotationOffset);
    }

    public void Release() {
        if (_currentState is not IGrabbing grabbing) return;
        _currentState = new Idle();
        grabbing.Grabbable.Release();
        _velocityPidController.Reset();
    }

    private void MoveGrabbableTowardsHand(Grabbable grabbable) {
        var currentPosition = grabbable.transform.position;
        _velocityPidController.Calculate(currentPosition, transform.position, Time.fixedDeltaTime);
        grabbable.Rigidbody.AddForce(_velocityPidController.Value, ForceMode.VelocityChange);
    }

    private void RotateGrabbableInHand(Grabbable grabbable, Quaternion rotationOffset) {
        grabbable.transform.rotation = looker.transform.rotation * rotationOffset;
    }

    private void MoveHandToIdlePosition() {
        transform.position = Vector3.Lerp(
            transform.position, idleAnchor.position, 0.5f);
    }


    private void MoveHandToSwingingPosition(float swingStrength) {
        transform.position = Vector3.Lerp(
            idleAnchor.position, swingingAnchor.position, swingStrength);
    }

    private void AutoRelease(Grabbable grabbable, float swingStrength) {
        var sqrThreshold = autoReleaseVelocityThreshold * autoReleaseVelocityThreshold;
        if (grabbable.InterpolatedVelocity.sqrMagnitude > sqrThreshold && swingStrength < Mathf.Epsilon) {
            Release();
        }
    }
    
    private interface IState {}

    private interface IGrabbing : IState {
        Grabbable Grabbable { get; }
    }
    
    private class Idle : IState {}

    private class Holding : IGrabbing {
        public Grabbable Grabbable { get; }
        public Quaternion RotationOffset { get; }
        public Holding(Grabbable grabbable, Quaternion rotationOffset)
        {
            Grabbable = grabbable;
            RotationOffset = rotationOffset;
        }
    }
    private class Swinging : IGrabbing {
        public Grabbable Grabbable { get; }
        public Quaternion RotationOffset { get; }
        public float MaxSwingDistance { get; }
        public float CurrentSwingDistance { get; }

        public Swinging(Grabbable grabbable, Quaternion rotationOffset, float maxSwingDistance, float currentSwingDistance) {
            Grabbable = grabbable;
            RotationOffset = rotationOffset;
            MaxSwingDistance = maxSwingDistance;
            CurrentSwingDistance = currentSwingDistance;
        }

        public float GetSwingStrength() {
            if (CurrentSwingDistance == 0) return 0;
            return Mathf.Clamp01(CurrentSwingDistance / MaxSwingDistance);
        }
    }
}