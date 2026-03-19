using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    
    private MenuToggle menuToggle;
    private bool listenersRegistered = false;

    void Start()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();
        RegisterListenersIfNeeded();
        ApplySavedSettings();
    }

    void OnEnable()
    {
        RegisterListenersIfNeeded();
        ApplySavedSettings();
    }

    private void RegisterListenersIfNeeded()
    {
        if (listenersRegistered)
            return;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        listenersRegistered = true;
    }

    public void ApplySavedSettings()
    {
        float sfxVolume = PlayerPrefs.GetFloat(SettingsKeys.SfxVolume, 0.5f);
        float musicVolume = PlayerPrefs.GetFloat(SettingsKeys.MusicVolume, 0.5f);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);

        SetSFXVolume(sfxVolume);
        SetMusicVolume(musicVolume);
    }

    public void ResumeGame()
    {
        if (menuToggle != null)
        {
            menuToggle.CloseMenu();
        }
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

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
