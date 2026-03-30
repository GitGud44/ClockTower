using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionsUI : MonoBehaviour
{
    public Canvas canvas;
    public GameManager.PlayMode canvasMode = GameManager.PlayMode.Desktop;
    private bool isOpen = true;
    private InputAction triggerAction;

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

    void OnDestroy()
    {
        triggerAction?.Dispose();
    }

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

    void OpenInstructions()
    {
        if (canvas != null)
        {
            canvas.enabled = true;
        }
        isOpen = true;
        Time.timeScale = 0f;
    }

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
