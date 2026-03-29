using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookInteractable : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Local clip for spatial page-flip playback")]
    public AudioClip pageFlipClip;
    [Range(0f, 1f)]
    public float pageFlipVolume = 1f;

    [Header("Pages")]
    [Tooltip("All page Transforms, ordered bottom to top of the initial RIGHT side of the stack.")]
    public List<Transform> pageObjects = new List<Transform>();

    [Header("Animation")]
    [Tooltip("Y position the page lifts to before flipping.")]
    public float liftHeight = 0.1f;

    [Tooltip("Y position of the bottom page in any stack.")]
    public float stackBaseY = 0f;

    [Tooltip("Additional Y per page in a stack (so pages dont overlap completely)")]
    public float pageStackStep = 0.005f;

    [Tooltip("Speed page lifts per second.")]
    public float liftSpeed = 1f;

    [Tooltip("Degrees per second for the 180 flip.")]
    public float flipSpeed = 180f;

    [Tooltip("Speed page drops per second.")]
    public float dropSpeed = 1f;

    [Header("Sides of the book")]
    public BoxCollider rightCollider;
    public BoxCollider leftCollider;

    //index 0 is bottom of stack
    private List<Transform> leftStack  = new List<Transform>();
    private List<Transform> rightStack = new List<Transform>();

    private bool isFlipping = false;

    void Awake()
    {
        SetupBookSide(rightCollider, isRight: true);
        SetupBookSide(leftCollider,  isRight: false);

        foreach (Transform page in pageObjects)
            rightStack.Add(page);

        SnapAllToStack();
    }

    private void SetupBookSide(BoxCollider side, bool isRight)
    {
        if (side == null) return;
        RaycastInteractable ri = side.gameObject.GetComponent<RaycastInteractable>();
        if (ri == null) ri = side.gameObject.AddComponent<RaycastInteractable>();

        if (ri.OnClick == null)
            ri.OnClick = new UnityEngine.Events.UnityEvent();

        ri.OnClick.AddListener(() => OnSideClicked(isRight));
    }

    private void OnSideClicked(bool clickedRight)
    {
        if (isFlipping) return;

        if (clickedRight && rightStack.Count > 0)
        {
            Transform page = rightStack[rightStack.Count - 1];
            rightStack.RemoveAt(rightStack.Count - 1);
            leftStack.Add(page);
            PlayFlipSound();
            StartCoroutine(AnimateFlip(page, forward: true));
        }
        else if (!clickedRight && leftStack.Count > 0)
        {
            Transform page = leftStack[leftStack.Count - 1];
            leftStack.RemoveAt(leftStack.Count - 1);
            rightStack.Add(page);
            PlayFlipSound();
            StartCoroutine(AnimateFlip(page, forward: false));
        }
    }

    private IEnumerator AnimateFlip(Transform page, bool forward)
    {
        isFlipping = true;

        //Lift page straight up along local Y to liftHeight (so all page flips will pivot around the same point)
        Vector3 liftTarget = new Vector3(page.localPosition.x, liftHeight, page.localPosition.z);
        while (page.localPosition.y < liftHeight - 0.0001f)
        {
            page.localPosition = Vector3.MoveTowards(page.localPosition, liftTarget, liftSpeed * Time.deltaTime);
            yield return null;
        }
        page.localPosition = liftTarget;

        //Rotate page 180 degrees around its local Z axis
        float rotated   = 0f;
        float direction = forward ? -1f : 1f;
        while (rotated < 180f)
        {
            float step = Mathf.Min(flipSpeed * Time.deltaTime, 180f - rotated);
            page.Rotate(Vector3.forward * direction * step, Space.Self);
            rotated += step;
            yield return null;
        }

        //Drop page straight down to stack position on other side
        List<Transform> destStack = forward ? leftStack : rightStack;
        float destY       = stackBaseY + (destStack.Count - 1) * pageStackStep;
        Vector3 dropTarget = new Vector3(page.localPosition.x, destY, page.localPosition.z);

        while (page.localPosition.y > destY + 0.0001f)
        {
            page.localPosition = Vector3.MoveTowards(page.localPosition, dropTarget, dropSpeed * Time.deltaTime);
            yield return null;
        }
        page.localPosition = dropTarget;

        isFlipping = false;
    }

    private void SnapAllToStack()
    {
        for (int i = 0; i < rightStack.Count; i++)
        {
            Transform p = rightStack[i];
            p.localPosition = new Vector3(p.localPosition.x, stackBaseY + i * pageStackStep, p.localPosition.z);
            p.localRotation = Quaternion.identity;
        }
        for (int i = 0; i < leftStack.Count; i++)
        {
            Transform p = leftStack[i];
            p.localPosition = new Vector3(p.localPosition.x, stackBaseY + i * pageStackStep, p.localPosition.z);
            p.localRotation = Quaternion.Euler(Vector3.forward * 180f);
        }
    }

    private void PlayFlipSound()
    {
        if (pageFlipClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySpatialClip(pageFlipClip, transform.position, pageFlipVolume, 1f);
        }

        Debug.Log("Book interacted - playing PageFlip sound");

        // TODO: Show hint UI with lantern order clues
    }

    //VR: hand trigger enters the side collider
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Hand")) return;
        if (rightCollider != null && rightCollider.bounds.Intersects(other.bounds))
            OnSideClicked(true);
        else if (leftCollider != null && leftCollider.bounds.Intersects(other.bounds))
            OnSideClicked(false);
    }
}