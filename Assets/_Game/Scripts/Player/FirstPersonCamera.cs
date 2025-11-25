using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the PlayerInputManager here")]
    public PlayerInputManager inputManager;
    [Tooltip("The main player body to rotate Y-axis")]
    public Transform playerBody;

    [Header("Settings")]
    public float mouseSensitivity = 15f;
    public float topClamp = 89f;
    public float bottomClamp = -89f;

    private float xRotation = 0f;

    private void Start()
    {
        // Lock cursor for FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (inputManager == null) return;

        // Get raw input
        Vector2 look = inputManager.LookInput;

        // Apply sensitivity and DeltaTime
        // Note: New Input System mouse delta is already frame-rate independent usually, 
        // but multiplying by scaling factor is good.
        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        // Calculate Up/Down rotation (Pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);

        // Rotate Camera (Pitch)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate Player Body (Yaw)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
