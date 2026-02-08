using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bell : MonoBehaviour
{
    [Header("Bell Code Sequence")]
    [Tooltip("Each number is how many times the bell rings before a pause.")]
    public int[] codeSequence;
    [Tooltip("How far the bell swings in degrees.")]
    public float swingAngle = 45f;
    [Tooltip("How long each swing takes (seconds).")]
    public float swingDuration = 0.4f;
    [Tooltip("Pause between code numbers (seconds).")]
    public float pauseBetweenGroups = 1.2f;
    [Tooltip("Pause between each swing (seconds).")]
    public float pauseBetweenSwings = 0.1f;
    [Tooltip("Audio clip to play on each ring.")]
    public AudioClip bellSound;
    [Tooltip("AudioSource from where to play the bell sound.")]
    public AudioSource audioSource;
    private Quaternion initialRotation;
    private Coroutine ringRoutine;

    int hand_colliders_inside = 0;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Awake()
    {
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.OnClick.AddListener(OnBellClicked);
        }
    }

    private void OnBellClicked()
    {
        StartRinging();
    }

    public void StartRinging()
    {
        if (ringRoutine == null && codeSequence != null && codeSequence.Length > 0)
        {
            RaycastInteractable interactable = GetComponent<RaycastInteractable>();
            if (interactable != null)
            {
                interactable.GazeExit(); 
                interactable.enableHighlight = false;
                interactable.enabled = false;
            }
            ringRoutine = StartCoroutine(RingCodeSequence());
        }
    }

    private IEnumerator RingCodeSequence()
    {
        for (int i = 0; i < codeSequence.Length; i++)
        {
            int swings = Mathf.Max(1, codeSequence[i]);
            float direction = 1f;
            for (int s = 0; s < swings; s++)
            {
                yield return StartCoroutine(SwingToAngle(direction * swingAngle));
                if (audioSource && bellSound)
                    audioSource.PlayOneShot(bellSound);
                //other direction
                direction *= -1f;
                yield return new WaitForSeconds(pauseBetweenSwings);
            }
            //return to center after each swing sequence
            yield return StartCoroutine(SwingToAngle(0f));
            yield return new WaitForSeconds(pauseBetweenGroups);
        }
        ringRoutine = null;
        RaycastInteractable interactable = GetComponent<RaycastInteractable>();
        if (interactable != null)
        {
            interactable.enabled = true;
            interactable.enableHighlight = true;
        }
    }

    void OnDisable()
    {
        if (ringRoutine != null)
        {
            StopCoroutine(ringRoutine);
            ringRoutine = null;
        }
    }

    private IEnumerator SwingToAngle(float targetAngle)
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = initialRotation * Quaternion.Euler(targetAngle, 0f, 0f);
        while (elapsed < swingDuration)
        {
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / swingDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = endRotation;
    }


  // ========== VR Trigger Interaction ==========

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hand"))
        {
            hand_colliders_inside++;

            if(hand_colliders_inside == 1 && ringRoutine == null)
             {
                StartRinging();
             }
        }
    }
}