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

        // Hide mode menu
        HideModeMenu();
    }

    private void HideModeMenu()
    {
        if (modeMenuPanel != null) modeMenuPanel.SetActive(false);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}

