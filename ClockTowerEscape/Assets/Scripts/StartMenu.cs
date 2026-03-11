using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    [Header("Scene Settings")]
    [Tooltip("Name of the first gameplay scene")]
    public string firstSceneName = "MainScene";
    public float fadeOutDuration = 1.5f;

    [Header("Audio Settings")]
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public AudioSource sfxAudioSource;
    public AudioSource musicAudioSource;

    [Header("Accessibility Settings")]
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

    void Start()
    {
        // Make sure cursor is visible in start menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Ensure SceneFadeManager exists
        if (SceneFadeManager.Instance == null)
            new GameObject("SceneFadeManager").AddComponent<SceneFadeManager>();

        // Show main panel, hide settings
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Load and apply saved volumes
        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

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

        // Apply volumes
        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);

        // Load and apply accessibility settings
        LoadAccessibilitySettings();
    }

    private void LoadAccessibilitySettings()
    {
        // Set up toggles
        if (teleportToggle != null)
            teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        if (moveToggle != null)
            moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);
        if (snapTurnToggle != null)
            snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggleChanged);
        if (continuousTurnToggle != null)
            continuousTurnToggle.onValueChanged.AddListener(OnContinuousTurnToggleChanged);

        // Set defaults
        if (moveToggle != null) moveToggle.isOn = true;
        if (move != null) move.SetActive(true);
        if (teleportToggle != null) teleportToggle.isOn = false;
        if (teleport != null) teleport.SetActive(false);

        // Load speed
        float savedSpeed = PlayerPrefs.GetFloat("PlayerSpeed", 5f);
        if (speedSlider != null)
        {
            speedSlider.value = savedSpeed;
            speedSlider.onValueChanged.AddListener(SetPlayerSpeed);
        }

        // Load sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }
    }

    // === BUTTON CALLBACKS ===

    public void StartGame()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(StartGameWithFade());
    }

    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // === VOLUME CONTROLS ===

    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = volume;
        
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        if (musicAudioSource != null)
            musicAudioSource.volume = volume;
        
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    // === ACCESSIBILITY CONTROLS ===

    void OnTeleportToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (moveToggle != null) moveToggle.isOn = false;
            if (teleport != null) teleport.SetActive(true);
            if (move != null) move.SetActive(false);
        }
        else
        {
            if (teleport != null) teleport.SetActive(false);
        }
    }

    void OnMoveToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (teleportToggle != null) teleportToggle.isOn = false;
            if (move != null) move.SetActive(true);
            if (teleport != null) teleport.SetActive(false);
        }
        else
        {
            if (move != null) move.SetActive(false);
        }
    }

    void OnSnapTurnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (continuousTurnToggle != null) continuousTurnToggle.isOn = false;
            if (snapTurn != null) snapTurn.SetActive(true);
            if (continuousTurn != null) continuousTurn.SetActive(false);
        }
        else
        {
            if (snapTurn != null) snapTurn.SetActive(false);
        }
    }

    void OnContinuousTurnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (snapTurnToggle != null) snapTurnToggle.isOn = false;
            if (continuousTurn != null) continuousTurn.SetActive(true);
            if (snapTurn != null) snapTurn.SetActive(false);
        }
        else
        {
            if (continuousTurn != null) continuousTurn.SetActive(false);
        }
    }

    public void SetPlayerSpeed(float speed)
    {
        PlayerPrefs.SetFloat("PlayerSpeed", speed);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    // === SCENE TRANSITION ===

    private IEnumerator StartGameWithFade()
    {
        SceneFadeManager fm = SceneFadeManager.Instance;

        // Fade to black
        if (fm != null)
            yield return StartCoroutine(fm.FadeOut(fadeOutDuration));

        // Preserve GameManager if it exists
        if (GameManager.Instance != null)
            DontDestroyOnLoad(GameManager.Instance.gameObject);

        // Load first scene (SceneFadeManager will auto-fade back in)
        SceneManager.LoadScene(firstSceneName);
    }
}
