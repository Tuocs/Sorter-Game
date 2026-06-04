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
        if (!IsOwner) return;
        _lookInput = value.Get<Vector2>();
    }

    private void LateUpdate()
    {
        if (!IsOwner || _mainCameraTransform == null) return;

        float mouseX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, topClamp, bottomClamp);
        _cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
