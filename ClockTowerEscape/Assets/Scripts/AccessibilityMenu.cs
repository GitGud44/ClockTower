using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AccessibilityMenu : MonoBehaviour
{
    public GameObject teleport;
    public GameObject move;
    public GameObject snapTurn;
    public GameObject continuousTurn;
    public XRRayInteractor teleportInteractor;

    public Toggle teleportToggle;
    public Toggle moveToggle;
    public Toggle snapTurnToggle;
    public Toggle continuousTurnToggle;
    public Slider speedSlider;
    public Slider sensitivitySlider;

    private DesktopPlayer desktopPlayer;
    private bool listenersRegistered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplySavedSettings();
    }

    void OnEnable()
    {
        ApplySavedSettings();
    }

    public void ApplySavedSettings()
    {
        RegisterListenersIfNeeded();
        RefreshFromPrefs();
    }

    private void RegisterListenersIfNeeded()
    {
        if (listenersRegistered)
            return;

        if (teleportToggle != null) teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        if (moveToggle != null) moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);
        if (snapTurnToggle != null) snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggleChanged);
        if (continuousTurnToggle != null) continuousTurnToggle.onValueChanged.AddListener(OnContinuousTurnToggleChanged);
        if (speedSlider != null) speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        listenersRegistered = true;
    }

    private void RefreshFromPrefs()
    {
        desktopPlayer = FindObjectOfType<DesktopPlayer>(true);

        bool useTeleport = PlayerPrefs.GetInt(SettingsKeys.LocomotionMode, 0) == 1;
        bool useContinuousTurn = PlayerPrefs.GetInt(SettingsKeys.TurnMode, 0) == 1;
        ApplyLocomotionMode(useTeleport, false);
        ApplyTurnMode(useContinuousTurn, false);

        if (speedSlider != null)
        {
            float savedSpeed = PlayerPrefs.GetFloat(SettingsKeys.PlayerSpeed, 5f);
            speedSlider.SetValueWithoutNotify(savedSpeed);
            OnSpeedChanged(savedSpeed);
        }

        if (sensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat(SettingsKeys.MouseSensitivity, 2f);
            sensitivitySlider.SetValueWithoutNotify(savedSensitivity);
            OnSensitivityChanged(savedSensitivity);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    private void ApplyLocomotionMode(bool useTeleport, bool save)
    {
        if (teleportToggle != null) teleportToggle.SetIsOnWithoutNotify(useTeleport);
        if (moveToggle != null) moveToggle.SetIsOnWithoutNotify(!useTeleport);

        if (teleport != null) teleport.SetActive(useTeleport);
        if (move != null) move.SetActive(!useTeleport);

        if (teleportInteractor != null)
            teleportInteractor.enabled = useTeleport;

        if (save)
        {
            PlayerPrefs.SetInt(SettingsKeys.LocomotionMode, useTeleport ? 1 : 0);
            PlayerPrefs.Save();
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
            PlayerPrefs.SetInt(SettingsKeys.TurnMode, useContinuousTurn ? 1 : 0);
            PlayerPrefs.Save();
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

    void OnSpeedChanged(float speed)
    {
        if (desktopPlayer == null)
            desktopPlayer = FindObjectOfType<DesktopPlayer>(true);

        if (desktopPlayer != null)
        {
            desktopPlayer.SetMoveSpeed(speed);
        }
        PlayerPrefs.SetFloat(SettingsKeys.PlayerSpeed, speed);
        PlayerPrefs.Save();
    }

    void OnSensitivityChanged(float sensitivity)
    {
        if (desktopPlayer == null)
            desktopPlayer = FindObjectOfType<DesktopPlayer>(true);

        if (desktopPlayer != null)
        {
            desktopPlayer.SetSensitivity(sensitivity);
        }
        PlayerPrefs.SetFloat(SettingsKeys.MouseSensitivity, sensitivity);
        PlayerPrefs.Save();
    }
}
