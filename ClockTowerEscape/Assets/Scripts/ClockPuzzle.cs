using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ClockPuzzle : MonoBehaviour
{
    [Header("Time Display Wheels")]
    public Transform hourWheel;
    public Transform minutesTensWheel;
    public Transform minutesOnesWheel;

    [Header("Buttons")]
    public RaycastInteractable hourButton;
    public RaycastInteractable minutesTensButton;
    public RaycastInteractable minutesOnesButton;

    [Header("Clock Hands")]
    public Transform hourHand;
    public Transform minuteHand;

    [Header("Animation")]
    public float snapDuration = 0.3f;

    [Header("Solution Code")]
    [Range(1, 12)] public int solutionHour = 3;
    [Range(0, 5)]  public int solutionMinutesTens = 0;
    [Range(0, 9)]  public int solutionMinutesOnes = 0;


    [Header("Audio")]
    public AudioClip buttonPressClip;
    [Range(0f, 1f)] public float buttonPressVolume = 1f;
    public AudioClip handMoveClip;
    [Range(0f, 1f)] public float handMoveVolume = 1f;

    [Header("Scene Transition")]
    public string nextSceneName;
    public float fadeOutDuration = 1.5f;
    public AudioClip solveSound;
    [Range(0f, 1f)] public float solveSoundVolume = 1f;

    private int hourIndex        = 0;
    private int minutesTensIndex = 0;
    private int minutesOnesIndex = 0;
    private bool solved          = false;

    private int CurrentHour        => hourIndex == 0 ? 12 : hourIndex;
    private int CurrentMinutesTens => minutesTensIndex % 6;
    private int CurrentMinutesOnes => minutesOnesIndex;
    private int CurrentMinutes     => CurrentMinutesTens * 10 + CurrentMinutesOnes;

    void Awake()
    {
        // Desktop
        RegisterButton(hourButton,        OnHourPressed);
        RegisterButton(minutesTensButton, OnMinutesTensPressed);
        RegisterButton(minutesOnesButton, OnMinutesOnesPressed);
    }

    void Start()
    {
        RegisterXR(hourButton,        OnHourPressed);
        RegisterXR(minutesTensButton, OnMinutesTensPressed);
        RegisterXR(minutesOnesButton, OnMinutesOnesPressed);

        ApplyClockHands(snap: true);
    }

    void Update()
    {
        if (solved) return;
        if (CurrentHour        == solutionHour        &&
            CurrentMinutesTens == solutionMinutesTens &&
            CurrentMinutesOnes == solutionMinutesOnes)
        {
            Debug.Log("Clock Puzzle SOLVED — starting transition");
            solved = true;
            DisableButton(hourButton);
            DisableButton(minutesTensButton);
            DisableButton(minutesOnesButton);
            StartCoroutine(SolveAndTransition());
        }
    }

    private void RegisterButton(RaycastInteractable button, UnityAction onClick)
    {
        if (button == null) return;
        button.OnClick.AddListener(onClick);
    }

    private void RegisterXR(RaycastInteractable button, UnityAction onClick)
    {
        if (button == null) return;
        XRSimpleInteractable xrSimple = button.GetComponent<XRSimpleInteractable>();
        if (xrSimple != null)
            xrSimple.selectEntered.AddListener((args) => onClick());
    }

    private void OnHourPressed()
    {
        if (solved) return;
        hourIndex = (hourIndex + 1) % 12;
        if (buttonPressClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayClip(buttonPressClip, buttonPressVolume);
        if (hourWheel != null) StartCoroutine(SnapRotate(hourWheel, hourIndex, 12));
        ApplyClockHands(snap: false);
    }

    private void OnMinutesTensPressed()
    {
        if (solved) return;
        minutesTensIndex = (minutesTensIndex + 1) % 12;
        if (buttonPressClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayClip(buttonPressClip, buttonPressVolume);
        if (minutesTensWheel != null) StartCoroutine(SnapRotate(minutesTensWheel, minutesTensIndex, 12));
        ApplyClockHands(snap: false);
    }

    private void OnMinutesOnesPressed()
    {
        if (solved) return;
        minutesOnesIndex = (minutesOnesIndex + 1) % 10;
        if (buttonPressClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayClip(buttonPressClip, buttonPressVolume);
        if (minutesOnesWheel != null) StartCoroutine(SnapRotate(minutesOnesWheel, minutesOnesIndex, 10));
        ApplyClockHands(snap: false);
    }

    private void ApplyClockHands(bool snap)
    {
        float hourDeg   = (hourIndex % 12) * 30f + CurrentMinutes * 0.5f;
        float minuteDeg = CurrentMinutes * 6f;

        if (snap)
        {
            if (hourHand   != null) hourHand.localRotation   = Quaternion.Euler(0f, hourDeg,   0f);
            if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(0f, minuteDeg, 0f);
        }
        else
        {
            if (handMoveClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayClip(handMoveClip, handMoveVolume);
            if (hourHand   != null) StartCoroutine(RotateHandTo(hourHand,   hourDeg));
            if (minuteHand != null) StartCoroutine(RotateHandTo(minuteHand, minuteDeg));
        }
    }

    private void DisableButton(RaycastInteractable button)
    {
        if (button == null) return;
        button.GazeExit();
        button.enableHighlight = false;
        button.enabled = false;
    }

    private IEnumerator SolveAndTransition()
    {
        Debug.Log($"SolveAndTransition started. nextSceneName='{nextSceneName}'");

        if (SceneFadeManager.Instance == null)
            new GameObject("SceneFadeManager").AddComponent<SceneFadeManager>();

        if (solveSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayClip(solveSound, solveSoundVolume);

        if (SceneFadeManager.Instance != null)
        {
            yield return StartCoroutine(SceneFadeManager.Instance.FadeOut(fadeOutDuration));
        }
        else
        {
            Debug.LogWarning("SceneFadeManager still null, skipping fade");
        }

        if (GameManager.Instance != null)
            DontDestroyOnLoad(GameManager.Instance.gameObject);

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator SnapRotate(Transform cylinder, int index, int sides)
    {
        float step       = 360f / sides;
        Quaternion start = cylinder.localRotation;
        Quaternion end   = Quaternion.Euler(-index * step, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < snapDuration)
        {
            cylinder.localRotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f, 1f, elapsed / snapDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        cylinder.localRotation = end;
    }

    private IEnumerator RotateHandTo(Transform hand, float targetYDeg)
    {
        Quaternion start = hand.localRotation;
        Quaternion end   = Quaternion.Euler(0f, targetYDeg, 0f);

        float elapsed = 0f;
        while (elapsed < snapDuration)
        {
            hand.localRotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f, 1f, elapsed / snapDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        hand.localRotation = end;
    }
}
