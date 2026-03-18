using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("SFX Clips")]
    public AudioClip[] soundEffects = new AudioClip[10]; // Adjust size as needed

    [Header("Music Clips")]
    public AudioClip backgroundMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize audio sources if not assigned
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a sound effect by index
    /// </summary>
    /// <param name="soundIndex">Index of the sound effect in the soundEffects array</param>
    public void PlaySound(int soundIndex)
    {
        if (sfxSource != null && soundEffects != null && soundIndex >= 0 && soundIndex < soundEffects.Length)
        {
            if (soundEffects[soundIndex] != null)
            {
                sfxSource.PlayOneShot(soundEffects[soundIndex]);
            }
            else
            {
                Debug.LogWarning($"Sound effect at index {soundIndex} is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid sound index: {soundIndex}");
        }
    }

    /// <summary>
    /// Plays a sound effect with specified volume
    /// </summary>
    public void PlaySound(int soundIndex, float volume)
    {
        if (sfxSource != null && soundEffects != null && soundIndex >= 0 && soundIndex < soundEffects.Length)
        {
            if (soundEffects[soundIndex] != null)
            {
                sfxSource.PlayOneShot(soundEffects[soundIndex], volume);
            }
            else
            {
                Debug.LogWarning($"Sound effect at index {soundIndex} is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid sound index: {soundIndex}");
        }
    }

    /// <summary>
    /// Starts playing background music
    /// </summary>
    public void StartMusic()
    {
        if (musicSource != null && backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stops background music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Pauses background music
    /// </summary>
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    /// <summary>
    /// Resumes background music
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    /// <summary>
    /// Sets the volume of sound effects (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Sets the volume of music (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
}
