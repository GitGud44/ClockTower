using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionsUI : MonoBehaviour
{
    //simple script to display the how to play instructions for vr and desktop
    //runs in keepers quarters
    public Canvas canvas;
    public GameManager.PlayMode canvasMode = GameManager.PlayMode.Desktop;
    private bool isOpen = true;
    private InputAction triggerAction;

    //checks to see if the current game mode matches with the canvas
    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        GameManager.PlayMode currentMode = GameManager.Instance != null
            ? GameManager.Instance.CurrentPlayMode
            : GameManager.PlayMode.None;

        if (currentMode != canvasMode)
        {
            canvas.enabled = false;
            isOpen = false;
            enabled = false;
            return;
        }

        if (canvasMode == GameManager.PlayMode.VR)
        {
            triggerAction = new InputAction("VRTrigger", InputActionType.Button);
            triggerAction.AddBinding("<XRController>{RightHand}/triggerPressed");
            triggerAction.Enable();
        }

        canvas.enabled = true;
        OpenInstructions();
    }

    //gets rif of this input action after closing the instructions
    void OnDestroy()
    {
        triggerAction?.Dispose();
    }

    //checks for input to close the instructions ui
    void Update()
    {
        if (!isOpen) return;

        if (GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.Desktop)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                CloseInstructions();
            }
        }
        else if (GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.VR)
        {
            if (triggerAction != null && triggerAction.WasPressedThisFrame())
            {
                CloseInstructions();
            }
        }
    }

    //pauses the game and opens the instructions ui 
    void OpenInstructions()
    {
        if (canvas != null)
        {
            canvas.enabled = true;
        }
        isOpen = true;
        Time.timeScale = 0f;
    }

    //closes the instructions and unpauses hgame
    void CloseInstructions()
    {
        if (canvas != null)
        {
            canvas.enabled = false;
        }
        isOpen = false;
        Time.timeScale = 1f;
    }
}
