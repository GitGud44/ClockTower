using UnityEngine;

public static class SettingsState
{
    public const float DefaultSfxVolume = 0.5f;
    public const float DefaultMusicVolume = 0.5f;
    public const float DefaultPlayerSpeed = 5f;
    public const float DefaultMouseSensitivity = 2f;

    public static float GetSfxVolume()
    {
        return PlayerPrefs.GetFloat(SettingsKeys.SfxVolume, DefaultSfxVolume);
    }

    public static float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(SettingsKeys.MusicVolume, DefaultMusicVolume);
    }

    public static float GetPlayerSpeed()
    {
        return PlayerPrefs.GetFloat(SettingsKeys.PlayerSpeed, DefaultPlayerSpeed);
    }

    public static float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat(SettingsKeys.MouseSensitivity, DefaultMouseSensitivity);
    }

    public static bool GetUseTeleport()
    {
        return PlayerPrefs.GetInt(SettingsKeys.LocomotionMode, 0) == 1;
    }

    public static bool GetUseContinuousTurn()
    {
        return PlayerPrefs.GetInt(SettingsKeys.TurnMode, 0) == 1;
    }

    public static void SetSfxVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);

        PlayerPrefs.SetFloat(SettingsKeys.SfxVolume, volume);
        PlayerPrefs.Save();
    }

    public static void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);

        PlayerPrefs.SetFloat(SettingsKeys.MusicVolume, volume);
        PlayerPrefs.Save();
    }

    public static void SetPlayerSpeed(float speed, DesktopPlayer desktopPlayer = null)
    {
        if (desktopPlayer == null)
            desktopPlayer = Object.FindFirstObjectByType<DesktopPlayer>();

        if (desktopPlayer != null)
            desktopPlayer.SetMoveSpeed(speed);

        PlayerPrefs.SetFloat(SettingsKeys.PlayerSpeed, speed);
        PlayerPrefs.Save();
    }

    public static void SetMouseSensitivity(float sensitivity, DesktopPlayer desktopPlayer = null)
    {
        if (desktopPlayer == null)
            desktopPlayer = Object.FindFirstObjectByType<DesktopPlayer>();

        if (desktopPlayer != null)
            desktopPlayer.SetSensitivity(sensitivity);

        PlayerPrefs.SetFloat(SettingsKeys.MouseSensitivity, sensitivity);
        PlayerPrefs.Save();
    }

    public static void SetLocomotionMode(bool useTeleport)
    {
        PlayerPrefs.SetInt(SettingsKeys.LocomotionMode, useTeleport ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetTurnMode(bool useContinuousTurn)
    {
        PlayerPrefs.SetInt(SettingsKeys.TurnMode, useContinuousTurn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ApplyRuntimeSettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(GetSfxVolume());
            AudioManager.Instance.SetMusicVolume(GetMusicVolume());
        }

        DesktopPlayer desktopPlayer = Object.FindFirstObjectByType<DesktopPlayer>();
        if (desktopPlayer != null)
        {
            desktopPlayer.SetMoveSpeed(GetPlayerSpeed());
            desktopPlayer.SetSensitivity(GetMouseSensitivity());
        }
    }
}
