using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Music Clips")]
    public AudioClip backgroundMusic;
    public bool playMusicOnAwake = true;

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

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureOwnedAudioSources();
        SetSFXVolume(SettingsState.GetSfxVolume());
        SetMusicVolume(SettingsState.GetMusicVolume());

        if (playMusicOnAwake)
            StartMusic();
    }

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

    public void PlayClip(AudioClip clip, float volumeScale = 1f)
    {
        EnsureOwnedAudioSources();

        if (clip == null)
            return;

        if (sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

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


    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }


    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }


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
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        EnsureOwnedAudioSources();

        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
}
