using UnityEngine;
using UnityEngine.InputSystem;

// flip through canvas "pages" — only one active. left click advances.
public class end_scene : MonoBehaviour
{
    public GameObject[] pages;

    int cur;

    void Start()
    {
        cur = 0;
        Show();
    }

    void Update()
    {
        if (pages == null || pages.Length == 0) return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (cur >= pages.Length - 1) return;

        cur++;
        Show();
    }

    void Show()
    {
        if (pages == null || pages.Length == 0) return;

        for (int i = 0; i < pages.Length; i++)
            if (pages[i] != null)
                pages[i].SetActive(i == cur);

        Debug.Log("end scene: page " + (cur + 1) + " / " + pages.Length);
    }
}
