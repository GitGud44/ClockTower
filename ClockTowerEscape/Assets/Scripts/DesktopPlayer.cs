using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera; // Assign your camera here in the Inspector

    [Header("Interaction")]
    public float interactionRange = 3f;
    public Transform holdPoint; // Empty GameObject in front of camera where grabbed objects go

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation = 0f;
    private RaycastInteractable lastTarget = null;
    private DesktopGrabbable currentlyHeld = null;

    // Public getter for currently held object (used by LanternInteractable to check if holding candle)
    public DesktopGrabbable GetHeldObject()
    {
        return currentlyHeld;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Call this to reset camera look direction
    public void ResetLook()
    {
        xRotation = 0f;
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.identity;
    }
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        
        // Cast ray from camera, ignoring colliders on the held object so it doesn't block aiming
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        SetHeldCollidersEnabled(false);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, interactionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        SetHeldCollidersEnabled(true);
        if (didHit)
        {
            RaycastInteractable target = hit.collider.GetComponentInParent<RaycastInteractable>();
            
            // Started looking at a new interactable
            if (target != null && target != lastTarget)
            {
                if (lastTarget != null) lastTarget.GazeExit();
                target.GazeEnter();
                lastTarget = target;
            }
            // Stopped looking at interactable (hit something else)
            else if (target == null && lastTarget != null)
            {
                lastTarget.GazeExit();
                lastTarget = null;
            }
            
            // Click while looking at interactable (only if not holding something)
            if (target != null && currentlyHeld == null && (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
            {
                target.Click();
            }
            // Use/interact while holding something (E key only, to avoid conflicts with mouse release)
            else if (target != null && currentlyHeld != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                target.Click();
            }
            
            // Grab: mouse pressed on a grabbable object
            if (currentlyHeld == null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                DesktopGrabbable grabbable = hit.collider.GetComponentInParent<DesktopGrabbable>();
                if (grabbable != null && holdPoint != null)
                {
                    grabbable.Grab(holdPoint);
                    currentlyHeld = grabbable;
                }
            }
        }
        else if (lastTarget != null)
        {
            // Ray hit nothing
            lastTarget.GazeExit();
            lastTarget = null;
        }
        
        // Release: mouse released while holding something
        if (currentlyHeld != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // if aiming at a gear slot, try to place the gear there instead of dropping it
            bool placed = false;
            if (didHit)
            {
                GearSlot slot = hit.collider.GetComponentInParent<GearSlot>();
                if (slot != null)
                {
                    slot.TryPlaceGear(this);
                    placed = slot.isFilled;
                }
            }

            if (!placed)
            {
                currentlyHeld.Release();
                currentlyHeld = null;
            }
        }
    }

    // Called by other scripts (e.g. AssemblyTable) to force the player to hold an object
    public void SetHeldObject(DesktopGrabbable grabbable)
    {
        currentlyHeld = grabbable;
    }

    private void SetHeldCollidersEnabled(bool enabled)
    {
        if (currentlyHeld == null) return;
        foreach (Collider col in currentlyHeld.GetComponentsInChildren<Collider>())
            col.enabled = enabled;
    }

    void HandleMouseLook()
    {
        // Get mouse input
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime * 10f;

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseDelta.x);

        // Rotate camera up/down (clamped so you can't look behind yourself)
        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        // Get WASD input
        Vector2 moveInput = Vector2.zero;
        
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

        // Calculate movement direction relative to where player is facing
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= moveSpeed;

        // Apply gravity
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // Small downward force to keep grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        move.y = verticalVelocity;

        // Move the player
        controller.Move(move * Time.deltaTime);
    }

    void OnDisable()
    {
        if (lastTarget != null) lastTarget.GazeExit();
        lastTarget = null;
        
        // Release any held object
        if (currentlyHeld != null)
        {
            currentlyHeld.Release();
            currentlyHeld = null;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnEnable()
    {
        // Lock cursor when this player is enabled
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
