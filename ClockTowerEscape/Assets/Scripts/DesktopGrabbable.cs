using UnityEngine;

public class DesktopGrabbable : MonoBehaviour
{
    [Header("Grab Settings")]
    public float followSpeed = 10f; // How fast the object follows the hold point
    public float maxSpeed = 5f;     // Maximum velocity to prevent clipping through walls
    public Vector3 holdOffset = Vector3.zero; // Offset from hold point for better visuals and customizable through the inspector
    
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
            // Calculate direction and distance to hold point (with offset)
            Vector3 targetPos = holdPoint.position + holdPoint.TransformDirection(holdOffset);
            Vector3 direction = targetPos - rb.position;
            
            // Calculate the desired velocity
            Vector3 desiredVelocity = direction * followSpeed;
            
            // Clamp to max speed to prevent clipping through walls
            if (desiredVelocity.magnitude > maxSpeed)
            {
                desiredVelocity = desiredVelocity.normalized * maxSpeed;
            }
            
            rb.linearVelocity = desiredVelocity;
            
            // Lock rotation to hold point (no physics rotation while held)
            rb.angularVelocity = Vector3.zero;
            rb.MoveRotation(holdPoint.rotation);
        }
    }
    
    public void Grab(Transform newHoldPoint)
    {
        holdPoint = newHoldPoint;
        isGrabbed = true;
        
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = false; // Keep physics enabled for collisions
            rb.useGravity = false;  // But disable gravity while held
        }
    }
    
    public void Release()
    {
        isGrabbed = false;
        holdPoint = null;
        
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero; // Stop movement on release
        }
    }
    
    public bool IsGrabbed()
    {
        return isGrabbed;
    }
}
