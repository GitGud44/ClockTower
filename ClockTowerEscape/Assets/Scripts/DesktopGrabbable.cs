using UnityEngine;

public class DesktopGrabbable : MonoBehaviour
{
    public float followSpeed = 25f; // how fast the object chases the hold point
    public float maxSpeed = 15f;    // I cap this so it doesnt clip through walls
    public Vector3 holdOffset = Vector3.zero; // offset from hold point, you can tweak this in the inspector
    public Vector3 holdEulerOffset = Vector3.zero; // extra rotation while held (useful for readable notes/papers)
    public float heldScaleMultiplier = 1f; // optional scale boost while held
    public bool disableCollidersWhileHeld = true; // some objects should keep colliders on while held
    public bool keepAboveSurfacesWhenReleased = false; // prevents sinking through thin floors/carpets
    public float releasedSurfacePadding = 0.01f;

    private Rigidbody rb;
    private bool isGrabbed = false;
    private Transform holdPoint;
    private bool wasKinematic;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isGrabbed && holdPoint != null && rb != null)
        {
            // I figure out where the object should be and then push it there with velocity
            Vector3 targetPos = holdPoint.position + holdPoint.TransformDirection(holdOffset);

            // this is the anti-falling-through-floor mechanic, gears kept going out of scene if player looked down while holding thme  
            // Clamp the target so the held object cant go below the players feet, this stops it from getting pushed through the floor
            float floorY = holdPoint.root.position.y + 0.1f;
            if (targetPos.y < floorY)
                targetPos.y = floorY;

            Vector3 direction = targetPos - rb.position;

            Vector3 desiredVelocity = direction * followSpeed;

            // clamp it so it doesnt go flying through walls
            if (desiredVelocity.magnitude > maxSpeed)
            {
                desiredVelocity = desiredVelocity.normalized * maxSpeed;
            }

            rb.linearVelocity = desiredVelocity;

            // lock the rotation so the object doesnt spin around while you're holding it
            rb.angularVelocity = Vector3.zero;
            rb.MoveRotation(GetHoldRotation());
        }
        else if (!isGrabbed && keepAboveSurfacesWhenReleased && rb != null)
        {
            KeepObjectAboveSurface();
        }
    }

    public void Grab(Transform newHoldPoint)
    {
        holdPoint = newHoldPoint;
        isGrabbed = true;
        originalScale = transform.localScale;

        // fetch again just in case the rigidbody got destroyed and re-added (this happens with the gear puzzle)
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Had a bug where the player could FLY while holding a big gear because the gear woudl touch the player making player stand on the object its holding 
        // to fix this I disabled the colliders while holding so the player doesnt stand on the object and fly around
        if (disableCollidersWhileHeld)
            SetCollidersEnabled(false);

        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = false;
            rb.useGravity = false;
            SnapAboveSurfaceIfNeeded();

            // snap it to the hold point right away so theres no float delay
            Vector3 targetPos = holdPoint.position + holdPoint.TransformDirection(holdOffset);
            rb.position = targetPos;
            rb.rotation = GetHoldRotation();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (heldScaleMultiplier > 0f && !Mathf.Approximately(heldScaleMultiplier, 1f))
            transform.localScale = originalScale * heldScaleMultiplier;
    }

    public void Release()
    {
        isGrabbed = false;

        // I raycast down to find the floor and if the object somehow ended up below it I push it back up, otherwise it falls through and is gone forever
        if (rb != null)
        {
            Vector3 origin = rb.position + Vector3.up * 5f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit floorHit, 10f))
            {
                if (rb.position.y < floorHit.point.y)
                {
                    rb.position = new Vector3(rb.position.x, floorHit.point.y + 0.1f, rb.position.z);
                }
            }
        }

        if (disableCollidersWhileHeld)
            SetCollidersEnabled(true);
        holdPoint = null;

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        transform.localScale = originalScale;
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    void SetCollidersEnabled(bool enabled)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = enabled;
    }

    Quaternion GetHoldRotation()
    {
        if (holdPoint == null)
            return transform.rotation;

        return holdPoint.rotation * Quaternion.Euler(holdEulerOffset);
    }

    void KeepObjectAboveSurface()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Bounds bounds = col.bounds;
        Vector3 castStart = bounds.center + Vector3.up * 1.5f;
        if (!Physics.Raycast(castStart, Vector3.down, out RaycastHit hit, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        if (hit.collider == null || hit.collider.transform == transform)
            return;

        float minY = hit.point.y + bounds.extents.y + releasedSurfacePadding;
        if (rb.position.y < minY)
        {
            rb.position = new Vector3(rb.position.x, minY, rb.position.z);
            if (rb.linearVelocity.y < 0f)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    void SnapAboveSurfaceIfNeeded()
    {
        Collider col = GetComponent<Collider>();
        if (col == null || rb == null)
            return;

        Bounds bounds = col.bounds;
        Vector3 castStart = bounds.center + Vector3.up * 1.5f;
        if (!Physics.Raycast(castStart, Vector3.down, out RaycastHit hit, 20f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return;

        if (hit.collider == null || hit.collider.transform == transform)
            return;

        float padding = Mathf.Max(releasedSurfacePadding, 0.01f);
        float minY = hit.point.y + bounds.extents.y + padding;
        if (rb.position.y < minY)
            rb.position = new Vector3(rb.position.x, minY, rb.position.z);
    }
}
