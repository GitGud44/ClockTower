using UnityEngine;
using UnityEngine.InputSystem;

//this was made for the game mode menu to choose vr mode and close it so you could proceed into the start menu
public class PrimaryButtonController : MonoBehaviour
{
    public GameModeMenu gameModeMenu;
    public InputActionReference primaryButtonAction;

    //listens for primary button input
    public void Awake()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.Enable();
            primaryButtonAction.action.performed += PressBtn;
        }
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    //destoys listeners for the primary button input
    public void OnDestroy()
    {
        if (primaryButtonAction != null)
        {
            primaryButtonAction.action.Disable();
            primaryButtonAction.action.performed -= PressBtn;
        }
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    //calls the start vr mode function
    private void PressBtn(InputAction.CallbackContext ctx)
    {
        GameModeMenu menu = gameModeMenu != null ? gameModeMenu : FindObjectOfType<GameModeMenu>();
        if (menu == null)
        {
            return;
        }

        menu.StartVRMode();
    }

    //this is just a check that if th controller gets disconneceted and reconnected it wont cause issues
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
