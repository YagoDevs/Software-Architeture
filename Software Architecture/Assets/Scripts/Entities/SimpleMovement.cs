using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float cameraSensitivity = 3f;
    
    private float cameraRotationX = 0f;
    private float cameraRotationY = 20f;

    void Update()
    {
        HandleCameraRotation();
        HandleMovement();
        UpdateCameraPosition();
    }
    
    void HandleCameraRotation()
    {
        // Rotate camera only with right button pressed
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            cameraRotationX += mouseDelta.x * cameraSensitivity * 0.1f;
            cameraRotationY -= mouseDelta.y * cameraSensitivity * 0.1f;
            cameraRotationY = Mathf.Clamp(cameraRotationY, 5f, 80f);
        }
    }
    
    void HandleMovement()
    {
        // Get input
        float horizontal = 0f;
        float vertical = 0f;
        
        if (Keyboard.current.wKey.isPressed) vertical = 1f;
        if (Keyboard.current.sKey.isPressed) vertical = -1f;
        if (Keyboard.current.aKey.isPressed) horizontal = -1f;
        if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        
        // Character always looks in the horizontal direction of the camera
        Quaternion targetRotation = Quaternion.Euler(0, cameraRotationX, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        if (horizontal != 0 || vertical != 0)
        {
            // Direction based on camera
            Vector3 forward = Quaternion.Euler(0, cameraRotationX, 0) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0, cameraRotationX, 0) * Vector3.right;
            
            Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
            
            // Move the character
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }
    
    void UpdateCameraPosition()
    {
        if (cameraTransform == null) return;
        
        // Camera position behind the player
        Quaternion rotation = Quaternion.Euler(cameraRotationY, cameraRotationX, 0);
        Vector3 offset = rotation * new Vector3(0, cameraHeight, -cameraDistance);
        
        cameraTransform.position = transform.position + offset;
        cameraTransform.LookAt(transform.position + Vector3.up * cameraHeight);
    }
}
