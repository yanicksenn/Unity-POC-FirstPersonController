# POC First Person Controller

A basic first person controller that can pick up and throw objects.

## Basic Idea

1. Wire mouse position delta data via the InputSystem to the player controller
1. Wire WASD data via the InputSystem to the player controller
1. Rotate the camera around the mouse delta
1. Determine the XZ movement direction based on the camera rotation
1. Use a rigidbody to move the player around
