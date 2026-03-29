using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionsUI : MonoBehaviour
{
    public Canvas canvas;
    public GameManager.PlayMode canvasMode = GameManager.PlayMode.Desktop;
    private bool isOpen = true;

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

        canvas.enabled = true;
        OpenInstructions();
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
            var rightTrigger = Gamepad.current?.rightTrigger;
            
            if (rightTrigger != null && rightTrigger.wasPressedThisFrame)
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
