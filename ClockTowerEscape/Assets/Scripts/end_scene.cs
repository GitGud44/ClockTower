using UnityEngine;
using UnityEngine.InputSystem;

//flip through pages
// desktop left click and vr trigger
// drag each page into the list 
public class end_scene : MonoBehaviour
{

    public GameObject screenCanvas;
    public GameObject vrCanvas;


    public GameObject[] desktopPages;


    public GameObject[] vrPages;

    GameObject[] pages;
    int cur;
    bool vr;
    InputAction triggerAction;

    void Start()
    {
        SetupMode();

        if (vr==true)
        {
            pages = vrPages;
        }
        else
        {
            pages = desktopPages;
        }

        if (vr==true)
        {
            triggerAction = new InputAction(binding: "<XRController>/triggerPressed");
            triggerAction.Enable();
        }

        cur = 0;
        Show();
    }

    void OnDestroy()
    {
        if (triggerAction != null)
        {
            triggerAction.Disable();
            triggerAction.Dispose();
        }
    }

    void SetupMode()
    {
        vr = GameManager.Instance != null && GameManager.Instance.CurrentPlayMode == GameManager.PlayMode.VR;

        if (vr==true)
        {
            if (screenCanvas != null) 
            {
                screenCanvas.SetActive(false);
            }
            if (vrCanvas != null) 
            {
                vrCanvas.SetActive(true);
            }
        }
        else
        {
            if (screenCanvas != null) 
            {
                screenCanvas.SetActive(true);
            }
            if (vrCanvas != null)
            {
                vrCanvas.SetActive(false);
            }

            var dp = FindFirstObjectByType<DesktopPlayer>(FindObjectsInactive.Include);
            if (dp != null)
            {
                dp.gameObject.SetActive(false);
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Debug.Log("end scene: mode = " + (vr ? "VR" : "Desktop"));
    }

    bool Pressed()
    {
        if (vr==true)
        {
            return triggerAction != null && triggerAction.WasPressedThisFrame();    
        }
        else
        {
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        }
    }

    void Update()
    {
        if (pages == null || pages.Length == 0)
        {
            return;
        } 
        if (!Pressed()) 
        {
            return;
        }

        if (cur >= pages.Length - 1)
        {
            return;
        } 

        cur++;
        Show();
    }

    void Show()
    {
        if (pages == null || pages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == cur);
            }
        }

        Debug.Log("end scene: page " + (cur + 1) + " / " + pages.Length);
    }
}
