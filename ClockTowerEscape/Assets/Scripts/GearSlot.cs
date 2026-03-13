using UnityEngine;

public class GearSlot : MonoBehaviour
{
    [Header("References")]
    public GearPuzzleManager puzzleManager;

    // Keep public so they can be accessed by the GearPuzzleManager but still dont want them to show in the inspector to clutter it up
    [HideInInspector] public bool isFilled = false;
    [HideInInspector] public Transform snappedGear;

    void Awake()
    {
        // Press E for interaction uisng the RaycastInteractable
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnSlotClicked);
        }
    }

    private void OnSlotClicked()
    {
        // if the slot is already filled, dont do anything
        if (isFilled) return;

        // Check if the player is holding a gear
        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player == null) return;

        DesktopGrabbable held = player.GetHeldObject();
        if (held == null || !held.CompareTag("Gear")) return;

        // Snap the gear into this slot
        held.Release();
        player.SetHeldObject(null);

        // Disable physics and parent to slot
        Rigidbody rb = held.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        // Disable the grabbable so it can't be picked up again
        held.enabled = false;

        // Dont parent to slot — keep gear independent so it spins on its own transform
        held.transform.SetParent(null);
        held.transform.position = transform.position;
        held.transform.rotation = transform.rotation;

        snappedGear = held.transform;
        isFilled = true;

        // Disable collider so it can't be interacted with again (axle stays visible)
        Collider slotCollider = GetComponent<Collider>();
        if (slotCollider != null)
        {
            slotCollider.enabled = false;
        }
    
        Debug.Log($"Gear placed in the slot: {gameObject.name}");

        if (puzzleManager != null)
        {
            puzzleManager.OnGearPlaced(this);
        }
            

    }
}
