using UnityEngine;

/*
This is the gear slot script, it goes on each peg/axle on the wall where a gear can be placed.
When the player is holding a gear and releases mouse on the slot it snaps the gear onto the peg.
This also works with E key as a fallback.

Any gear can go on any peg now. If its the right size it sits flush and the chain connects.
If its too big it goes halfway in sticking out from the wall cause it doesnt fit properly.
If its too small it sits flush but theres a gap so the chain doesnt reach the next gear.
The player can click on the gear itself to take it back off and try a different one, the gear
highlights when you look at it so you know its clickable
*/
public class GearSlot : MonoBehaviour
{
    public GearPuzzleManager puzzleManager;
    public int requiredGearSize = 1;

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

    // This is the E key fallback for placing gears, removal is handled over in DesktopPlayer now
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
        if (grabbable == null) return;

        // I clear the slot reference on the gear so it knows its not on a peg anymore
        GearInfo info = snappedGear.GetComponent<GearInfo>();
        if (info != null) info.currentSlot = null;

        // I have to add a rigidbody back since I destroyed it when placing
        Rigidbody rb = snappedGear.GetComponent<Rigidbody>();
        if (rb == null) rb = snappedGear.gameObject.AddComponent<Rigidbody>();

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

        Debug.Log($"Gear removed from {gameObject.name}");
    }

    void PlaceGear(DesktopGrabbable held, GearInfo info, DesktopPlayer player)
    {
        held.Release();
        player.SetHeldObject(null);

        // I destroy the rigidbody here entirely so it doesnt jitter after placing
        Rigidbody rb = held.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

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
    }
}
