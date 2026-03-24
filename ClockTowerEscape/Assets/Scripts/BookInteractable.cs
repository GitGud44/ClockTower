using UnityEngine;

public class BookInteractable : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Local clip for spatial page-flip playback")]
    public AudioClip pageFlipClip;
    [Range(0f, 1f)]
    public float pageFlipVolume = 1f;

    private int hand_colliders_inside = 0;

    void Awake()
    {
        // Desktop interaction: listen for clicks via RaycastInteractable
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnBookInteracted);
        }
    }

    // Desktop mode: player clicks on book
    private void OnBookInteracted()
    {
        if (pageFlipClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpatialClip(pageFlipClip, transform.position, pageFlipVolume, 1f);
        }

        Debug.Log("Book interacted - playing PageFlip sound");

        // TODO: Show hint UI with lantern order clues
    }

    // VR mode: player touches book with hand
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            hand_colliders_inside++;
            if (hand_colliders_inside == 1)
            {
                OnBookInteracted();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            hand_colliders_inside--;
            if (hand_colliders_inside < 0)
                hand_colliders_inside = 0;
        }
    }
}
