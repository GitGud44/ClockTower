using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//created by ElevatorController
public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance { get; private set; }

    private CanvasGroup fadeGroup;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildFadeCanvas();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void BuildFadeCanvas()
    {
        var go = new GameObject("_ElevatorFadeCanvas");
        go.transform.SetParent(transform);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var img = go.AddComponent<Image>();
        img.color = Color.black;
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = Vector2.zero;
        img.rectTransform.offsetMax = Vector2.zero;

        fadeGroup = go.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.interactable = false;
        fadeGroup.blocksRaycasts = false;
    }

    // Fires after every scene load — fades from black back in.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f, 1.5f));
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return FadeTo(1f, duration);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float from    = fadeGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadeGroup.alpha = Mathf.Lerp(from, target, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeGroup.alpha = target;
    }
}
