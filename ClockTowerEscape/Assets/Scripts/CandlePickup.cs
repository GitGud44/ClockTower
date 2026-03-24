using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CandlePickup : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Local clip for spatial pickup playback")]
    public AudioClip pickupClip;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    private bool hasBeenPickedUp = false;

    void Awake()
    {
        // Desktop interaction: listen for clicks via RaycastInteractable
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnPickup);
        }
    }

    void Start()
    {
        // VR interaction: listen for XR grab events
        XRGrabInteractable xrGrab = GetComponent<XRGrabInteractable>();
        if (xrGrab != null)
        {
            xrGrab.selectEntered.AddListener((args) => OnPickup());
        }
    }

    private void OnPickup()
    {
        if (!hasBeenPickedUp)
        {
            hasBeenPickedUp = true;

            if (pickupClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySpatialClip(pickupClip, transform.position, pickupVolume, 1f);

            Debug.Log("Candle picked up - playing PickupCandle sound");
        }
    }
}
