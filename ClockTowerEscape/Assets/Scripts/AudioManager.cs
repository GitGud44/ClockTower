using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioSource musicSource;

    public AudioClip backgroundMusic;
    public bool playMusicOnAwake = true;

    //checks to see that theres only one audiomanager per scene and starts music + applies previous sound effect settings
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureOwnedAudioSources();

        SetSFXVolume(SettingsState.GetSfxVolume());
        SetMusicVolume(SettingsState.GetMusicVolume());

        if (playMusicOnAwake)
            StartMusic();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //deletes the audiomanager if the scene changes if one already exists
    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //when a new scene loads, applies existing audio settings, and plays music
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureOwnedAudioSources();
        SetSFXVolume(SettingsState.GetSfxVolume());
        SetMusicVolume(SettingsState.GetMusicVolume());

        if (playMusicOnAwake)
            StartMusic();
    }

    //checks to see that the audiomanager gameobject has audio sources on it, if not it adds them
    private void EnsureOwnedAudioSources()
    {
        if (sfxSource == null || sfxSource.gameObject != gameObject)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicSource == null || musicSource.gameObject != gameObject || musicSource == sfxSource)
        {
            AudioSource[] allSources = GetComponents<AudioSource>();
            musicSource = null;

            for (int index = 0; index < allSources.Length; index++)
            {
                if (allSources[index] != sfxSource)
                {
                    musicSource = allSources[index];
                    break;
                }
            }

            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }

    //plays the sound effect clip at the specific volume that was set
    public void PlayClip(AudioClip clip, float volumeScale = 1f)
    {
        EnsureOwnedAudioSources();

        if (clip == null)
            return;

        if (sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    //if sfx is attached to object, it just plays the sound clip from there instead
    public void PlaySpatialClip(AudioClip clip, Vector3 worldPosition, float volumeScale = 1f, float spatialBlend = 1f)
    {
        if (clip == null)
        {
            return;
        }

        GameObject tempAudioObject = new GameObject($"TempSpatialSfx_{clip.name}");
        tempAudioObject.transform.position = worldPosition;

        AudioSource tempSource = tempAudioObject.AddComponent<AudioSource>();
        tempSource.playOnAwake = false;
        tempSource.clip = clip;
        tempSource.spatialBlend = Mathf.Clamp01(spatialBlend);
        tempSource.rolloffMode = AudioRolloffMode.Logarithmic;
        tempSource.minDistance = 1f;
        tempSource.maxDistance = 20f;

        float baseSfxVolume = sfxSource != null ? sfxSource.volume : 1f;
        tempSource.volume = Mathf.Clamp01(baseSfxVolume * Mathf.Clamp01(volumeScale));

        if (sfxSource != null)
        {
            AudioMixerGroup sfxMixerGroup = sfxSource.outputAudioMixerGroup;
            if (sfxMixerGroup != null)
                tempSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        tempSource.Play();
        Destroy(tempAudioObject, clip.length + 0.1f);
    }

    //starts music if it exists and isnt already playing
    public void StartMusic()
    {
        EnsureOwnedAudioSources();

        if (musicSource == null || backgroundMusic == null)
            return;

        if (musicSource.clip != backgroundMusic)
            musicSource.clip = backgroundMusic;

        if (!musicSource.isPlaying)
        {
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    //stops music if its playing
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    //pauses music if its playing
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    //resumes music if its paused
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void SetSFXVolume(float volume)
    {
        EnsureOwnedAudioSources();

        if (sfxSource != null)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            bool shouldMute = clampedVolume <= 0f;
            // logarithmic volume so the slider feels natural instead of linear
            // based on Mathf.Log10(slider) * 20 formula from https://www.youtube.com/watch?v=xNHSGMKtlv4
            // but that formula is for AudioMixer decibels, since I'm setting audioSource.volume directly (0-1 range)
            // I use a power curve which does the same thing: makes the slider feel even across the whole range
            sfxSource.volume = clampedVolume * clampedVolume;
            sfxSource.mute = shouldMute;
        }
    }

    public void SetMusicVolume(float volume)
    {
        EnsureOwnedAudioSources();

        if (musicSource != null)
        {
            float clampedVolume = Mathf.Clamp01(volume);
            bool shouldMute = clampedVolume <= 0f;
            // same logarithmic curve as SFX, from the video
            musicSource.volume = clampedVolume * clampedVolume;
            musicSource.mute = shouldMute;
        }
    }
}
