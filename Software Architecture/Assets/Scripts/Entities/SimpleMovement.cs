/*
This script is used to move the player (WASD) and rotate the camera with the mouse. It also drives the Animator 'Speed' parameter when available.
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 9f;
    public float rotationSpeed = 10f;
    [Header("Physics (CharacterController)")]
    public CharacterController controller;
    public float gravity = -20f;
    float verticalVelocity;
    
    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float cameraSensitivity = 3f;
    public bool invertY = false;
    public bool lockCursorOnStart = true;
    public Key unlockCursorKey = Key.Escape;

    [Header("Animation (optional)")]
    public Animator animator; // assign the child (Rogue_Hooded) Animator here
    public string animSpeedParam = "Speed"; // 0 = idle, 1 = run
    public float animSpeedDamp = 10f;
    public bool logAnimationWarnings = true;
    
    private float cameraRotationX = 0f;
    private float cameraRotationY = 20f;
    float move01;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            // Avoid cases where animation stops due to culling / settings.
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;

            if (logAnimationWarnings)
            {
                if (animator.runtimeAnimatorController == null)
                    Debug.LogWarning("SimpleMovement: Animator found, but no RuntimeAnimatorController assigned on it.");
                if (!animator.isActiveAndEnabled)
                    Debug.LogWarning("SimpleMovement: Animator found, but it is not active/enabled.");
            }
        }

        if (lockCursorOnStart)
            LockCursor(true);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current[unlockCursorKey].wasPressedThisFrame)
            LockCursor(false);

        HandleCameraRotation();
        HandleMovement();
        UpdateCameraPosition();
        UpdateAnimator();
    }
    
    void HandleCameraRotation()
    {
        // Rotate camera with mouse movement (no need to hold buttons)
        // this is for the inventory UI, so we can rotate the camera while the inventory is open
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (Mouse.current == null) return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            cameraRotationX += mouseDelta.x * cameraSensitivity * 0.1f;

        float yDelta = mouseDelta.y * cameraSensitivity * 0.1f;
        cameraRotationY += invertY ? yDelta : -yDelta;
            cameraRotationY = Mathf.Clamp(cameraRotationY, 5f, 80f);
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

        // For animation: 0 idle, 1 moving
        move01 = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);
        
        // Character always looks in the horizontal direction of the camera
        Quaternion targetRotation = Quaternion.Euler(0, cameraRotationX, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Direction based on camera
        Vector3 forward = Quaternion.Euler(0, cameraRotationX, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, cameraRotationX, 0) * Vector3.right;
        Vector3 moveDirection = (forward * vertical + right * horizontal);
        if (moveDirection.sqrMagnitude > 1f) moveDirection.Normalize();

        // Gravity + grounded handling
        if (controller != null)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -1f; // keep grounded
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            // Fallback (shouldn't happen): old behavior
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
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

    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // this is for the animation, so we can animate the player's movement
    void UpdateAnimator()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(animSpeedParam)) return;

        if (logAnimationWarnings && !HasParameter(animator, animSpeedParam))
        {
            logAnimationWarnings = false; // warn once
            Debug.LogWarning($"SimpleMovement: Animator is set but parameter '{animSpeedParam}' was not found on controller '{animator.runtimeAnimatorController?.name}'. Check your Animator Controller parameters.");
            return;
        }

        float current = animator.GetFloat(animSpeedParam);
        float next = Mathf.Lerp(current, move01, animSpeedDamp * Time.deltaTime);
        animator.SetFloat(animSpeedParam, next);
    }

    // this is for the animation, so we can check if the parameter exists
    static bool HasParameter(Animator anim, string paramName)
    {
        if (anim == null) return false;
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].name == paramName)
                return true;
        }
        return false;
    }
}
