using UnityEngine;

public class DesktopGrabbable : MonoBehaviour
{
    public float followSpeed = 25f; // how fast the object chases the hold point
    public float maxSpeed = 15f;    // I cap this so it doesnt clip through walls
    public Vector3 holdOffset = Vector3.zero; // offset from hold point, you can tweak this in the inspector

    private Rigidbody rb;
    private bool isGrabbed = false;
    private Transform holdPoint;
    private bool wasKinematic;

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
            rb.MoveRotation(holdPoint.rotation);
        }
    }

    public void Grab(Transform newHoldPoint)
    {
        holdPoint = newHoldPoint;
        isGrabbed = true;

        // fetch again just in case the rigidbody got destroyed and re-added (this happens with the gear puzzle)
        if (rb == null) rb = GetComponent<Rigidbody>();

        // Had a bug where the player could FLY while holding a big gear because the gear woudl touch the player making player stand on the object its holding 
        // to fix this I disabled the colliders while holding so the player doesnt stand on the object and fly around
        SetCollidersEnabled(false);

        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = false;
            rb.useGravity = false;

            // snap it to the hold point right away so theres no float delay
            Vector3 targetPos = holdPoint.position + holdPoint.TransformDirection(holdOffset);
            rb.position = targetPos;
            rb.rotation = holdPoint.rotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

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

        SetCollidersEnabled(true);
        holdPoint = null;

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

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
}
