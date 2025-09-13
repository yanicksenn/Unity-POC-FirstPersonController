# POC First Person Controller

![recording](https://raw.githubusercontent.com/yanicksenn/Unity-POC-FirstPersonController/2a5838c6fb8f481a593e2bc79c8bfd65d254981c/Img/recording.gif)

A basic first person controller that can pick up and throw objects.

## Basic Idea

1. Wire mouse position delta data via the InputSystem to the player controller
1. Wire WASD data via the InputSystem to the player controller
1. Rotate the camera around the mouse delta
1. Determine the XZ movement direction based on the camera rotation
1. Use a rigidbody to move the player around
