using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LanternInteractable : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The LanternLight child sphere to activate when lit")]
    public GameObject lanternLight;
    [Tooltip("Reference to the puzzle manager")]
    public LanternPuzzleManager puzzleManager;

    [Header("VR Detection")]
    [Tooltip("Tag on the candle object for VR trigger detection")]
    public string candleTag = "Candle";

    [Header("Audio")]
    [Tooltip("Local SFX played when this lantern is lit")]
    public AudioClip lightLanternClip;
    [Range(0f, 1f)]
    public float lightLanternVolume = 1f;

    private bool isLit = false;

    void Awake()
    {
        // Desktop interaction: listen for clicks via RaycastInteractable
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnLanternClicked);
        }
    }

    void Start()
    {
        // Ensure lantern starts in the off state
        if (lanternLight != null)
            lanternLight.SetActive(false);
        isLit = false;
    }

    // Desktop mode: player clicks on lantern while holding candle
    private void OnLanternClicked()
    {
        if (isLit) return; // Already lit, do nothing

        // Check if desktop player is holding a candle
        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player != null && player.GetHeldObject() != null)
        {
            if (player.GetHeldObject().CompareTag(candleTag))
            {
                LightUp();
            }
        }
    }

    // VR mode: player brings candle near lantern (trigger collision)
    private void OnTriggerEnter(Collider other)
    {
        if (isLit) return; // Already lit, do nothing

        // Only allow trigger-based lighting in VR mode (skip if desktop player is active)
        DesktopPlayer player = FindAnyObjectByType<DesktopPlayer>();
        if (player != null && player.gameObject.activeInHierarchy)
            return;

        // Check if the colliding object is the candle
        if (other.CompareTag(candleTag))
        {
            LightUp();
        }
    }

    private void LightUp()
    {
        if (isLit) return;

        isLit = true;

        // Activate the light
        if (lanternLight != null)
            lanternLight.SetActive(true);

        if (lightLanternClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(lightLanternClip, transform.position, lightLanternVolume, 1f);

        // Notify the puzzle manager
        if (puzzleManager != null)
            puzzleManager.OnLanternLit(this);

        Debug.Log($"Lantern lit: {gameObject.name}");
    }

    // Called by puzzle manager to extinguish the lantern
    public void Extinguish()
    {
        isLit = false;

        if (lanternLight != null)
            lanternLight.SetActive(false);

        Debug.Log($"Lantern extinguished: {gameObject.name}");
    }
}
