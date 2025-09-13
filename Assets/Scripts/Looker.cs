using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Looker : MonoBehaviour {
    [SerializeField]
    private Camera camera;
        
    [SerializeField]
    private float lookSensitivity;

    public Vector2 LookInput { get; set; }
    public Vector3 LookOrigin => GetLookOrigin();
    public Vector3 LookDirection => GetLookDirection();

    private Vector2 _lookRotation;

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        var mouseX = LookInput.x * lookSensitivity * Time.deltaTime;
        var mouseY = LookInput.y * lookSensitivity * Time.deltaTime;
        _lookRotation = new Vector2(
            Mathf.Clamp(_lookRotation.x - mouseY, -60f, 60f),
            _lookRotation.y + mouseX
        );
        camera.transform.localRotation = Quaternion.Euler(
            _lookRotation.x,
            0f,
            0f
        );
        transform.localRotation = Quaternion.Euler(
            0f,
            _lookRotation.y,
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