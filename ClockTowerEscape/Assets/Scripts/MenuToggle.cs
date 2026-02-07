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
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isMenuOpen = !isMenuOpen;
            UIMenu.SetActive(isMenuOpen);
        }
    }
}
