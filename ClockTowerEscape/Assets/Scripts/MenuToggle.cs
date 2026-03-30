using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MenuToggle : MonoBehaviour
{
    public GameObject UIMenu;
    bool isMenuOpen = false;
    public InputActionReference vrMenuAction;
    void Start()
    {
        UIMenu.SetActive(false);
    }

    void OnEnable()
    {
        if (vrMenuAction != null)
        {
            vrMenuAction.action.performed += OnVrMenuPerformed;
            vrMenuAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (vrMenuAction != null)
        {
            vrMenuAction.action.performed -= OnVrMenuPerformed;
            vrMenuAction.action.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenuState();
        }
    }

    private void OnVrMenuPerformed(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.VR)
        {
            ToggleMenuState();
        }
    }

    private void ToggleMenuState()
    {
        isMenuOpen = !isMenuOpen;
        UIMenu.SetActive(isMenuOpen);
        UpdateCursorState();
        UpdateGamePause();
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
