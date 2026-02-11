using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject accessibilityMenuPanel;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public AudioSource sfxAudioSource;
    public AudioSource musicAudioSource;
    
    private MenuToggle menuToggle;

    void Start()
    {
        menuToggle = FindFirstObjectByType<MenuToggle>();
        
        // Load saved volumes or set defaults
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 50f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 50f);
        
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
        
        // Apply the loaded volumes
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

    public void OpenAccessibilityMenu()
    {
        mainMenuPanel.SetActive(false);
        accessibilityMenuPanel.SetActive(true);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = volume;
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = volume;
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
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
