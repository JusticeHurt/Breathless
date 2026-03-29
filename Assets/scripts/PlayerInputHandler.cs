using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    
    [Header("References")]
    public Hunter hunter; 
    public FirstPersonCamera playerCamera; 
    private PlayerBreath playerBreath; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerBreath = GetComponent<PlayerBreath>();
    }

    void Update()
    {
        if (hunter == null || playerCamera == null) return;

        HandleMovement();
        HandleRotationAndCamera();
        HandleBreathing();
        HandleJumping();
    }

    private void HandleMovement()
    {
        Vector3 moveInput = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) moveInput += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) moveInput += Vector3.back;
        if (Keyboard.current.aKey.isPressed) moveInput += Vector3.left;
        if (Keyboard.current.dKey.isPressed) moveInput += Vector3.right;

        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        float moveMultiplier = 1.0f;

        // left shift (Breath) or right click (Aim) both slow me down
        if (Keyboard.current.leftShiftKey.isPressed || Mouse.current.rightButton.isPressed) 
        {
            moveMultiplier = 0.4f; 
        }

        Vector3 relativeMove = (camForward * moveInput.z) + (camRight * moveInput.x);
        hunter.Move(relativeMove * moveMultiplier);

    }

    private void HandleRotationAndCamera()
    {
        hunter.transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
        float mouseX = Mouse.current.delta.x.ReadValue();
        float mouseY = Mouse.current.delta.y.ReadValue();
        playerCamera.AdjustRotation(mouseX, mouseY);
    }

    private void HandleBreathing()
    {
        if (playerBreath != null)
        {
            // leftShiftKey 
            playerBreath.isHoldingBreath = Keyboard.current.leftShiftKey.isPressed;
        }
    }

    private void HandleJumping()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) hunter.Jump();
    }

}
