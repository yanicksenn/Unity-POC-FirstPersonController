using UnityEngine;

[DisallowMultipleComponent,
 RequireComponent(typeof(Rigidbody)),
 RequireComponent(typeof(Collider))]
public class Grabbable : MonoBehaviour {
    [SerializeField, Tooltip("The maximum speed at which the grabbable can be released in units per second.")] 
    private float maxReleaseVelocity = 15f;
    
    private IState _currentState = new Idle();
    private Rigidbody _rigidbody;
    private Collider _collider;

    public Rigidbody Rigidbody => _rigidbody;
    public Collider Collider => _collider;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }


    public void Grab(Hand hand) {
        if (_currentState is Grabbed) {
            return;
        }

        _currentState = new Grabbed(hand, 
            RigidbodySnapshot.From(_rigidbody),  
            ColliderSnapshot.From(_collider));
        _rigidbody.useGravity = false;
        _rigidbody.linearDamping = 10f;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _collider.excludeLayers |= 1 << LayerMask.NameToLayer("Player");
    }

    public void Release() {
        if (_currentState is not Grabbed grabbed) {
            return;
        }
        
        _currentState = new Idle();
        grabbed.RigidbodySnapshot.ApplyTo(_rigidbody);
        grabbed.ColliderSnapshot.ApplyTo(_collider);
    }
    
    public void ReleaseWithForce(Vector3 force) {
        Release();
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.AddForce(force);
    }

    private interface IState {}
    private class Idle : IState {}

    private class Grabbed : IState {
        public Hand Hand { get; }
        public RigidbodySnapshot RigidbodySnapshot { get; }
        public ColliderSnapshot ColliderSnapshot { get; }
        public Grabbed(Hand hand, RigidbodySnapshot rigidbodySnapshot, ColliderSnapshot colliderSnapshot) {
            Hand = hand;
            RigidbodySnapshot = rigidbodySnapshot;
            ColliderSnapshot = colliderSnapshot;
        }
    }

    private struct RigidbodySnapshot {
        private bool _useGravity;
        private float _linearDamping;
        private RigidbodyConstraints _constraints;
        private RigidbodyInterpolation _interpolation;
        private CollisionDetectionMode _collisionDetectionMode;

        public static RigidbodySnapshot From(Rigidbody rigidbody) {
            return new RigidbodySnapshot() {
                _useGravity = rigidbody.useGravity,
                _linearDamping = rigidbody.linearDamping,
                _constraints = rigidbody.constraints
            };
        }

        public void ApplyTo(Rigidbody rigidbody) {
            rigidbody.useGravity = _useGravity;
            rigidbody.linearDamping = _linearDamping;
            rigidbody.constraints = _constraints;
            rigidbody.interpolation = _interpolation;
            rigidbody.collisionDetectionMode = _collisionDetectionMode;
        }
    }

    private struct ColliderSnapshot {
        private LayerMask _includeLayers;
        private LayerMask _excludeLayers;

        public static ColliderSnapshot From(Collider collider) {
            return new ColliderSnapshot() {
                _includeLayers = collider.includeLayers,
                _excludeLayers = collider.excludeLayers,
            };
        }

        public void ApplyTo(Collider collider) {
            collider.includeLayers = _includeLayers;
            collider.excludeLayers = _excludeLayers;
        }
    }
}