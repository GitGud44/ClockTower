using UnityEngine;
using UnityEngine.Events;

public class RaycastInteractable : MonoBehaviour
{
    [Header("Highlight Settings")]
    public bool enableHighlight = true;
    public GameObject[] objectsToHighlight; // kept for backwards compatibility
    public Color emissionColor = Color.white;
    [Range(0f, 2f)]
    public float emissionIntensity = 0.2f;

    [Header("Events")]
    public UnityEvent OnGazeEnter;
    public UnityEvent OnGazeExit;
    public UnityEvent OnClick;

    private Material[] materials;
    private Color[] originalEmissions;
    private bool[] hadEmissions;
    private bool initialized = false;

    void InitializeMaterials()
    {
        if (initialized) return;
        initialized = true;

        // Always grab all renderers on this object and its children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Renderer self = GetComponent<Renderer>();
            if (self != null) renderers = new Renderer[] { self };
        }
        if (renderers.Length == 0) return;

        materials = new Material[renderers.Length];
        originalEmissions = new Color[renderers.Length];
        hadEmissions = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
            hadEmissions[i] = materials[i].IsKeywordEnabled("_EMISSION");
            originalEmissions[i] = materials[i].GetColor("_EmissionColor");
        }
    }

    public void GazeEnter()
    {
        InitializeMaterials(); // Initialize when first looked at
        if (enableHighlight && materials != null)
        {
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                }
            }
        }
        OnGazeEnter?.Invoke();
    }

    public void GazeExit()
    {
        if (enableHighlight && materials != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    if (!hadEmissions[i])
                    {
                        materials[i].DisableKeyword("_EMISSION");
                    }
                    materials[i].SetColor("_EmissionColor", originalEmissions[i]);
                }
            }
        }
        OnGazeExit?.Invoke();
    }

    public void Click()
    {
        OnClick?.Invoke();
    }
}
