using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    private const float AudioSliderMaxValue = 10f;

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
        float savedSfxVolume = SettingsState.GetSfxVolume();
        float savedMusicVolume = SettingsState.GetMusicVolume();
        float sfxSliderValue = Mathf.Clamp01(savedSfxVolume) * AudioSliderMaxValue;
        float musicSliderValue = Mathf.Clamp01(savedMusicVolume) * AudioSliderMaxValue;

        ConfigureAudioSlider(sfxVolumeSlider, SetSFXVolume, sfxSliderValue);
        ConfigureAudioSlider(musicVolumeSlider, SetMusicVolume, musicSliderValue);

        SetSFXVolume(sfxSliderValue);
        SetMusicVolume(musicSliderValue);

        LoadAccessibilitySettings();
    }

    private void ConfigureAudioSlider(Slider slider, UnityAction<float> listener, float value)
    {
        if (slider == null)
            return;

        slider.onValueChanged.RemoveListener(listener);
        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = AudioSliderMaxValue;
        slider.SetValueWithoutNotify(Mathf.Clamp(value, slider.minValue, slider.maxValue));
        slider.onValueChanged.AddListener(listener);
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

        bool useTeleport = SettingsState.GetUseTeleport();
        bool useContinuousTurn = SettingsState.GetUseContinuousTurn();

        ApplyLocomotionMode(useTeleport, false);
        ApplyTurnMode(useContinuousTurn, false);

        float savedSpeed = SettingsState.GetPlayerSpeed();
        if (speedSlider != null)
        {
            speedSlider.SetValueWithoutNotify(savedSpeed);
            speedSlider.onValueChanged.AddListener(SetPlayerSpeed);
        }

        float savedSensitivity = SettingsState.GetMouseSensitivity();
        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(savedSensitivity);
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
        float normalizedVolume = Mathf.Clamp01(volume / AudioSliderMaxValue);
        SettingsState.SetSfxVolume(normalizedVolume);
    }

    public void SetMusicVolume(float volume)
    {
        float normalizedVolume = Mathf.Clamp01(volume / AudioSliderMaxValue);
        SettingsState.SetMusicVolume(normalizedVolume);
    }

    private void ApplyLocomotionMode(bool useTeleport, bool save)
    {
        if (teleportToggle != null) teleportToggle.SetIsOnWithoutNotify(useTeleport);
        if (moveToggle != null) moveToggle.SetIsOnWithoutNotify(!useTeleport);

        if (teleport != null) teleport.SetActive(useTeleport);
        if (move != null) move.SetActive(!useTeleport);

        if (save)
        {
            SettingsState.SetLocomotionMode(useTeleport);
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
            SettingsState.SetTurnMode(useContinuousTurn);
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
        SettingsState.SetPlayerSpeed(speed);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        SettingsState.SetMouseSensitivity(sensitivity);
    }

    private IEnumerator StartGameWithFade()
    {
        SceneFadeManager fm = SceneFadeManager.Instance;

        if (fm != null)
            yield return StartCoroutine(fm.FadeOut(fadeOutDuration));

        if (GameManager.Instance != null)
        {
            DontDestroyOnLoad(GameManager.Instance.gameObject);
            GameManager.Instance.StartMusic();
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartMusic();
        }

        SceneManager.LoadScene(firstSceneName);
    }
}
