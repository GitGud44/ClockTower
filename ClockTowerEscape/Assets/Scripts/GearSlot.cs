using UnityEngine;

/*
This is the gear slot script, it goes on each peg/axle on the wall where a gear can be placed.
When the player is holding a gear and releases mouse on the slot it snaps the gear onto the peg.
This also works with E key as a fallback.

requiredGearType has to match the GearInfo.gearType on the gear or it wont accept it.
If requiredGearType is left empty it accepts any gear (good for testing)
*/
public class GearSlot : MonoBehaviour
{
    public GearPuzzleManager puzzleManager;
    public string requiredGearType = "";

    [HideInInspector] public bool isFilled = false;
    [HideInInspector] public Transform snappedGear;

    void Awake()
    {
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnSlotClicked);
        }
    }

    // This is called by DesktopPlayer when mouse is released while looking at the slot
    public void TryPlaceGear(DesktopPlayer player)
    {
        if (isFilled) return;

        DesktopGrabbable held = player.GetHeldObject();
        if (held == null || !held.CompareTag("Gear")) return;

        GearInfo info = held.GetComponent<GearInfo>();
        if (info == null) return;

        if (requiredGearType != "" && info.gearType != requiredGearType)
        {
            Debug.Log($"Wrong gear! this slot needs a {requiredGearType} gear");
            return;
        }

        // I release the gear from the player here
        held.Release();
        player.SetHeldObject(null);

        // I destroy the rigidbody entirely here so it doesnt jitter after placing
        // setting it to kinematic wasnt enough, there was a frame where gravity kicked in
        Rigidbody rb = held.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        held.enabled = false;
        held.transform.SetParent(null);
        held.transform.position = transform.position;
        held.transform.rotation = transform.rotation;

        snappedGear = held.transform;
        isFilled = true;

        // I turn off the collider here but keep the axle mesh visible
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log($"Gear placed in slot: {gameObject.name}");
        if (puzzleManager != null)
            puzzleManager.OnGearPlaced(this);
    }

    // This is the E key fallback, does the same thing as drag
    void OnSlotClicked()
    {
        if (isFilled) return;

        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player == null) return;
        DesktopGrabbable held = player.GetHeldObject();
        if (held == null || !held.CompareTag("Gear")) return;

        GearInfo info = held.GetComponent<GearInfo>();
        if (info == null) return;
        if (requiredGearType != "" && info.gearType != requiredGearType)
        {
            Debug.Log($"Wrong gear! this slot needs a {requiredGearType} gear");
            return;
        }

        held.Release();
        player.SetHeldObject(null);

        Rigidbody rb = held.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
        held.enabled = false;

        held.transform.SetParent(null);
        held.transform.position = transform.position;
        held.transform.rotation = transform.rotation;
        snappedGear = held.transform;
        isFilled = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log($"Gear placed in slot: {gameObject.name}");
        if (puzzleManager != null)
            puzzleManager.OnGearPlaced(this);
    }
}
