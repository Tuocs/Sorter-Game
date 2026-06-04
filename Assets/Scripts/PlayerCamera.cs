using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Connection;
using FishNet.Object;

public class PlayerCamera : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _cameraHolder;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 15f;
    [SerializeField] private float topClamp = -90f;
    [SerializeField] private float bottomClamp = 90f;

    private Vector2 _lookInput;
    private float _xRotation = 0f;
    private Transform _mainCameraTransform;

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);

        if (Camera.main == null)
            return;

        // If we are the new network owner, lock the camera and mouse cursor locally
        if (IsOwner)
        {
            _mainCameraTransform = Camera.main.transform;
            
            _mainCameraTransform.SetPositionAndRotation(_cameraHolder.position, _cameraHolder.rotation);
            _mainCameraTransform.SetParent(_cameraHolder);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public void OnLook(InputValue value)
    {
        // Safety check to ensure remote proxy clones do not process local mouse data
        if (!IsOwner) return;

        // Store the incoming Vector2 mouse delta frame data
        _lookInput = value.Get<Vector2>();
    }

    private void LateUpdate()
    {
        // Only run rotation math on the client who owns this specific player instance
        if (!IsOwner || _mainCameraTransform == null) 
            return;

        // Process horizontal and vertical calculations using the cached input values
        float mouseX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * mouseSensitivity * Time.deltaTime;

        // 1. Vertical Rotation (Look Up/Down) - target the localized camera holder
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, topClamp, bottomClamp);
        _cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // 2. Horizontal Rotation (Look Left/Right) - pivot the entire player root structure
        transform.Rotate(Vector3.up * mouseX);
    }
}
