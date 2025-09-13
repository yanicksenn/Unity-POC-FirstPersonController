using System;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent,
 RequireComponent(typeof(Rigidbody)),
 RequireComponent(typeof(Looker))]
public class Mover : MonoBehaviour {
    
    [FormerlySerializedAs("walkingSpeed")] [SerializeField]
    private float walkingForce = 3500f;
    [FormerlySerializedAs("runningSpeed")] [SerializeField]
    private float runningForce = 5000f;
    [SerializeField]
    private float jumpForce = 15000;
    
    public Vector2 MoveInput { get; set; }
    public bool IsRunning { get; set; }
    public bool IsGrounded => Mathf.Abs(_rigidbody.linearVelocity.y) < Mathf.Epsilon;

    private Rigidbody _rigidbody;
    private Looker _looker;
    private bool _isRunning;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _looker = GetComponent<Looker>();

        _rigidbody.linearDamping = 10f;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate() {
        if (IsGrounded) {
            _rigidbody.linearDamping = 10f;
            ApplyMovementInDirection();
        } else {
            _rigidbody.linearDamping = 0f;
        }
    }

    public void Jump() {
        if (!IsGrounded) return;
        _rigidbody.AddForce(Vector3.up * jumpForce);
    }

    private void ApplyMovementInDirection() {
        var lookDirection = _looker.LookDirection;
        var forward = new Vector3(lookDirection.x, 0f, lookDirection.z).normalized;
        var right = new Vector3(forward.z, 0, -forward.x);
        var moveDirection = (forward * MoveInput.y + right * MoveInput.x).normalized;
        var speed = IsRunning ? runningForce : walkingForce;
        _rigidbody.AddForce(speed * moveDirection);
    }
}