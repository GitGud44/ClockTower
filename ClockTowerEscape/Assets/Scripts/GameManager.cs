using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum PlayMode { None, Desktop, VR }
    public PlayMode CurrentPlayMode { get; private set; } = PlayMode.None;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetDesktopMode()
    {
        CurrentPlayMode = PlayMode.Desktop;
    }

    public void SetVRMode()
    {
        CurrentPlayMode = PlayMode.VR;
    }

    public bool isDesktopMode()
    {
        return CurrentPlayMode == PlayMode.Desktop;
    }

    public bool isVRMode()
    {
        return CurrentPlayMode == PlayMode.VR;
    }

    /// <summary>
    /// Sets the game mode (Desktop or VR)
    /// </summary>
    public void SetGameMode(PlayMode mode)
    {
        CurrentPlayMode = mode;
    }

    /// <summary>
    /// Plays a sound effect by delegating to AudioManager
    /// </summary>
    public void PlaySound(int index)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(index);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found!");
        }
    }

    /// <summary>
    /// Plays a sound effect with volume by delegating to AudioManager
    /// </summary>
    public void PlaySound(int index, float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(index, volume);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found!");
        }
    }

    /// <summary>
    /// Starts background music by delegating to AudioManager
    /// </summary>
    public void StartMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartMusic();
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found!");
        }
    }

    /// <summary>
    /// Stops background music by delegating to AudioManager
    /// </summary>
    public void StopMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found!");
        }
    }

    // void Update()
    // {
    //     if (!mode_has_been_chosen)
    //     {
    //         if (Keyboard.current.digit1Key.wasPressedThisFrame)
    //         {
    //             Debug.Log("1 pressed! Switching to Desktop mode");
    //             mode_has_been_chosen = true;

    //             desktop_mode_objects.SetActive(true);
    //             vr_mode_objects.SetActive(false);

    //             CharacterController cc = DesktopPlayer.GetComponent<CharacterController>();
    //             if (cc != null) cc.enabled = false;
    //             DesktopPlayer.transform.position = start_position.position;
    //             DesktopPlayer.transform.rotation = start_position.rotation;
    //             if (cc != null) cc.enabled = true;

    //             var dp = DesktopPlayer.GetComponent<DesktopPlayer>();
    //             if (dp != null) dp.ResetLook();

    //             StartMusic();
    //         }
    //         else if (Keyboard.current.digit2Key.wasPressedThisFrame)
    //         {
    //             mode_has_been_chosen = true;

    //             desktop_mode_objects.SetActive(false);
    //             vr_mode_objects.SetActive(true);

    //             VRPlayer.transform.position = start_position.position;
    //             VRPlayer.transform.rotation = start_position.rotation;

    //             StartMusic();
    //         }
    //     }
    // }
}
