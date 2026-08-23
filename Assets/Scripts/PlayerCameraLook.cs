using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraLook : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.08f;
    [SerializeField] private float gamepadSensitivity = 120f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 60f;
    [SerializeField] private bool invertY = false;

    private Vector2 lookInput;
    private float yaw;
    private float pitch;

    private void OnLook(InputValue input)
    {
        lookInput = input.Get<Vector2>();
    }

    private void LateUpdate()
    {
        if (cameraPivot == null)
        {
            return;
        }

        bool usingMouse = Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f;
        float sensitivity = usingMouse ? mouseSensitivity : gamepadSensitivity * Time.deltaTime;
        float yDirection = invertY ? 1f : -1f;

        yaw += lookInput.x * sensitivity;
        pitch += lookInput.y * sensitivity * yDirection;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}