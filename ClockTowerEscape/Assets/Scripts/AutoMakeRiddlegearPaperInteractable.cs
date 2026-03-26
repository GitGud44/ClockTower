using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class AutoMakeRiddlegearPaperInteractable
{
    private const string TargetMaterialName = "riddlegear";
    private const float SurfaceSnapPadding = 0.01f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded += (_, _) => ApplyToScene();
        ApplyToScene();
    }

    private static void ApplyToScene()
    {
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (var renderer in renderers)
        {
            if (!UsesTargetMaterial(renderer))
                continue;

            EnsureGrabSetup(renderer.gameObject);
            SnapAboveSurface(renderer.gameObject);
        }
    }

    private static bool UsesTargetMaterial(Renderer renderer)
    {
        var materials = renderer.sharedMaterials;
        if (materials == null || materials.Length == 0)
            return false;

        foreach (var mat in materials)
        {
            if (mat == null)
                continue;

            // Unity may append " (Instance)" at runtime.
            if (mat.name.StartsWith(TargetMaterialName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void EnsureGrabSetup(GameObject go)
    {
        EnsureSolidCollider(go);

        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
            rb = go.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (go.GetComponent<XRGrabInteractable>() == null)
            go.AddComponent<XRGrabInteractable>();

        if (go.GetComponent<DesktopGrabbable>() == null)
            go.AddComponent<DesktopGrabbable>();

        var desktopGrab = go.GetComponent<DesktopGrabbable>();
        desktopGrab.holdEulerOffset = new Vector3(0f, 270f, 0f);
        desktopGrab.holdOffset = new Vector3(0f, 0.24f, 0.08f);
        desktopGrab.heldScaleMultiplier = 3f;
        desktopGrab.disableCollidersWhileHeld = false;
        desktopGrab.keepAboveSurfacesWhenReleased = true;
        desktopGrab.releasedSurfacePadding = 0.015f;

        // Keep this object hittable by the desktop raycast.
        if (go.layer == LayerMask.NameToLayer("Ignore Raycast"))
            go.layer = 0;
    }

    private static void EnsureSolidCollider(GameObject go)
    {
        var meshFilter = go.GetComponent<MeshFilter>();
        var existingMeshCollider = go.GetComponent<MeshCollider>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            var meshCollider = existingMeshCollider;
            if (meshCollider == null)
                meshCollider = go.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = false;

            var allColliders = go.GetComponents<Collider>();
            foreach (var col in allColliders)
            {
                if (col == meshCollider)
                    continue;

                col.isTrigger = false;
            }

            return;
        }

        var fallbackCollider = go.GetComponent<Collider>();
        if (fallbackCollider == null)
            fallbackCollider = go.AddComponent<BoxCollider>();

        fallbackCollider.isTrigger = false;
    }

    private static void SnapAboveSurface(GameObject go)
    {
        var collider = go.GetComponent<Collider>();
        if (collider == null)
            return;

        var bounds = collider.bounds;
        var castStart = bounds.center + Vector3.up * 2f;
        var castDistance = 20f;

        if (!Physics.Raycast(castStart, Vector3.down, out var hit, castDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        if (hit.collider == null || hit.collider.gameObject == go)
            return;

        var targetY = hit.point.y + bounds.extents.y + SurfaceSnapPadding;
        var pos = go.transform.position;

        if (pos.y < targetY)
            go.transform.position = new Vector3(pos.x, targetY, pos.z);
    }
}
