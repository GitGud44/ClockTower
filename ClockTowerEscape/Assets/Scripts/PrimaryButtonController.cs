using UnityEngine;
using UnityEngine.InputSystem;

public class PrimaryButtonController : MonoBehaviour
{
    public GameModeMenu gameModeMenu;
    public InputActionReference primaryButtonAction;

    public void Awake()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.Enable();
            primaryButtonAction.action.performed += PressBtn;
        }
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    public void OnDestroy()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.Disable();
            primaryButtonAction.action.performed -= PressBtn;
        }
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void PressBtn(InputAction.CallbackContext ctx)
    {
        GameModeMenu menu = gameModeMenu != null ? gameModeMenu : FindObjectOfType<GameModeMenu>();
        if (menu == null)
        {
            return;
        }

        menu.StartVRMode();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (primaryButtonAction == null)
            return;

        switch (change)
        {
            case InputDeviceChange.Disconnected:
                primaryButtonAction.action.Disable();
                primaryButtonAction.action.performed -= PressBtn;
                break;
            case InputDeviceChange.Reconnected:
                primaryButtonAction.action.Enable();
                primaryButtonAction.action.performed += PressBtn;
                break;
        }
    }
}
