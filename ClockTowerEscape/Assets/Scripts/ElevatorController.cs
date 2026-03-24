using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorController : MonoBehaviour
{
    [Header("Door Transforms")]
    public Transform outerDoorLeft;
    public Transform outerDoorRight;
    public Transform innerDoorLeft;
    public Transform innerDoorRight;

    [Header("Door Open Offsets")]
    [Tooltip("Amount each door slides from their starting positions when the elevator opens")]
    public Vector3 outerLeftOpenOffset  = new Vector3(1f, 0f, 0f);
    public Vector3 outerRightOpenOffset = new Vector3(-1f, 0f, 0f);
    public Vector3 innerLeftOpenOffset  = new Vector3(0.86f, 0f, 0f);
    public Vector3 innerRightOpenOffset = new Vector3(-0.86f, 0f, 0f);

    [Header("Door Animation")]
    public float doorSlideDuration     = 1.5f;
    public float delayBetweenDoorPairs = 0.5f;

    [Header("Scene Transition")]
    [Tooltip("Scene name as it appears in Build Settings")]
    public string nextSceneName;
    [Tooltip("Optional local clip for the elevator ding.")]
    public AudioClip dingSoundClip;
    [Range(0f, 1f)]
    public float dingVolume = 1f;
    public float fadeOutDuration = 1.5f;

    [Header("Player Detection")]
    [Tooltip("Tag on the player collider")]
    public string playerTag = "Player";

    [Header("State")]
    [Tooltip("If true, the doors start locked. Call UnlockAndOpenDoors() to open them")]
    public bool startsLocked = true;

   //store initial closed positions
    private Vector3 outerLeftClosed, outerRightClosed;
    private Vector3 innerLeftClosed, innerRightClosed;

    private bool isUnlocked     = false;
    private bool doorsOpen      = false;
    private bool isTransitioning = false;

    void Start()
    {
        if (outerDoorLeft)  outerLeftClosed  = outerDoorLeft.localPosition;
        if (outerDoorRight) outerRightClosed = outerDoorRight.localPosition;
        if (innerDoorLeft)  innerLeftClosed  = innerDoorLeft.localPosition;
        if (innerDoorRight) innerRightClosed = innerDoorRight.localPosition;

        //Make sure a SceneFadeManager exists in the scene
        if (SceneFadeManager.Instance == null)
            new GameObject("SceneFadeManager").AddComponent<SceneFadeManager>();

        isUnlocked = !startsLocked;
    }

    //public API
    //connect to specific unlock event (eg. Bell.OnRingComplete unlocks elevator when the bell code is fully rung)

    public void UnlockAndOpenDoors()
    {
        if (isUnlocked) return;
        isUnlocked = true;
        StartCoroutine(OpenDoorsSequence());
    }

    //Door animation

    private IEnumerator OpenDoorsSequence()
    {
        //outer doors slide open
        yield return StartCoroutine(SlideDoorPair(
            outerDoorLeft,  outerLeftClosed,  outerLeftClosed  + outerLeftOpenOffset,
            outerDoorRight, outerRightClosed, outerRightClosed + outerRightOpenOffset));

        yield return new WaitForSeconds(delayBetweenDoorPairs);

        //inner doors slide open
        yield return StartCoroutine(SlideDoorPair(
            innerDoorLeft,  innerLeftClosed,  innerLeftClosed  + innerLeftOpenOffset,
            innerDoorRight, innerRightClosed, innerRightClosed + innerRightOpenOffset));

        doorsOpen = true;
    }

    private IEnumerator SlideDoorPair(
        Transform a, Vector3 fromA, Vector3 toA,
        Transform b, Vector3 fromB, Vector3 toB)
    {
        float elapsed = 0f;
        while (elapsed < doorSlideDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / doorSlideDuration);
            if (a) a.localPosition = Vector3.Lerp(fromA, toA, t);
            if (b) b.localPosition = Vector3.Lerp(fromB, toB, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (a) a.localPosition = toA;
        if (b) b.localPosition = toB;
    }

    //Player detection
    private void OnTriggerEnter(Collider other)
    {
        if (!doorsOpen || isTransitioning) return;

        bool isPlayer = other.CompareTag(playerTag)
                     || other.GetComponent<DesktopPlayer>() != null;

        if (isPlayer)
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextScene());
        }
    }

    //Scene transition

    private IEnumerator TransitionToNextScene()
    {
        SceneFadeManager fm = SceneFadeManager.Instance;

        //fade to black
        if (fm != null)
            yield return StartCoroutine(fm.FadeOut(fadeOutDuration));

        //play ding and dont destroy GameManager in the next scene
        if (GameManager.Instance != null)
        {
            if (dingSoundClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayClip(dingSoundClip, dingVolume);

            DontDestroyOnLoad(GameManager.Instance.gameObject);
        }

        //brief pause so the ding has a moment to play
        yield return new WaitForSecondsRealtime(0.5f);

        //Load next scene
        //SceneFadeManager (DontDestroyOnLoad) automatically fades back in
        SceneManager.LoadScene(nextSceneName);
    }
}
