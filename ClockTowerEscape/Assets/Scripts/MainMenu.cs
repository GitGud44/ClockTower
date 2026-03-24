using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject accessibilityMenuPanel;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    
    private MenuToggle menuToggle;

    void Start()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

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
        SettingsState.SetSfxVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        SettingsState.SetMusicVolume(volume);
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
        float sfxVolume = SettingsState.GetSfxVolume();
        float musicVolume = SettingsState.GetMusicVolume();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);

        SettingsState.ApplyRuntimeSettings();
    }
}
