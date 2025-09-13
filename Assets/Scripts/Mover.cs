using System;
using UnityEngine;

[DisallowMultipleComponent,
 RequireComponent(typeof(Looker))]
public class Mover : MonoBehaviour {
    
    [SerializeField]
    private CharacterController characterController;
    
    [SerializeField]
    private float walkingSpeed = 4f;
    [SerializeField]
    private float runningSpeed = 7f;
    
    public Vector2 MoveInput { get; set; }
    public bool IsRunning { get; set; }

    private Looker looker;
    private bool isRunning;

    private void Awake() {
        looker = GetComponent<Looker>();
    }

    private void Update() {
        var lookDirection = looker.LookDirection;
        var forward = new Vector3(lookDirection.x, 0f, lookDirection.z).normalized;
        var right = new Vector3(forward.z, 0, -forward.x);
        var moveDirection = forward * MoveInput.y + right * MoveInput.x;
        var speed = isRunning ? runningSpeed : walkingSpeed;
        characterController.Move(Time.deltaTime * speed * moveDirection);
    }
}