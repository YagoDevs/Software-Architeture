using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 700f;

    void Update()
    {
        float moveForward = 0f;
        float turn = 0f;

        // Get keyboard input for movement (W/S or Up/Down Arrow)
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveForward = 1f;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveForward = -1f;

        // Get keyboard input for rotation (A/D or Left/Right Arrow)
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            turn = -1f;
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            turn = 1f;

        // Move character forward/backward
        transform.Translate(0, 0, moveForward * speed * Time.deltaTime);

        // Rotate character left/right
        transform.Rotate(0, turn * rotationSpeed * Time.deltaTime, 0);
    }
}

