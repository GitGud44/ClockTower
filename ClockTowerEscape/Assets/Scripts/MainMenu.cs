using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const float AudioSliderMaxValue = 10f;

    public GameObject mainMenuPanel;
    public GameObject accessibilityMenuPanel;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    
    private MenuToggle menuToggle;

    void Start()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();

        RefreshAudioSettings();
    }

    void OnEnable()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();
        RefreshAudioSettings();
    }

    public void ResumeGame()
    {
        if (menuToggle != null)
        {
            menuToggle.CloseMenu();
        }
    }

    public void OpenAccessibilityMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (accessibilityMenuPanel != null) accessibilityMenuPanel.SetActive(true);
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

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void RefreshAudioSettings()
    {
        float savedSfxVolume = SettingsState.GetSfxVolume();
        float savedMusicVolume = SettingsState.GetMusicVolume();
        float sfxSliderValue = Mathf.Clamp01(savedSfxVolume) * AudioSliderMaxValue;
        float musicSliderValue = Mathf.Clamp01(savedMusicVolume) * AudioSliderMaxValue;

        ConfigureAudioSlider(sfxVolumeSlider, SetSFXVolume, sfxSliderValue);
        ConfigureAudioSlider(musicVolumeSlider, SetMusicVolume, musicSliderValue);

        SettingsState.ApplyRuntimeSettings();
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
}
