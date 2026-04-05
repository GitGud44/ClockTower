using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class MenuToggle : MonoBehaviour
{
    public GameObject UIMenu;
    bool isMenuOpen = false;
    public InputActionReference vrMenuAction;

    [Header("XR Pause Menu Positioning")]
    public Transform xrCamera;
    public float menuDistance = 2f;

    void Start()
    {
        UIMenu.SetActive(false);
        if (xrCamera == null && Camera.main != null)
            xrCamera = Camera.main.transform;
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
        if (isMenuOpen && GameManager.Instance != null && GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.VR)
        {
            PositionMenuInFrontOfVRPlayer();
        }
        UIMenu.SetActive(isMenuOpen);
        UpdateCursorState();
        UpdateGamePause();
    }

    private void PositionMenuInFrontOfVRPlayer()
    {
        if (xrCamera == null || UIMenu == null) return;
        Vector3 forward = Vector3.ProjectOnPlane(xrCamera.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f)
            forward = xrCamera.transform.parent ? xrCamera.transform.parent.forward : Vector3.forward;
        Vector3 targetPos = xrCamera.position + forward * menuDistance;
        targetPos.y = UIMenu.transform.position.y;
        UIMenu.transform.position = targetPos;
        UIMenu.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
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
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.Desktop)
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
}
