using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

/*
VR UI canvas "patcher" script: problem is that in VR mode the ray interactor cant hit
any UI buttons unless the canvases have TrackedDeviceGraphicRaycaster, and the EventSystem
needs XRUIInputModule instead of the normal StandaloneInputModule. I put this on the VR_Mode prefab
so it patches every canvas automatically when a new scene loads, that way I dont have to manually
go into every scene and add the components to every canvas.

At first I made my own ray(laser pointer) script, but then I realized XR Interaction Toolkit already has a built in one! no need for me to build one from scratch
It's from the XR Interaction Toolkit starter assets, I dragged it under RightHand. This script only handles the canvas patching side now, no more ray creating stuff
*/
public class VRUIRaySetup : MonoBehaviour
{
    void OnEnable()
    {
        PatchAllCanvases();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // every time a new scene loads I need to patch the canvases again since they're new objects
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PatchAllCanvases();
    }

    // I keep patching every frame because canvases can get activated at any time (like the VR menu showing up after clicking VR mode)
    void LateUpdate()
    {
        PatchAllCanvases();
    }

    // I go through every canvas in the scene and slap a TrackedDeviceGraphicRaycaster on it so the VR ray can actually hit UI elements
    void PatchAllCanvases()
    {
        EnsureXRUIInputModule();

        var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in allCanvases)
        {
            if (!canvas.isRootCanvas)
                continue;

            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
    }

    // the event system needs XRUIInputModule instead of the default input modules or the ray wont register clicks on UI
    void EnsureXRUIInputModule()
    {
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
            return;

        var standaloneModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (standaloneModule != null)
            Destroy(standaloneModule);

        var inputSystemModule = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputSystemModule != null && inputSystemModule.GetType() == typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule))
            Destroy(inputSystemModule);

        if (eventSystem.GetComponent<XRUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<XRUIInputModule>();

            //refresh ray interactors so they recognize the new input module
            var rayInteractors = FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);
            foreach (var ray in rayInteractors)
            {
                if (ray.enabled)
                {
                    ray.enabled = false;
                    ray.enabled = true;
                }
            }
        }
    }
}
