using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Audio")]
    public AudioSource audioCuesSource;
    public AudioSource musicSource;
    public AudioClip[] audioClips;

    [Header("Mode Selection")]
    bool mode_has_been_chosen = false;
    public GameObject desktop_mode_objects;
    public GameObject vr_mode_objects;

    public Transform VRPlayer;
    public Transform DesktopPlayer;

    public Transform start_position;

    void Awake()
    {
        Instance = this;
    }

    public void PlaySound(int index)
    {
        if (audioCuesSource != null && audioClips != null && index >= 0 && index < audioClips.Length)
        {
            audioCuesSource.clip = audioClips[index];
            audioCuesSource.Play();
        }
    }

    public void StartMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    void Update()
    {
        if (!mode_has_been_chosen)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("1 pressed! Switching to Desktop mode");
                mode_has_been_chosen = true;

                desktop_mode_objects.SetActive(true);
                vr_mode_objects.SetActive(false);

                CharacterController cc = DesktopPlayer.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                DesktopPlayer.transform.position = start_position.position;
                DesktopPlayer.transform.rotation = start_position.rotation;
                if (cc != null) cc.enabled = true;

                var dp = DesktopPlayer.GetComponent<DesktopPlayer>();
                if (dp != null) dp.ResetLook();

                StartMusic();
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                mode_has_been_chosen = true;

                desktop_mode_objects.SetActive(false);
                vr_mode_objects.SetActive(true);

                VRPlayer.transform.position = start_position.position;
                VRPlayer.transform.rotation = start_position.rotation;

                StartMusic();
            }
        }
    }
}
