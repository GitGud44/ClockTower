using UnityEngine;
using UnityEngine.UI;

public class AccessibilityMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject accessibilityMenuPanel;
    public GameObject teleport;
    public GameObject move;

    public Toggle teleportToggle;
    public Toggle moveToggle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);

        moveToggle.isOn = true;
        move.SetActive(true);
        teleportToggle.isOn = false;
        teleport.SetActive(false);
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
        }
        else
        {
            teleport.SetActive(false);
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
}