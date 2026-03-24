using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        StartCoroutine(RepositionPlayerAfterSceneLoad(scene));
    }

    private IEnumerator RepositionPlayerAfterSceneLoad(Scene loadedScene)
    {
        yield return null;

        SettingsState.ApplyRuntimeSettings();

        GameObject activePlayer = GetActivePlayerObject();
        if (activePlayer == null)
        {
            Debug.LogWarning("[GameManager] No active player found after scene load.");
            yield break;
        }

        Transform spawnPoint = FindSpawnTransformInScene(loadedScene);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[GameManager] No spawn point found in scene '{loadedScene.name}'. Use tag 'Respawn' or name 'SpawnPoint'.");
            yield break;
        }

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
            desktopPlayer.SetMoveSpeed(SettingsState.GetPlayerSpeed());
            desktopPlayer.SetSensitivity(SettingsState.GetMouseSensitivity());
            desktopPlayer.ResetLook();
        }

        Debug.Log($"[GameManager] Repositioned player '{activePlayer.name}' to spawn '{spawnPoint.name}' in scene '{loadedScene.name}'.");
    }

    private GameObject GetActivePlayerObject()
    {
        if (CurrentPlayMode == PlayMode.Desktop)
        {
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

        return null;
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

}
