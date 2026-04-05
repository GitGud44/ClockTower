using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeMenu : MonoBehaviour
{
    //the 3 main panels
    public GameObject modeMenuPanel;
    public GameObject desktopMenuUI;
    public GameObject vrMenuUI;

    //player prefabs
    public GameObject desktopPlayerPrefab;
    public GameObject vrPlayerPrefab;

    //both vr and desktop pause menus
    public GameObject desktopPauseMenu;
    public GameObject vrPauseMenu;

    //menu camera (disabled after mode selection)
    public Camera menuCamera;

    public void StartDesktopMode()
    {
        //set the game mode in GameManager
        GameManager.Instance.SetGameMode(GameManager.PlayMode.Desktop);
        EnsurePauseMenu(GameManager.PlayMode.Desktop);

        //show desktop UI and hide VR UI
        if (desktopMenuUI != null) desktopMenuUI.SetActive(true);
        if (vrMenuUI != null) vrMenuUI.SetActive(false);

        //enable desktop player controls
        if (desktopPlayerPrefab != null) desktopPlayerPrefab.SetActive(true);
        if (vrPlayerPrefab != null) vrPlayerPrefab.SetActive(false);

        //mark desktop player as persistent across scenes, so it doenst delete between scen changes!!!
        if (desktopPlayerPrefab != null)
            DontDestroyOnLoad(desktopPlayerPrefab);

        //turn off desktop controls until player leaves the menu
        var desktopController = desktopPlayerPrefab != null ? desktopPlayerPrefab.GetComponent<DesktopPlayer>() : null;
        if (desktopController != null) desktopController.enabled = false;

        //enable cursor so u can click the mode
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //hide the game mode selection menu
        HideModeMenu();
    }

    //same logic as above but for vr
    public void StartVRMode()
    {
        GameManager.Instance.SetGameMode(GameManager.PlayMode.VR);
        EnsurePauseMenu(GameManager.PlayMode.VR);

        if (vrMenuUI != null) vrMenuUI.SetActive(true);
        if (desktopMenuUI != null) desktopMenuUI.SetActive(false);

        if (vrPlayerPrefab != null) vrPlayerPrefab.SetActive(true);
        if (desktopPlayerPrefab != null) desktopPlayerPrefab.SetActive(false);

        if (vrPlayerPrefab != null)
            DontDestroyOnLoad(vrPlayerPrefab);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        HideModeMenu();
    }

//hides menu and camera
    private void HideModeMenu()
    {
        if (modeMenuPanel != null) {
            modeMenuPanel.SetActive(false);
        }
        if (menuCamera != null) {
            menuCamera.gameObject.SetActive(false);
        }
    }

    //makes sur ethat the correct pause menu is enabled and if it doenst exist it creates it and makes it persistet
    private void EnsurePauseMenu(GameManager.PlayMode mode)
    {
        PauseMenuPersistence pauseMenu = PauseMenuPersistence.Instance;
        GameObject desiredPauseMenu = mode == GameManager.PlayMode.Desktop ? desktopPauseMenu : vrPauseMenu;

        if (pauseMenu != null && pauseMenu.MenuMode != mode)
        {
            Destroy(pauseMenu.gameObject);
            pauseMenu = null;
        }

        if (pauseMenu == null)
        {
            if (desiredPauseMenu == null)
                return;

            GameObject pauseMenuObject = GetOrCreatePauseMenuObject(desiredPauseMenu);
            if (pauseMenuObject == null)
                return;

            GameObject pauseMenuRoot = pauseMenuObject.transform.root.gameObject;

            if (pauseMenuRoot.scene.IsValid() && pauseMenuRoot.scene.isLoaded)
                DontDestroyOnLoad(pauseMenuRoot);

            pauseMenu = pauseMenuRoot.GetComponent<PauseMenuPersistence>();
            if (pauseMenu == null)
                pauseMenu = pauseMenuRoot.AddComponent<PauseMenuPersistence>();
        }

        if (pauseMenu != null)
        {
            pauseMenu.Initialize(mode);
            pauseMenu.gameObject.SetActive(true);
        }
    }

    //applies the settings from the settings menu to the correct pause menu
    private GameObject GetOrCreatePauseMenuObject(GameObject pauseMenuReference)
    {
        if (pauseMenuReference == null)
            return null;

        if (pauseMenuReference.scene.IsValid() && pauseMenuReference.scene.isLoaded)
            return pauseMenuReference;

        return Instantiate(pauseMenuReference);
    }
    
    //quits game if quit button is pressed
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
