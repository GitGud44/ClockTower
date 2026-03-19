using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject settingsPanel;

    public string firstSceneName = "MainScene";
    public float fadeOutDuration = 1.5f;

    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    public GameObject teleport;
    public GameObject move;
    public GameObject snapTurn;
    public GameObject continuousTurn;
    public Toggle teleportToggle;
    public Toggle moveToggle;
    public Toggle snapTurnToggle;
    public Toggle continuousTurnToggle;
    public Slider speedSlider;
    public Slider sensitivitySlider;

    private bool isTransitioning = false;

    void Update()
    {
        if (isTransitioning) return;

        bool shouldKeepCursorUnlocked = GameManager.Instance == null ||
                                        GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.Desktop;

        if (shouldKeepCursorUnlocked)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (SceneFadeManager.Instance == null)
            new GameObject("SceneFadeManager").AddComponent<SceneFadeManager>();

        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        float sfxVolume = PlayerPrefs.GetFloat(SettingsKeys.SfxVolume, 0.5f);
        float musicVolume = PlayerPrefs.GetFloat(SettingsKeys.MusicVolume, 0.5f);

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);

        LoadAccessibilitySettings();
    }

    private void LoadAccessibilitySettings()
    {
        if (teleportToggle != null)
            teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        if (moveToggle != null)
            moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);
        if (snapTurnToggle != null)
            snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggleChanged);
        if (continuousTurnToggle != null)
            continuousTurnToggle.onValueChanged.AddListener(OnContinuousTurnToggleChanged);

        bool useTeleport = PlayerPrefs.GetInt(SettingsKeys.LocomotionMode, 0) == 1;
        bool useContinuousTurn = PlayerPrefs.GetInt(SettingsKeys.TurnMode, 0) == 1;

        ApplyLocomotionMode(useTeleport, false);
        ApplyTurnMode(useContinuousTurn, false);

        float savedSpeed = PlayerPrefs.GetFloat(SettingsKeys.PlayerSpeed, 5f);
        if (speedSlider != null)
        {
            speedSlider.value = savedSpeed;
            speedSlider.onValueChanged.AddListener(SetPlayerSpeed);
        }

        float savedSensitivity = PlayerPrefs.GetFloat(SettingsKeys.MouseSensitivity, 2f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }
    }

    public void StartGame()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        // Lock and hide cursor when leaving the menu
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // If Desktop mode was chosen, turn on the DesktopPlayer script now
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.Desktop)
        {
            // Older Unity versions use the bool overload for includeInactive
            var desktopController = FindObjectOfType<DesktopPlayer>(true);
            if (desktopController != null && desktopController.enabled == false)
            {
                desktopController.enabled = true;
            }
        }

        StartCoroutine(StartGameWithFade());
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }


    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
        
        PlayerPrefs.SetFloat(SettingsKeys.SfxVolume, volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);
        
        PlayerPrefs.SetFloat(SettingsKeys.MusicVolume, volume);
        PlayerPrefs.Save();
    }

    private void ApplyLocomotionMode(bool useTeleport, bool save)
    {
        if (teleportToggle != null) teleportToggle.SetIsOnWithoutNotify(useTeleport);
        if (moveToggle != null) moveToggle.SetIsOnWithoutNotify(!useTeleport);

        if (teleport != null) teleport.SetActive(useTeleport);
        if (move != null) move.SetActive(!useTeleport);

        if (save)
        {
            PlayerPrefs.SetInt(SettingsKeys.LocomotionMode, useTeleport ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void ApplyTurnMode(bool useContinuousTurn, bool save)
    {
        if (continuousTurnToggle != null) continuousTurnToggle.SetIsOnWithoutNotify(useContinuousTurn);
        if (snapTurnToggle != null) snapTurnToggle.SetIsOnWithoutNotify(!useContinuousTurn);

        if (continuousTurn != null) continuousTurn.SetActive(useContinuousTurn);
        if (snapTurn != null) snapTurn.SetActive(!useContinuousTurn);

        if (save)
        {
            PlayerPrefs.SetInt(SettingsKeys.TurnMode, useContinuousTurn ? 1 : 0);
            PlayerPrefs.Save();
        }
    }


    void OnTeleportToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyLocomotionMode(true, true);
    }

    void OnMoveToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyLocomotionMode(false, true);
    }

    void OnSnapTurnToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyTurnMode(false, true);
    }

    void OnContinuousTurnToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyTurnMode(true, true);
    }

    public void SetPlayerSpeed(float speed)
    {
        PlayerPrefs.SetFloat(SettingsKeys.PlayerSpeed, speed);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SettingsKeys.MouseSensitivity, sensitivity);
        PlayerPrefs.Save();
    }

    private IEnumerator StartGameWithFade()
    {
        SceneFadeManager fm = SceneFadeManager.Instance;

        if (fm != null)
            yield return StartCoroutine(fm.FadeOut(fadeOutDuration));

        if (GameManager.Instance != null)
        {
            DontDestroyOnLoad(GameManager.Instance.gameObject);
        }

        SceneManager.LoadScene(firstSceneName);
    }
}
