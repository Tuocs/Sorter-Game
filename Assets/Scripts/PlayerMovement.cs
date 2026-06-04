using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = 9.81f;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    private bool jumpRequested;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 verticalVelocity;
    private Transform cameraTransform;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Ensure only the local owner processes input
        if (base.IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
            
            // Cache the main camera for direction calculations
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
        else
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // This function hooks into the New Input System's "Send Messages" setup
    public void OnMove(InputValue value)
    {
        if (!base.IsOwner) return;

        // Read the 2D vector value using InputValue
        moveInput = value.Get<Vector2>();
    }

    // This function hooks into the New Input System's "Send Messages" setup for Jump
    public void OnJump(InputValue value)
    {
        if (!base.IsOwner) return;

        // Trigger the jump if the button is pressed down
        if (value.isPressed && controller.isGrounded)
        {
            jumpRequested = true;
        }
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Apply Gravity and Handle Jumping
        if (controller.isGrounded)
        {
            // Small negative value to keep player stuck to slopes/ground
            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -0.5f; 
            }

            // Execute the jump request
            if (jumpRequested)
            {
                // Kinematic formula: v = sqrt(2 * g * h)
                verticalVelocity.y = Mathf.Sqrt(2f * gravity * jumpHeight);
                jumpRequested = false;
            }
        }
        else
        {
            // Apply gravity over time when in mid-air
            verticalVelocity.y -= gravity * Time.deltaTime;
            
            // Clear accidental jump inputs if hit in mid-air
            jumpRequested = false;
        }

        // Calculate direction relative to the camera look angle
        Vector3 moveDirection = Vector3.zero;

        if (cameraTransform != null)
        {
            // Get camera vectors
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            // Flatten vectors on the Y axis to stay on the ground plane
            camForward.y = 0f;
            camRight.y = 0f;
            
            camForward.Normalize();
            camRight.Normalize();

            // Combine vectors based on player input
            moveDirection = (camForward * moveInput.y) + (camRight * moveInput.x);
        }
        else
        {
            // Fallback to world space if no camera is found
            moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        }
        
        // Move the CharacterController
        Vector3 finalMovement = (moveDirection * moveSpeed) + verticalVelocity;
        controller.Move(finalMovement * Time.deltaTime);
    }
}