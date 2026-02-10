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
        teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);
        snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggleChanged);
        continuousTurnToggle.onValueChanged.AddListener(OnContinuousTurnToggleChanged);

        moveToggle.isOn = true;
        move.SetActive(true);
        teleportToggle.isOn = false;
        teleport.SetActive(false);
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
            moveToggle.isOn = false;
            teleport.SetActive(true);
            move.SetActive(false);
            if (teleportInteractor != null)
            {
                teleportInteractor.enabled = true;
            }
        }
        else
        {
            teleport.SetActive(false);
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
            teleportToggle.isOn = false;
            move.SetActive(true);
            teleport.SetActive(false);
        }
        else
        {
            move.SetActive(false);
        }
    }

    void OnSnapTurnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            continuousTurnToggle.isOn = false;
            snapTurn.SetActive(true);
            continuousTurn.SetActive(false);
        }
        else
        {
            snapTurn.SetActive(false);
        }
    }

    void OnContinuousTurnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            snapTurnToggle.isOn = false;
            continuousTurn.SetActive(true);
            snapTurn.SetActive(false);
        }
        else
        {
            continuousTurn.SetActive(false);
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