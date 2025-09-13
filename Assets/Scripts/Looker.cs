using UnityEngine;

[DisallowMultipleComponent]
public class Looker : MonoBehaviour
{
    [SerializeField]
    private Camera camera;
        
    [SerializeField]
    private float lookSensitivity;

    public Vector2 LookInput { get; set; }
    public Vector3 LookOrigin => GetLookOrigin();
    public Vector3 LookDirection => GetLookDirection();

    private Vector2 lookRotation;

    private void Update() {
        var mouseX = LookInput.x * lookSensitivity * Time.deltaTime;
        var mouseY = LookInput.y * lookSensitivity * Time.deltaTime;
        lookRotation = new Vector2(
            Mathf.Clamp(lookRotation.x - mouseY, -60f, 60f),
            lookRotation.y + mouseX
        );
        camera.transform.localRotation = Quaternion.Euler(
            lookRotation.x,
            lookRotation.y,
            0f
        );
    }

    private Vector3 GetLookOrigin() {
        return camera.transform.position;
    }

    private Vector3 GetLookDirection() {
        return camera.transform.forward;
    }
}