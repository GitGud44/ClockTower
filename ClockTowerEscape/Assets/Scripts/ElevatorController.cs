using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorController : MonoBehaviour
{
    public Transform outerDoorLeft;
    public Transform outerDoorRight;
    public Transform innerDoorLeft;
    public Transform innerDoorRight;

    public Vector3 outerLeftOpenOffset  = new Vector3(1f, 0f, 0f);
    public Vector3 outerRightOpenOffset = new Vector3(-1f, 0f, 0f);
    public Vector3 innerLeftOpenOffset  = new Vector3(0.86f, 0f, 0f);
    public Vector3 innerRightOpenOffset = new Vector3(-0.86f, 0f, 0f);

    public float doorSlideDuration = 1.5f;
    public float delayBetweenDoorPairs = 0.5f;

    public string nextSceneName;
    public AudioClip dingSoundClip;

    [Header("Door SFX")]
    public AudioClip openDoorsClip;
    [Range(0f, 1f)]
    public float openDoorsVolume = 1f;

    public float dingVolume = 1f;
    public float fadeOutDuration = 1.5f;

    public string playerTag = "Player";

    public bool startsLocked = true;

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

        if (SceneFadeManager.Instance == null)
            new GameObject("SceneFadeManager").AddComponent<SceneFadeManager>();

        isUnlocked = !startsLocked;

        if (isUnlocked)
            StartCoroutine(OpenDoorsSequence());
    }

    public void UnlockAndOpenDoors()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        if (openDoorsClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(openDoorsClip, transform.position, openDoorsVolume, 1f);

        StartCoroutine(OpenDoorsSequence());
    }

    //The Door animation
    private IEnumerator OpenDoorsSequence()
    {
        //outer doors open
        yield return StartCoroutine(SlideDoorPair(
            outerDoorLeft,  outerLeftClosed,  outerLeftClosed  + outerLeftOpenOffset,
            outerDoorRight, outerRightClosed, outerRightClosed + outerRightOpenOffset));

        yield return new WaitForSeconds(delayBetweenDoorPairs);

        //inner doors open
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


    private void OnTriggerEnter(Collider other)
    {
        if (!doorsOpen || isTransitioning) return;

        // Here added this for the VR player, it was working for desktop player but the VR player needs to get the player from the Tag, otherwise would walk in elevator and not do anything for VR
        bool isPlayer = other.CompareTag(playerTag) || other.GetComponent<DesktopPlayer>() != null || other.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>() != null;

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

        //play ding
        if (GameManager.Instance != null)
        {
            if (dingSoundClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlayClip(dingSoundClip, dingVolume);

            DontDestroyOnLoad(GameManager.Instance.gameObject);
        }

        yield return new WaitForSecondsRealtime(0.5f);
        // next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
