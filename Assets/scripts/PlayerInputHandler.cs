using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("References")]
    public Hunter hunter; 
    public FirstPersonCamera playerCamera; 
    private PlayerBreath playerBreath; 

    [Header("Interaction Settings")]
    public float interactDistance = 3f; 
    public GameObject interactPromptUI; 
    private NoteInteraction activeNote = null; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerBreath = GetComponent<PlayerBreath>();
        
        // Make sure the prompt is hidden when the game starts
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    void Update()
    {
        if (hunter == null || playerCamera == null) return;

        HandleInteraction(); 

        // If a note is open, stop moving and looking
        if (activeNote != null) return; 

        HandleMovement();
        HandleRotationAndCamera();
        HandleBreathing();
        HandleJumping();
    }

    private void HandleInteraction()
    {
        // CURRENTLY READING A NOTE
        if (activeNote != null)
        {
            // prompt hidden while reading
            if (interactPromptUI != null) interactPromptUI.SetActive(false);

            // Press R to close it
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                activeNote.CloseNote();
                activeNote = null;
            }
            return; //sop running the rest of this method
        }

        //NOT READING (Walking around)
        bool isLookingAtNote = false;
        NoteInteraction hoveredNote = null;

        // Shoot the laser every frame to see what we are looking at
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactDistance))
        {
            hoveredNote = hit.collider.GetComponent<NoteInteraction>();
            if (hoveredNote != null)
            {
                isLookingAtNote = true; // the Laser hit a note
            }
        }

        // urn the UI on if we see a note, off if we don't
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(isLookingAtNote);
        }

        // 
        // PRESS R TO OPEN
        if (isLookingAtNote && Keyboard.current.rKey.wasPressedThisFrame)
        {
            hoveredNote.OpenNote();
            activeNote = hoveredNote;
            
            // hide the prompt instantly once we open the note
            if (interactPromptUI != null) interactPromptUI.SetActive(false); 
        }
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
            playerBreath.isHoldingBreath = Keyboard.current.leftShiftKey.isPressed;
        }
    }

    private void HandleJumping()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) hunter.Jump();
    }
}