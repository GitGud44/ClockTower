using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameModeMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject modeMenuPanel;
    public GameObject desktopMenuUI;
    public GameObject vrMenuUI;

    [Header("Player Prefabs")]
    public GameObject desktopPlayerPrefab;
    public GameObject vrPlayerPrefab;

    [Header("Cameras")]
    public Camera menuCamera;

    public void StartDesktopMode()
    {
        // Set the game mode in GameManager
        GameManager.Instance.SetGameMode(GameManager.PlayMode.Desktop);

        // Show desktop UI and hide VR UI
        if (desktopMenuUI != null) desktopMenuUI.SetActive(true);
        if (vrMenuUI != null) vrMenuUI.SetActive(false);

        // Enable desktop player controls
        if (desktopPlayerPrefab != null) desktopPlayerPrefab.SetActive(true);
        if (vrPlayerPrefab != null) vrPlayerPrefab.SetActive(false);

        // Mark desktop player as persistent across scenes
        if (desktopPlayerPrefab != null)
            DontDestroyOnLoad(desktopPlayerPrefab);

        // Keep desktop input off during menu navigation so cursor stays free
        var desktopController = desktopPlayerPrefab != null ? desktopPlayerPrefab.GetComponent<DesktopPlayer>() : null;
        if (desktopController != null) desktopController.enabled = false;

        // Enable cursor for desktop menu navigation
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Hide mode menu
        HideModeMenu();
    }

    public void StartVRMode()
    {
        // Set the game mode in GameManager
        GameManager.Instance.SetGameMode(GameManager.PlayMode.VR);

        // Show VR UI and hide desktop UI
        if (vrMenuUI != null) vrMenuUI.SetActive(true);
        if (desktopMenuUI != null) desktopMenuUI.SetActive(false);

        // Enable VR player controls
        if (vrPlayerPrefab != null) vrPlayerPrefab.SetActive(true);
        if (desktopPlayerPrefab != null) desktopPlayerPrefab.SetActive(false);

        // Mark VR player as persistent across scenes
        if (vrPlayerPrefab != null)
            DontDestroyOnLoad(vrPlayerPrefab);

        // Hide cursor for VR mode
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Hide mode menu
        HideModeMenu();
    }

    private void HideModeMenu()
    {
        if (modeMenuPanel != null) modeMenuPanel.SetActive(false);
        if (menuCamera != null) menuCamera.gameObject.SetActive(false);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

