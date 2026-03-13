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

        if (moveToggle != null) moveToggle.isOn = true;
        if (move != null) move.SetActive(true);
        if (teleportToggle != null) teleportToggle.isOn = false;
        if (teleport != null) teleport.SetActive(false);
        if (teleportInteractor != null)
        {
            teleportInteractor.enabled = false;
        }

        desktopPlayer = FindFirstObjectByType<DesktopPlayer>();
        if (speedSlider != null)
        {
            float savedSpeed = PlayerPrefs.GetFloat("PlayerSpeed", 5f);
            speedSlider.value = savedSpeed;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(savedSpeed);
        }
        if (sensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
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

    void OnTeleportToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (moveToggle != null) moveToggle.isOn = false;
            if (teleport != null) teleport.SetActive(true);
            if (move != null) move.SetActive(false);
            if (teleportInteractor != null)
            {
                teleportInteractor.enabled = true;
            }
        }
        else
        {
            if (teleport != null) teleport.SetActive(false);
            if (teleportInteractor != null)
            {
                teleportInteractor.enabled = false;
            }
        }
    }

    void OnMoveToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (teleportToggle != null) teleportToggle.isOn = false;
            if (move != null) move.SetActive(true);
            if (teleport != null) teleport.SetActive(false);
        }
        else
        {
            if (move != null) move.SetActive(false);
        }
    }

    void OnSnapTurnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (continuousTurnToggle != null) continuousTurnToggle.isOn = false;
            if (snapTurn != null) snapTurn.SetActive(true);
            if (continuousTurn != null) continuousTurn.SetActive(false);
        }
        else
        {
            if (snapTurn != null) snapTurn.SetActive(false);
        }
    }

    void OnContinuousTurnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            if (snapTurnToggle != null) snapTurnToggle.isOn = false;
            if (continuousTurn != null) continuousTurn.SetActive(true);
            if (snapTurn != null) snapTurn.SetActive(false);
        }
        else
        {
            if (continuousTurn != null) continuousTurn.SetActive(false);
        }
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
