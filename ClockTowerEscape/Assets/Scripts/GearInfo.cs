using UnityEngine;

// This goes on each loose gear on the floor so I can identify what size it is
// gearSize is what determines if it fits on a peg or not
public class GearInfo : MonoBehaviour
{
    public int gearSize = 1;

    // GearSlot sets this when the gear gets placed on a peg, its null when the gear is just loose on the ground
    [HideInInspector] public GearSlot currentSlot; // i dont want to show in inspector, but i still need to be accessible from other scripts so hide in inspector exists--an in between of private and public basically
}
