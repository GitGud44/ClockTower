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

    //finds the menu toggle and applies the saved audio settinhgs
    void Start()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();

        RefreshAudioSettings();
    }

    //refreshes audio settings if needed
    void OnEnable()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();
        RefreshAudioSettings();
    }

    //resume
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

    //sets sfx volume based on slider
    public void SetSFXVolume(float volume)
    {
        float normalizedVolume = Mathf.Clamp01(volume / AudioSliderMaxValue);
        SettingsState.SetSfxVolume(normalizedVolume);
    }

    //same but with music
    public void SetMusicVolume(float volume)
    {
        float normalizedVolume = Mathf.Clamp01(volume / AudioSliderMaxValue);
        SettingsState.SetMusicVolume(normalizedVolume);
    }

    //quit game
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    //gets the saved settings and applies them 
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

    //sets slider configurations 
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
