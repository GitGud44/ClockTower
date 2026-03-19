using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum PlayMode { None, Desktop, VR }
    public PlayMode CurrentPlayMode { get; private set; } = PlayMode.None;

    private GameObject desktopPlayerRoot;
    private GameObject vrPlayerRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetGameMode(PlayMode mode)
    {
        CurrentPlayMode = mode;
    }

    public void RegisterModePlayers(GameObject desktopRoot, GameObject vrRoot)
    {
        desktopPlayerRoot = desktopRoot;
        vrPlayerRoot = vrRoot;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        StartCoroutine(RepositionPlayerAfterSceneLoad(scene));
    }

    private IEnumerator RepositionPlayerAfterSceneLoad(Scene loadedScene)
    {
        yield return null;

        GameObject activePlayer = GetActivePlayerObject();
        if (activePlayer != null)
        {
            Transform spawnPoint = FindSpawnTransformInScene(loadedScene);
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[GameManager] No spawn point found in scene '{loadedScene.name}'. Use tag 'Respawn' or name 'SpawnPoint'.");
            }
            else
            {
                CharacterController characterController = activePlayer.GetComponent<CharacterController>();
                if (characterController != null)
                    characterController.enabled = false;

                activePlayer.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

                if (characterController != null)
                {
                    characterController.enabled = true;
                    characterController.Move(Vector3.zero);
                }

                DesktopPlayer desktopPlayer = activePlayer.GetComponent<DesktopPlayer>();
                if (desktopPlayer != null)
                {
                    float savedSpeed = PlayerPrefs.GetFloat(SettingsKeys.PlayerSpeed, 5f);
                    float savedSensitivity = PlayerPrefs.GetFloat(SettingsKeys.MouseSensitivity, 2f);
                    desktopPlayer.SetMoveSpeed(savedSpeed);
                    desktopPlayer.SetSensitivity(savedSensitivity);
                    desktopPlayer.ResetLook();
                }

                Debug.Log($"[GameManager] Repositioned player '{activePlayer.name}' to spawn '{spawnPoint.name}' in scene '{loadedScene.name}'.");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] No active player found after scene load.");
        }

        if (AudioManager.Instance != null)
        {
            float savedSfxVolume = PlayerPrefs.GetFloat(SettingsKeys.SfxVolume, 0.5f);
            float savedMusicVolume = PlayerPrefs.GetFloat(SettingsKeys.MusicVolume, 0.5f);
            AudioManager.Instance.SetSFXVolume(savedSfxVolume);
            AudioManager.Instance.SetMusicVolume(savedMusicVolume);
        }

        MainMenu[] pauseMainMenus = FindObjectsOfType<MainMenu>(true);
        for (int index = 0; index < pauseMainMenus.Length; index++)
            pauseMainMenus[index].ApplySavedSettings();

        AccessibilityMenu[] accessibilityMenus = FindObjectsOfType<AccessibilityMenu>(true);
        for (int index = 0; index < accessibilityMenus.Length; index++)
            accessibilityMenus[index].ApplySavedSettings();
    }

    private GameObject GetActivePlayerObject()
    {
        if (CurrentPlayMode == PlayMode.Desktop)
        {
            if (desktopPlayerRoot != null)
                return desktopPlayerRoot;

            DesktopPlayer[] desktopPlayers = FindObjectsOfType<DesktopPlayer>(true);
            for (int index = 0; index < desktopPlayers.Length; index++)
            {
                DesktopPlayer candidate = desktopPlayers[index];
                if (candidate.gameObject.scene.name == "DontDestroyOnLoad")
                    return candidate.gameObject;
            }

            if (desktopPlayers.Length > 0)
                return desktopPlayers[0].gameObject;
        }
        else if (CurrentPlayMode == PlayMode.VR)
        {
            if (vrPlayerRoot != null)
                return vrPlayerRoot;
        }

        return null;
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

    private Transform FindSpawnTransformInScene(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();

        for (int index = 0; index < rootObjects.Length; index++)
        {
            Transform taggedSpawn = FindChildByTag(rootObjects[index].transform, "Respawn");
            if (taggedSpawn != null)
                return taggedSpawn;
        }

        for (int index = 0; index < rootObjects.Length; index++)
        {
            Transform namedSpawn = FindChildByName(rootObjects[index].transform, "SpawnPoint");
            if (namedSpawn != null)
                return namedSpawn;
        }

        return null;
    }

    private Transform FindChildByTag(Transform root, string tag)
    {
        if (root.CompareTag(tag))
            return root;

        for (int index = 0; index < root.childCount; index++)
        {
            Transform match = FindChildByTag(root.GetChild(index), tag);
            if (match != null)
                return match;
        }

        return null;
    }

    private Transform FindChildByName(Transform root, string objectName)
    {
        if (root.name == objectName)
            return root;

        for (int index = 0; index < root.childCount; index++)
        {
            Transform match = FindChildByName(root.GetChild(index), objectName);
            if (match != null)
                return match;
        }

        return null;
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
