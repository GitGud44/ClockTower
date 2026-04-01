using UnityEngine;

/*
This is the gear slot script, it goes on each peg/axle on the wall where a gear can be placed.
When the player is holding a gear, press E to snap the gear onto the peg.

I made some complex logic here:
Any gear can go on any peg now. If its the right size it sits flush and the chain connects and the gears spin.
If its too big, to simulate real life it fits on the peg but goes IN FRONT of the other gears: it goes halfway in sticking out from the wall cause it doesnt fit properly.
If its too small it sits flush but theres a gap so the chain doesnt reach the next gear.
The player can click on the gear itself to take it back off and try a different one, the gear
highlights when you look at it so you know its clickable
*/
public class GearSlot : MonoBehaviour
{
    public GearPuzzleManager puzzleManager;
    public int requiredGearSize = 1;

    [Header("Audio")]
    public AudioClip placeClip;
    public AudioClip removeClip;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [HideInInspector] public bool isFilled = false;
    [HideInInspector] public Transform snappedGear;
    [HideInInspector] public bool isCorrectGear = false;
    float removeCooldown = 0f;

    void Awake()
    {
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnSlotClicked);
        }
    }

    void Update()
    {
        if (removeCooldown > 0f) removeCooldown -= Time.deltaTime;
    }

    // This is called by DesktopPlayer when the mouse gets released while looking at the slot
    public void TryPlaceGear(DesktopPlayer player)
    {
        if (isFilled || removeCooldown > 0f) return;

        DesktopGrabbable held = player.GetHeldObject();
        if (held == null || !held.CompareTag("Gear")) return;

        GearInfo info = held.GetComponent<GearInfo>();
        if (info == null) return;

        PlaceGear(held, info, player);
    }

    // This is the E key fallback for placing gears, its removal is handled over in DesktopPlayer now
    void OnSlotClicked()
    {
        if (isFilled || removeCooldown > 0f) return;

        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player == null) return;

        DesktopGrabbable heldGear = player.GetHeldObject();
        if (heldGear == null || !heldGear.CompareTag("Gear")) return;

        GearInfo info = heldGear.GetComponent<GearInfo>();
        if (info == null) return;

        PlaceGear(heldGear, info, player);
    }




    // this takes the gear off the peg and gives it back to the player so they can try placing it somewhere else
    public void RemoveGear(DesktopPlayer player)
    {
        DesktopGrabbable grabbable = snappedGear.GetComponent<DesktopGrabbable>();
        if (grabbable == null)
        {
            return;
        }
        

        // I clear the slot reference on the gear so it knows its not on a peg anymore
        GearInfo info = snappedGear.GetComponent<GearInfo>();
        if (info != null)
        {
            info.currentSlot = null;
        }

        // the rigidbody is kinematic from when I placed it, I need to set it back to non-kinematic before Grab saves wasKinematic otherwise when the player drops it later it stays kinematic and just freezes mid air
        Rigidbody rb = snappedGear.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = snappedGear.gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;

        // re-enable grabbable and use the same Grab call that DesktopPlayer uses, Grab handles disabling colliders and everything
        grabbable.enabled = true;
        grabbable.Grab(player.holdPoint);
        player.SetHeldObject(grabbable);

        isFilled = false;
        isCorrectGear = false;
        snappedGear = null;

        removeCooldown = 0.5f;

        if (puzzleManager != null)
            puzzleManager.OnGearRemoved(this);

        if (removeClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(removeClip, transform.position, sfxVolume, 1f);

        Debug.Log($"Gear removed from {gameObject.name}");
    }

    // this is the same as RemoveGear but for VR, I cant use the desktop version because it needs a DesktopPlayer
    // reference and in VR theres no DesktopPlayer, the XRGrabInteractable handles the actual grabbing part
    public void RemoveGearVR()
    {
        if (!isFilled || snappedGear == null) return;

        GearInfo info = snappedGear.GetComponent<GearInfo>();
        if (info != null) info.currentSlot = null;

        // I re-enable the desktop grabbable just in case, it was disabled when the gear was placed
        DesktopGrabbable grabbable = snappedGear.GetComponent<DesktopGrabbable>();
        if (grabbable != null) grabbable.enabled = true;

        // XRGrabInteractable takes over the rigidbody from here so I just need to un-kinematic it
        Rigidbody rb = snappedGear.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        isFilled = false;
        isCorrectGear = false;
        snappedGear = null;
        removeCooldown = 0.5f;

        if (puzzleManager != null)
            puzzleManager.OnGearRemoved(this);

        if (removeClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(removeClip, transform.position, sfxVolume, 1f);

        Debug.Log($"Gear removed (VR) from {gameObject.name}");
    }

    // same deal as PlaceGear but for VR, VRGearInteraction calls this when the player releases a gear near a slot
    public void PlaceGearVR(Transform gear, GearInfo info)
    {
        if (isFilled || removeCooldown > 0f) return;

        // I freeze the rigidbody so the gear doesnt jitter or fall off the peg after I snap it there
        Rigidbody rb = gear.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        gear.SetParent(null);
        gear.position = transform.position;
        gear.rotation = transform.rotation;

        // same offset logic as the desktop version, if the gear is too big it sticks out
        if (info.gearSize > requiredGearSize)
        {
            float stickOut = (info.gearSize - requiredGearSize) * 0.05f;
            gear.position += gear.up * stickOut;
        }

        snappedGear = gear;
        isFilled = true;
        isCorrectGear = (info.gearSize == requiredGearSize);
        info.currentSlot = this;

        foreach (Collider c in gear.GetComponentsInChildren<Collider>())
            c.enabled = true;

        if (puzzleManager != null)
            puzzleManager.OnGearPlaced(this);

        if (placeClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(placeClip, transform.position, sfxVolume, 1f);

        Debug.Log($"Gear placed (VR) in {gameObject.name} — {(isCorrectGear ? "correct size!" : "wrong size")}");
    }

    void PlaceGear(DesktopGrabbable held, GearInfo info, DesktopPlayer player)
    {
        held.Release();
        player.SetHeldObject(null);

        // I used to Destroy the rigidbody here but then XRGrabInteractable couldnt detect the gear anymore in VR mode because XR needs a rigidbody on the object to interact with it, so now I just make it kinematic instead which stops the jitter without breaking VR
        Rigidbody rb = held.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        held.enabled = false;
        held.transform.SetParent(null);

        // center the gear on the peg first then offset it if its too big
        held.transform.position = transform.position;
        held.transform.rotation = transform.rotation;

        // if the gear is too big it sticks out from the wall halfway cause it doesnt fit on the peg properly
        if (info.gearSize > requiredGearSize)
        {
            // I push it away from the wall along the peg axis so it looks like its jammed in there
            float stickOut = (info.gearSize - requiredGearSize) * 0.05f;
            held.transform.position += held.transform.up * stickOut;
            Debug.Log($"Gear placed in {gameObject.name} — too big, sticking out");
        }

        snappedGear = held.transform;
        isFilled = true;
        isCorrectGear = (info.gearSize == requiredGearSize);
        info.currentSlot = this;

        // I re-enable colliders here so the player can actually click on the gear to take it back off
        foreach (Collider c in held.GetComponentsInChildren<Collider>())
            c.enabled = true;

        if (isCorrectGear)
            Debug.Log($"Gear placed in {gameObject.name} — correct size! chain connects through");
        else if (info.gearSize < requiredGearSize)
            Debug.Log($"Gear placed in {gameObject.name} — too small, doesnt reach the next gear");

        if (puzzleManager != null)
            puzzleManager.OnGearPlaced(this);

        if (placeClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(placeClip, transform.position, sfxVolume, 1f);
    }
}
