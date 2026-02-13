using UnityEngine;

public class BookInteractable : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Index in GameManager.audioClips for the PageFlip sound")]
    public int pageFlipSoundIndex = 3;

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
        if (GameManager.Instance != null)
            GameManager.Instance.PlaySound(pageFlipSoundIndex);

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
