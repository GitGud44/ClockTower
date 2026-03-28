using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AssemblyTable : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("Display Mode: destroys dropped item and shows a pre-placed display object. Snap Mode: physically snaps the dropped item to a position on the table.")]
    public bool useSnapMode = false;

    [Header("Display Mode Settings")]
    [Tooltip("Only used in Display Mode. Child objects on the table that represent placed items.")]
    public TableSlot[] slots;

    public bool enforceOrder = false;
    public int wrongOrderSoundIndex = 2;
    private int nextSlotIndex = 0;

    [Header("Snap Mode Settings")]
    [Tooltip("Only used in Snap Mode. Tags that are accepted for snapping.")]
    public string[] snapAcceptedTags;
    [Tooltip("Only used in Snap Mode. Where snapped items go.")]
    public Transform snapPoint;

    [Header("Audio")]
    public AudioClip placementClip;
    [Range(0f, 1f)]
    public float placementVolume = 1f;

    private List<GameObject> snappedItems = new List<GameObject>();
    private HashSet<GameObject> ignoredItems = new HashSet<GameObject>();

    // Stores the last VR interactor (hand/controller) that triggered a select, so we can force-grab items into it
    private IXRSelectInteractor lastVRInteractor = null;

    [System.Serializable]
    public class TableSlot
    {
        [Tooltip("The visual object on the table (starts hidden, shown when item is placed)")]
        public GameObject displayObject;
        [Tooltip("Tag that the dropped item must have to fill this slot")]
        public string acceptedTag;
        [Tooltip("Prefab to spawn when the player clicks to grab this item off the table")]
        public Transform grabbablePrefab;
        [HideInInspector] public bool isFilled;
    }

    void Start()
    {
        if (!useSnapMode)
        {
            foreach (TableSlot slot in slots)
            {
                if (slot.displayObject != null)
                    slot.displayObject.SetActive(false);
                slot.isFilled = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (ignoredItems.Contains(other.gameObject)) return;

        if (useSnapMode)
            HandleSnapMode(other);
        else
            HandleDisplayMode(other);
    }

    // ========== Display Mode ==========

    void HandleDisplayMode(Collider other)
    {
        if (enforceOrder)
        {
            // Only accept the next expected slot in sequence
            if (nextSlotIndex >= slots.Length) return;

            TableSlot nextSlot = slots[nextSlotIndex];
            if (!other.CompareTag(nextSlot.acceptedTag))
            {
                // Check if it matches any slot at all — if so, it's out of order
                bool isAnySlotTag = false;
                foreach (TableSlot s in slots)
                    if (other.CompareTag(s.acceptedTag)) { isAnySlotTag = true; break; }

                if (isAnySlotTag && GameManager.Instance != null)
                    GameManager.Instance.PlaySound(wrongOrderSoundIndex);
                return;
            }

            Debug.Log("Item placed on table (ordered): " + nextSlot.acceptedTag);
            if (GameManager.Instance != null) GameManager.Instance.PlaySound(1);

            ForceReleaseXR(other.gameObject);
            Destroy(other.gameObject);

            nextSlot.isFilled = true;
            nextSlotIndex++;

            ActivateSlotDisplay(nextSlot);
        }
        else
        {
            foreach (TableSlot slot in slots)
            {
                if (slot.isFilled) continue;
        if (enforceOrder)
        {
            // Only accept the next expected slot in sequence
            if (nextSlotIndex >= slots.Length) return;

            TableSlot nextSlot = slots[nextSlotIndex];
            if (!other.CompareTag(nextSlot.acceptedTag))
            {
                // Check if it matches any slot at all — if so, it's out of order
                bool isAnySlotTag = false;
                foreach (TableSlot s in slots)
                    if (other.CompareTag(s.acceptedTag)) { isAnySlotTag = true; break; }

                if (isAnySlotTag && GameManager.Instance != null)
                    GameManager.Instance.PlaySound(wrongOrderSoundIndex);
                return;
            }

            Debug.Log("Item placed on table (ordered): " + nextSlot.acceptedTag);
            if (GameManager.Instance != null) GameManager.Instance.PlaySound(1);

            ForceReleaseXR(other.gameObject);
            Destroy(other.gameObject);

            nextSlot.isFilled = true;
            nextSlotIndex++;

            ActivateSlotDisplay(nextSlot);
        }
        else
        {
            foreach (TableSlot slot in slots)
            {
                if (slot.isFilled) continue;

            if (other.CompareTag(slot.acceptedTag))
            {
                Debug.Log("Item placed on table: " + slot.acceptedTag);

                if (placementClip != null && AudioManager.Instance != null)
                    AudioManager.Instance.PlayClip(placementClip, placementVolume);

                    ForceReleaseXR(other.gameObject);
                    Destroy(other.gameObject);
                    ForceReleaseXR(other.gameObject);
                    Destroy(other.gameObject);

                    slot.isFilled = true;
                    ActivateSlotDisplay(slot);
                    return;
                }
            }
        }
    }

     void ActivateSlotDisplay(TableSlot slot)
    {
        if (slot.displayObject == null) return;

        slot.displayObject.SetActive(true);
                    slot.isFilled = true;
                    ActivateSlotDisplay(slot);
                    return;
                }
            }
        }
    }

     void ActivateSlotDisplay(TableSlot slot)
    {
        if (slot.displayObject == null) return;

        slot.displayObject.SetActive(true);

        RaycastInteractable interactable = slot.displayObject.GetComponent<RaycastInteractable>();
        if (interactable == null)
        {
            interactable = slot.displayObject.AddComponent<RaycastInteractable>();
            interactable.objectsToHighlight = new GameObject[] { slot.displayObject };
            interactable.emissionIntensity = 0.3f;
        }
        interactable.enabled = true;

        if (interactable.OnClick == null)
            interactable.OnClick = new UnityEngine.Events.UnityEvent();
        interactable.OnClick.RemoveAllListeners();
        TableSlot capturedSlot = slot;
        interactable.OnClick.AddListener(() => GrabFromSlot(capturedSlot));
        if (interactable.OnClick == null)
            interactable.OnClick = new UnityEngine.Events.UnityEvent();
        interactable.OnClick.RemoveAllListeners();
        TableSlot capturedSlot = slot;
        interactable.OnClick.AddListener(() => GrabFromSlot(capturedSlot));

        SetupVRClickable(slot.displayObject, () => GrabFromSlot(capturedSlot));
    }

    void GrabFromSlot(TableSlot slot)
    {
        if (!slot.isFilled) return;

        Debug.Log("Grabbed item from table: " + slot.acceptedTag);

        slot.displayObject.SetActive(false);
        slot.isFilled = false;

        if (slot.grabbablePrefab != null)
        {
            StartCoroutine(SpawnAndGrabNextFrame(slot));
        }
    }

    System.Collections.IEnumerator SpawnAndGrabNextFrame(TableSlot slot)
    {
        yield return null;

        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player != null && player.holdPoint != null && player.gameObject.activeInHierarchy)
        {
            // Desktop: spawn at hold point and auto-grab
            Vector3 spawnPos = player.holdPoint.position;
            Transform spawned = Instantiate(slot.grabbablePrefab, spawnPos, Quaternion.identity);

            ignoredItems.Add(spawned.gameObject);
            StartCoroutine(RemoveFromIgnoreAfterDelay(spawned.gameObject, 1f));

            DesktopGrabbable grabbable = spawned.GetComponent<DesktopGrabbable>();
            if (grabbable != null)
            {
                grabbable.Grab(player.holdPoint);
                player.SetHeldObject(grabbable);
            }
        }
        else
        {
            // VR: spawn and force-grab into the hand that selected it
            Vector3 spawnPos = Vector3.zero;
            if (lastVRInteractor != null)
                spawnPos = (lastVRInteractor as Component).transform.position;
            else
                spawnPos = slot.displayObject.transform.position + Vector3.up * 0.3f;

            Transform spawned = Instantiate(slot.grabbablePrefab, spawnPos, Quaternion.identity);

            ignoredItems.Add(spawned.gameObject);
            StartCoroutine(RemoveFromIgnoreAfterDelay(spawned.gameObject, 1f));

            // Force XR grab into the hand
            StartCoroutine(ForceXRGrabNextFrame(spawned.gameObject));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (ignoredItems.Contains(other.gameObject))
        {
            GameObject obj = other.gameObject;
            StartCoroutine(RemoveFromIgnoreAfterDelay(obj, 0.5f));
        }
    }

    System.Collections.IEnumerator RemoveFromIgnoreAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        ignoredItems.Remove(obj);
    }

    // ========== Snap Mode ==========

    void HandleSnapMode(Collider other)
    {
        bool isAccepted = false;
        foreach (string tag in snapAcceptedTags)
        {
            if (other.CompareTag(tag))
            {
                isAccepted = true;
                break;
            }
        }

        if (!isAccepted) return;
        if (snappedItems.Contains(other.gameObject)) return;

        Debug.Log("Item snapped to table: " + other.gameObject.name);

        if (placementClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayClip(placementClip, placementVolume);

        GameObject item = other.gameObject;

        ForceReleaseXR(item);

        // Stop physics
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Release if grabbed (desktop)
        DesktopGrabbable grabbable = item.GetComponent<DesktopGrabbable>();
        if (grabbable != null && grabbable.IsGrabbed())
            grabbable.Release();
        if (grabbable != null)
            grabbable.enabled = false;

        // Disable XR grab so it doesn't interfere while snapped
        XRGrabInteractable xrGrab = item.GetComponent<XRGrabInteractable>();
        if (xrGrab != null)
            xrGrab.enabled = false;

        // Snap position
        if (snapPoint != null)
        {
            Vector3 offset = Vector3.up * (snappedItems.Count * 0.15f);
            item.transform.position = snapPoint.position + offset;
            item.transform.rotation = snapPoint.rotation;
        }

        item.transform.SetParent(transform);
        snappedItems.Add(item);

        // Desktop: Make clickable via raycast
        RaycastInteractable interactable = item.GetComponent<RaycastInteractable>();
        if (interactable == null)
        {
            interactable = item.AddComponent<RaycastInteractable>();
            interactable.objectsToHighlight = new GameObject[] { item };
            interactable.emissionIntensity = 0.3f;
        }
        interactable.enabled = true;

        interactable.OnClick.RemoveAllListeners();
        GameObject capturedItem = item;
        interactable.OnClick.AddListener(() => GrabSnappedItem(capturedItem));

        // VR: Make clickable via XR select
        SetupVRClickable(item, () => GrabSnappedItem(capturedItem));
    }

    void GrabSnappedItem(GameObject item)
    {
        if (!snappedItems.Contains(item)) return;

        Debug.Log("Grabbed snapped item: " + item.name);

        snappedItems.Remove(item);
        item.transform.SetParent(null);

        RaycastInteractable interactable = item.GetComponent<RaycastInteractable>();
        if (interactable != null)
            interactable.OnClick.RemoveAllListeners();

        CleanupVRClickable(item);

        ignoredItems.Add(item);
        StartCoroutine(RemoveFromIgnoreAfterDelay(item, 1f));

        StartCoroutine(GrabSnappedNextFrame(item));
    }

    System.Collections.IEnumerator GrabSnappedNextFrame(GameObject item)
    {
        yield return null;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        DesktopGrabbable grabbable = item.GetComponent<DesktopGrabbable>();
        if (grabbable != null)
            grabbable.enabled = true;

        // Re-enable XR grab for VR
        XRGrabInteractable xrGrab = item.GetComponent<XRGrabInteractable>();
        if (xrGrab != null)
            xrGrab.enabled = true;

        // Auto-grab to desktop player's hand
        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player != null && player.holdPoint != null && player.gameObject.activeInHierarchy && grabbable != null)
        {
            item.transform.position = player.holdPoint.position;
            grabbable.Grab(player.holdPoint);
            player.SetHeldObject(grabbable);
        }
        else
        {
            // VR: force-grab into the hand
            StartCoroutine(ForceXRGrabNextFrame(item));
        }
    }

    // ========== VR Interaction Helpers ==========

    void ForceReleaseXR(GameObject item)
    {
        XRGrabInteractable xrGrab = item.GetComponent<XRGrabInteractable>();
        if (xrGrab != null && xrGrab.isSelected)
        {
            xrGrab.enabled = false;
            xrGrab.enabled = true;
        }
    }

    void SetupVRClickable(GameObject obj, UnityEngine.Events.UnityAction callback)
    {
        XRSimpleInteractable xrInteractable = obj.GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
            xrInteractable = obj.AddComponent<XRSimpleInteractable>();
        xrInteractable.enabled = true;
        xrInteractable.selectEntered.RemoveAllListeners();
        xrInteractable.selectEntered.AddListener((args) =>
        {
            // Remember which hand/controller selected this so we can force-grab the next item into it
            lastVRInteractor = args.interactorObject;
            callback();
        });
    }

    void CleanupVRClickable(GameObject obj)
    {
        XRSimpleInteractable xrInteractable = obj.GetComponent<XRSimpleInteractable>();
        if (xrInteractable != null)
        {
            xrInteractable.selectEntered.RemoveAllListeners();
            xrInteractable.enabled = false;
        }
    }

    // Force the XR interaction manager to grab an item into the last VR hand that selected something
    System.Collections.IEnumerator ForceXRGrabNextFrame(GameObject item)
    {
        yield return null; // Wait a frame for XR components to initialize

        XRGrabInteractable xrGrab = item.GetComponent<XRGrabInteractable>();
        if (xrGrab == null || lastVRInteractor == null) yield break;

        // Make sure XR grab is enabled
        xrGrab.enabled = true;

        // Move item to the hand position
        Component interactorComponent = lastVRInteractor as Component;
        if (interactorComponent != null)
            item.transform.position = interactorComponent.transform.position;

        // Use the interaction manager to force the grab
        XRInteractionManager manager = FindAnyObjectByType<XRInteractionManager>();
        if (manager != null)
        {
            manager.SelectEnter(lastVRInteractor, xrGrab);
            Debug.Log("VR: Force-grabbed item into hand: " + item.name);
        }
    }

    // ========== Shared ==========

    public void ResetTable()
    {
        nextSlotIndex = 0;
        foreach (TableSlot slot in slots)
        {
            if (slot.displayObject != null)
                slot.displayObject.SetActive(false);
            slot.isFilled = false;
        }

        foreach (GameObject item in snappedItems)
        {
            if (item != null)
                Destroy(item);
        }
        snappedItems.Clear();
    }
}
