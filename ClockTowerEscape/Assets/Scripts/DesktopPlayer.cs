using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;


    public float mouseSensitivity = 2f;
    public Transform playerCamera;


    public float interactionRange = 3f;
    public Transform holdPoint; // empty gameobject in front of camera where grabbed objects go


    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation = 0f;
    private RaycastInteractable lastTarget = null;
    private DesktopGrabbable currentlyHeld = null;

    // I track the highlighted placed gear separately since placed gears dont have a RaycastInteractable on them
    private GearInfo highlightedGear = null;
    private Material[] highlightedGearMats = null;
    private Color[] highlightedGearOrigEmissions = null;
    private bool[] highlightedGearHadEmission = null;

    // I need this so other scripts like LanternInteractable can check what the player is holding
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
    
    // reset where the camera is looking, use this when switching back to this player
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
        
        // A ray is shotfrom the camera to see what the player is looking at, held object colliders are already off in DesktopGrabbable
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, interactionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        // I also check if the ray hit a placed gear so I can highlight it separately from the normal interactable system
        GearInfo hitGear = didHit ? hit.collider.GetComponentInParent<GearInfo>() : null;
        bool hitPlacedGear = (hitGear != null && hitGear.currentSlot != null);

        // I handle placed gear highlighting manually here since they dont have a RaycastInteractable component on them
        if (hitPlacedGear && hitGear != highlightedGear)
        {
            UnhighlightGear();
            HighlightGear(hitGear);
        }
        else if (!hitPlacedGear && highlightedGear != null)
        {
            UnhighlightGear();
        }

        if (didHit)
        {
            RaycastInteractable target = hitPlacedGear ? null : hit.collider.GetComponentInParent<RaycastInteractable>();

            // started looking at something new
            if (target != null && target != lastTarget)
            {
                if (lastTarget != null) lastTarget.GazeExit();
                target.GazeEnter();
                lastTarget = target;
            }
            // stopped looking at it, hit something else
            else if (target == null && lastTarget != null)
            {
                lastTarget.GazeExit();
                lastTarget = null;
            }

            // if the player clicks while not holding anything I check what they clicked on
            if (currentlyHeld == null && (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
            {
                // here I check if the player clicked on a gear thats sitting on a peg, if so I take it off and give it back to them
                if (hitPlacedGear)
                {
                    UnhighlightGear();
                    hitGear.currentSlot.RemoveGear(this);
                }
                // or maybe they clicked the peg itself while a gear is on it, same thing
                else if (target != null)
                {
                    GearSlot slot = hit.collider.GetComponentInParent<GearSlot>();
                    if (slot != null && slot.isFilled)
                        slot.RemoveGear(this);
                    else
                        target.Click();
                }
            }
            // E key interact while holding something, I use E only here so it doesnt conflict with mouse release
            else if (target != null && currentlyHeld != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                target.Click();
            }

            //grab: if they pressed mouse on something grabbable I pick it up
            if (currentlyHeld == null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                DesktopGrabbable grabbable = hit.collider.GetComponentInParent<DesktopGrabbable>();
                if (grabbable != null && holdPoint != null)
                {
                    grabbable.Grab(holdPoint);
                    currentlyHeld = grabbable;

                    if (grabbable.CompareTag("Gear"))
                    {
                        GearInfo gearInfo = grabbable.GetComponent<GearInfo>();
                        if (gearInfo != null && gearInfo.pickupClip != null && AudioManager.Instance != null)
                            AudioManager.Instance.PlaySpatialClip(gearInfo.pickupClip, grabbable.transform.position, gearInfo.pickupVolume, 1f);
                    }
                }
            }
        }
        else
        {
            // ray hit nothing
            if (lastTarget != null)
            {
                lastTarget.GazeExit();
                lastTarget = null;
            }
        }
        
        // mouse released while holding something, I check if theyre aiming at a gear slot to place it there
        if (currentlyHeld != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
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

    // other scripts call this to force the player to hold something (like when taking a gear off a peg)
    public void SetHeldObject(DesktopGrabbable grabbable)
    {
        currentlyHeld = grabbable;
    }



    // I highlight placed gears manually by turning on emission so the player knows they can click them
    void HighlightGear(GearInfo gear)
    {
        highlightedGear = gear;
        Renderer[] renderers = gear.GetComponentsInChildren<Renderer>();
        highlightedGearMats = new Material[renderers.Length];
        highlightedGearOrigEmissions = new Color[renderers.Length];
        highlightedGearHadEmission = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            highlightedGearMats[i] = renderers[i].material;
            highlightedGearHadEmission[i] = highlightedGearMats[i].IsKeywordEnabled("_EMISSION");
            highlightedGearOrigEmissions[i] = highlightedGearMats[i].GetColor("_EmissionColor");
            highlightedGearMats[i].EnableKeyword("_EMISSION");
            highlightedGearMats[i].SetColor("_EmissionColor", Color.white * 0.2f);
        }
    }

    void UnhighlightGear()
    {
        if (highlightedGear == null) return;
        if (highlightedGearMats != null)
        {
            for (int i = 0; i < highlightedGearMats.Length; i++)
            {
                if (highlightedGearMats[i] == null) continue;
                if (!highlightedGearHadEmission[i])
                    highlightedGearMats[i].DisableKeyword("_EMISSION");
                highlightedGearMats[i].SetColor("_EmissionColor", highlightedGearOrigEmissions[i]);
            }
        }
        highlightedGear = null;
        highlightedGearMats = null;
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime * 10f;

        // rotate player left/right
        transform.Rotate(Vector3.up * mouseDelta.x);

        // rotate camera up/down, I clamp it so you cant look behind yourself
        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;
        
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

        // movement direction relative to where the player is facing
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= moveSpeed;

        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // small push down to keep grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void OnDisable()
    {
        if (lastTarget != null) lastTarget.GazeExit();
        lastTarget = null;
        UnhighlightGear();

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
        // lock cursor when this player gets enabled
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // if any grabbable object falls way below the player it went out of bounds somehow (through a wall or
    // through the floor), I just teleport it back near the player. same idea as VRInteractionBridge does
    // for VR mode but this catches desktop grabbables too
    void LateUpdate()
    {
        float playerY = transform.position.y;
        DesktopGrabbable[] allGrabbables = FindObjectsByType<DesktopGrabbable>(FindObjectsSortMode.None);

        for (int i = 0; i < allGrabbables.Length; i++)
        {
            if (allGrabbables[i].transform.position.y < playerY - 10f)
            {
                Transform obj = allGrabbables[i].transform;
                obj.position = transform.position + Vector3.up * 0.5f;

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    StartCoroutine(RestorePhysicsNextFrame(rb));
                }

                Debug.Log($"[DesktopPlayer] '{obj.name}' fell out of bounds, teleported back to player");
            }
        }
    }

    IEnumerator RestorePhysicsNextFrame(Rigidbody rb)
    {
        yield return null;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
