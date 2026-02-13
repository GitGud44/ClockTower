using UnityEngine;
using UnityEngine.Events;

public class RaycastInteractable : MonoBehaviour
{
    [Header("Highlight Settings")]
    public bool enableHighlight = true;
    [Tooltip("If empty and highlightAllChildren is true, will auto-highlight all child renderers")]
    public GameObject[] objectsToHighlight;
    [Tooltip("If objectsToHighlight is empty, highlight all children with renderers")]
    public bool highlightAllChildren = false;
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

        if (objectsToHighlight != null && objectsToHighlight.Length > 0)
        {
            materials = new Material[objectsToHighlight.Length];
            originalEmissions = new Color[objectsToHighlight.Length];
            hadEmissions = new bool[objectsToHighlight.Length];

            for (int i = 0; i < objectsToHighlight.Length; i++)
            {
                if (objectsToHighlight[i] != null)
                {
                    Renderer rend = objectsToHighlight[i].GetComponent<Renderer>();
                    if (rend != null)
                    {
                        materials[i] = rend.material;
                        hadEmissions[i] = materials[i].IsKeywordEnabled("_EMISSION");
                        originalEmissions[i] = materials[i].GetColor("_EmissionColor");
                    }
                }
            }
        }
        else if (highlightAllChildren)
        {
            // Highlight all child renderers
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            if (childRenderers.Length > 0)
            {
                materials = new Material[childRenderers.Length];
                originalEmissions = new Color[childRenderers.Length];
                hadEmissions = new bool[childRenderers.Length];

                for (int i = 0; i < childRenderers.Length; i++)
                {
                    materials[i] = childRenderers[i].material;
                    hadEmissions[i] = materials[i].IsKeywordEnabled("_EMISSION");
                    originalEmissions[i] = materials[i].GetColor("_EmissionColor");
                }
            }
        }
        else
        {
            // Fallback: highlight just this object
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                materials = new Material[1];
                originalEmissions = new Color[1];
                hadEmissions = new bool[1];
                materials[0] = rend.material;
                hadEmissions[0] = materials[0].IsKeywordEnabled("_EMISSION");
                originalEmissions[0] = materials[0].GetColor("_EmissionColor");
            }
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
