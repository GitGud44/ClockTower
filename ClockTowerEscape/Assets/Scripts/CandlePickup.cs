using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CandlePickup : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Index in GameManager.audioClips for the PickupCandle sound")]
    public int pickupSoundIndex = 4;

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

            if (GameManager.Instance != null)
                GameManager.Instance.PlaySound(pickupSoundIndex);

            Debug.Log("Candle picked up - playing PickupCandle sound");
        }
    }
}
