using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AccessibilityMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject accessibilityMenuPanel;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (teleportToggle != null) teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        if (moveToggle != null) moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);
        if (snapTurnToggle != null) snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggleChanged);
        if (continuousTurnToggle != null) continuousTurnToggle.onValueChanged.AddListener(OnContinuousTurnToggleChanged);

        bool useTeleport = PlayerPrefs.GetInt(SettingsKeys.LocomotionMode, 0) == 1;
        bool useContinuousTurn = PlayerPrefs.GetInt(SettingsKeys.TurnMode, 0) == 1;
        ApplyLocomotionMode(useTeleport, false);
        ApplyTurnMode(useContinuousTurn, false);

        desktopPlayer = FindFirstObjectByType<DesktopPlayer>();
        if (speedSlider != null)
        {
            float savedSpeed = PlayerPrefs.GetFloat(SettingsKeys.PlayerSpeed, 5f);
            speedSlider.value = savedSpeed;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(savedSpeed);
        }
        if (sensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat(SettingsKeys.MouseSensitivity, 2f);
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            OnSensitivityChanged(savedSensitivity);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void BackToMainMenu()
    {
        mainMenuPanel.SetActive(true);
        accessibilityMenuPanel.SetActive(false);
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
        if (desktopPlayer != null)
        {
            desktopPlayer.SetMoveSpeed(speed);
        }
        PlayerPrefs.SetFloat("PlayerSpeed", speed);
        PlayerPrefs.Save();
    }

    void OnSensitivityChanged(float sensitivity)
    {
        if (desktopPlayer != null)
        {
            desktopPlayer.SetSensitivity(sensitivity);
        }
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }
}
