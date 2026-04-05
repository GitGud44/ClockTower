using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class MenuToggle : MonoBehaviour
{
    public GameObject UIMenu;
    bool isMenuOpen = false;
    public InputActionReference vrSecondaryButtonAction;

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
        if (vrSecondaryButtonAction != null)
        {
            vrSecondaryButtonAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (vrSecondaryButtonAction != null)
        {
            vrSecondaryButtonAction.action.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //opens pause menu if esc is pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenuState();
        }

        //opens the pause menu in vr
        if (GameManager.Instance != null && GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.VR)
        {
            if (vrSecondaryButtonAction != null && vrSecondaryButtonAction.action.WasPressedThisFrame())
            {
                ToggleMenuState();
            }
        }
    }

    //toggles menu state and cursor state
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

    //closes menu
    public void CloseMenu()
    {
        isMenuOpen = false;
        UIMenu.SetActive(false);
        UpdateCursorState();
        UpdateGamePause();
    }

    //updates cursor visibility if menu is open or not
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

    //freeze game if menu is open
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
