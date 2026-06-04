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

    [Header("Pickup")]
    public float maxDistance = 50f;

    private CharacterController controller;
    private Vector2 moveInput;
    private float verticalVelocity;
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

    public void OnMove(InputValue value)
    {
        if (!base.IsOwner) return;
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!base.IsOwner) return;
        if (value.isPressed && controller.isGrounded)
        {
            jumpRequested = true;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!base.IsOwner) return;
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            string objectName = hitObject.name;
            string objectTag = hitObject.tag;
            
            Debug.Log($"Hit object: {objectName} with tag: {objectTag}");
            Debug.DrawLine(ray.origin, hit.point, Color.green);
        }

        if (hit.collider.TryGetComponent<Scroll>(out Scroll scroll))
        {
            scroll.RpcPickup();
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
            if (verticalVelocity < 0)
            {
                verticalVelocity = -0.5f; 
            }
            if (jumpRequested)
            {
                // Kinematic formula: v = sqrt(2 * g * h)
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
                jumpRequested = false;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            jumpRequested = false;
        }

        // Calculate direction relative to the camera look angle
        Vector3 moveDirection = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * moveInput.y) + (camRight * moveInput.x);
        }
        else
        {
            // Fallback to world space if no camera is found
            moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        }
        
        // Move the CharacterController
        Vector3 finalMovement = (moveDirection * moveSpeed) + new Vector3(0,verticalVelocity,0);
        controller.Move(finalMovement * Time.deltaTime);
    }
}