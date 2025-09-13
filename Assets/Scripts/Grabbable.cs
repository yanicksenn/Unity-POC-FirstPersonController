using UnityEngine;

[DisallowMultipleComponent,
 RequireComponent(typeof(Rigidbody)),
 RequireComponent(typeof(Collider))]
public class Grabbable : MonoBehaviour {
    [SerializeField, Tooltip("The maximum speed at which the grabbable can be released in units per second.")] 
    private float maxReleaseVelocity = 15f;
    
    private IState currentState = new Idle();
    private Rigidbody rigidbody;
    private Collider collider;

    public Rigidbody Rigidbody => rigidbody;
    public Collider Collider => collider;

    private void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }


    public void Grab(Hand hand) {
        if (currentState is Grabbed) {
            return;
        }

        currentState = new Grabbed(hand, 
            RigidbodySnapshot.From(rigidbody),  
            ColliderSnapshot.From(collider));
        rigidbody.useGravity = false;
        rigidbody.linearDamping = 10f;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        collider.excludeLayers |= 1 << LayerMask.NameToLayer("Player");
    }

    public void Release() {
        if (currentState is not Grabbed grabbed) {
            return;
        }
        
        currentState = new Idle();
        grabbed.RigidbodySnapshot.ApplyTo(rigidbody);
        grabbed.ColliderSnapshot.ApplyTo(collider);
    }
    
    public void ReleaseWithVelocity(Vector3 velocity) {
        Release();
        rigidbody.linearVelocity = velocity;
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
        public bool useGravity;
        public float linearDamping;
        public RigidbodyConstraints constraints;
        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;

        public static RigidbodySnapshot From(Rigidbody rigidbody) {
            return new RigidbodySnapshot() {
                useGravity = rigidbody.useGravity,
                linearDamping = rigidbody.linearDamping,
                constraints = rigidbody.constraints
            };
        }

        public void ApplyTo(Rigidbody rigidbody) {
            rigidbody.useGravity = useGravity;
            rigidbody.linearDamping = linearDamping;
            rigidbody.constraints = constraints;
            rigidbody.interpolation = interpolation;
            rigidbody.collisionDetectionMode = collisionDetectionMode;
        }
    }

    private struct ColliderSnapshot {
        public LayerMask includeLayers;
        public LayerMask excludeLayers;

        public static ColliderSnapshot From(Collider collider) {
            return new ColliderSnapshot() {
                includeLayers = collider.includeLayers,
                excludeLayers = collider.excludeLayers,
            };
        }

        public void ApplyTo(Collider collider) {
            collider.includeLayers = includeLayers;
            collider.excludeLayers = excludeLayers;
        }
    }
}