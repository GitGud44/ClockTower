using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    public GameObject UIMenu;
    bool isMenuOpen = false;
    void Start()
    {
        UIMenu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isMenuOpen = !isMenuOpen;
            UIMenu.SetActive(isMenuOpen);
            UpdateCursorState();
            UpdateGamePause();
        }
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        UIMenu.SetActive(false);
        UpdateCursorState();
        UpdateGamePause();
    }

    void UpdateCursorState()
    {
        if (isMenuOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void UpdateGamePause()
    {
        if (isMenuOpen)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
