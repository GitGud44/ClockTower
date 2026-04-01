using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/*
This is the VR gear interaction script, this is on each gear alongside the XRGrabInteractable.
The problem was that in desktop mode the gear slot placement and removal is all handled through
DesktopPlayer with mouse clicks or E, but in VR theres no DesktopPlayer so I needed a way to hook
into the XR grab/release events and call the gear slot stuff from there.

When the VR player grabs a gear thats sitting on a peg it calls RemoveGearVR on the slot to
pull it off first, and when they let go of a gear near a slot it calls PlaceGearVR to snap it
on. I also had to deal with XRGrabInteractable restoring the old rigidbody state when it lets
go, because if you grabbed a gear off a peg it was kinematic with no gravity and XR would
restore that state making the gear just float in the air after you dropped it.
Grabbable objects going out of bounds and recovering (teleporting objects back to the player) is handled by VRInteractionBridge
now for all objects not just gears. I used to have floor protection in here: I used to do if the gear fell through the floor,
if its Y was lower than players, it would teleport back up. but it was teleporting
gears to the wrong spot (just above the floor instead of back to the player) and one of the gears
kept flying up into the sky at scene start (it didnt account for falling through the wall horizontally)
So now I just removed this, and its handled by just teleporting back to the player by VRInteractionBridge.
*/
public class VRGearInteraction : MonoBehaviour
{
    [Tooltip("How close the gear needs to be to a slot to snap onto it when released")]
    public float snapRadius = 0.5f;

    private XRGrabInteractable grabInteractable;
    private GearInfo gearInfo;
    private bool isHeld = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        gearInfo = GetComponent<GearInfo>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    // when the VR player grabs this gear I check if its currently on a slot and pull it off
    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;

        if (gearInfo != null && gearInfo.pickupClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(gearInfo.pickupClip, transform.position, gearInfo.pickupVolume, 1f);

        if (gearInfo != null && gearInfo.currentSlot != null)
        {
            gearInfo.currentSlot.RemoveGearVR();
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (gearInfo == null) return;

        // I check if theres a slot nearby and snap the gear onto it
        GearSlot closest = FindNearestEmptySlot();
        if (closest != null)
        {
            closest.PlaceGearVR(transform, gearInfo);
        }
        else
        {
            // this is the fix for gears floating after being taken off a peg, XRGrabInteractable saves the rigidbody
            // state when the grab starts and restores it when you let go, but if I grabbed a placed gear it was
            // kinematic with no gravity so XR restores that and the gear just floats there
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        isHeld = false;
    }

    // all out of bounds recovery is handled by VRInteractionBridge now, it teleports objects back to the player

    GearSlot FindNearestEmptySlot()
    {
        GearSlot[] slots = FindObjectsByType<GearSlot>(FindObjectsSortMode.None);
        GearSlot best = null;
        float bestDist = snapRadius;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].isFilled) continue;
            float dist = Vector3.Distance(transform.position, slots[i].transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = slots[i];
            }
        }

        return best;
    }
}
