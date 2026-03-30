using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/*
This is the VR highlight bridge. The problem was that in desktop mode DesktopPlayer does a
Physics.Raycast every frame and calls GazeEnter/GazeExit on whatever it hits, but in VR mode
theres no DesktopPlayer so nothing was calling those functions and objects werent highlighting.
I basically just do the same raycast but from the VR ray interactor's transform instead of the
camera, that way the existing RaycastInteractable highlight system works in VR without having
to change any of the interactable scripts.

I also handle the placed gear highlighting here the same way DesktopPlayer does it with the
emission toggle, since placed gears dont have a RaycastInteractable on them they need the
manual emission stuff.


--- Out of bounds object recovery ---

I had a HUGE headache and trouble trying to fix this. Grabbale objects going out of bounds,
they shouldnt be able to go out of bounds at all but if they do, they should somehow come back to the scene. 

In VR the player can shove their hand through a wall and drag objects outside the room. 
I tried like 5 different approaches to fix this:

1. Raycast from the hand to the held object to detect a wall between them - didnt work
   because XRGrabInteractable puts the object right at the hand so theres no gap to detect

2. Track the object position frame to frame and raycast from last position to current position
   to catch the wall crossing - this worked once then broke, I think the raycast was hitting
   the objects own collider sometimes and the frame to frame tracking kept getting desynced

3. Do the check in FixedUpdate - didnt work because XRGrabInteractable moves the object to
   the hand AFTER FixedUpdate so it just overrides my pushback immediately

4. Move the check to LateUpdate and use transform.position instead of rb.position - still
   didnt work well would sometimes bug, the object would still clip through on fast movements

5. CancelInteractorSelection to force drop the object when it clips - worked once then the
   interactor state got messed up and it stopped firing events on the second grab, also got
   NullReferenceExceptions because CancelInteractorSelection triggers OnSelectExited which
   nulls everything out in the middle of the function

6. Listen to selectEntered/selectExited events on the ray interactor to track held objects
   and check their Y position - the events werent even firing from the XR simulator
   interactor so it never knew an object was being held

What actually works: the SIMPLE approach! If a grabbable object is more than 10 units below the player's Y I teleport them back to the player!!. 
The player is always on the scene so this is genius!

I just loop through every XRGrabInteractable in the scene every frame and if any
of them are less than y=-10, I teleport them back to the player.
*/
public class VRInteractionBridge : MonoBehaviour
{
    public float raycastRange = 10f;

    private XRRayInteractor rayInteractor;
    private RaycastInteractable currentTarget;

    // placed gear highlight state, I copied this from DesktopPlayer basically
    private GearInfo highlightedGear;
    private Material[] highlightedGearMats;
    private Color[] highlightedGearOrigEmissions;
    private bool[] highlightedGearHadEmission;

    void Start()
    {
        rayInteractor = GetComponentInChildren<XRRayInteractor>();
        if (rayInteractor == null)
        {
            // Debug help
            Debug.LogWarning("[VRInteractionBridge] No XRRayInteractor found in children!");
            return;
        }

    }

    void OnDisable()
    {
        // clean up highlights when disabled or scene changes so they dont get stuck on
        if (currentTarget != null)
        {
            currentTarget.GazeExit();
            currentTarget = null;
        }
        UnhighlightGear();
    }

    void Update()
    {
        if (rayInteractor == null) return;

        // I shoot a raycast from the ray interactor the same way DesktopPlayer does from the camera
        Ray ray = new Ray(rayInteractor.transform.position, rayInteractor.transform.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, raycastRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        GearInfo hitGear = didHit ? hit.collider.GetComponentInParent<GearInfo>() : null;
        bool hitPlacedGear = hitGear != null && hitGear.currentSlot != null;

        // I handle placed gear highlighting separately here since they dont have RaycastInteractable, same as DesktopPlayer
        if (hitPlacedGear && hitGear != highlightedGear)
        {
            UnhighlightGear();
            HighlightGear(hitGear);
        }
        else if (!hitPlacedGear && highlightedGear != null)
        {
            UnhighlightGear();
        }

        if (didHit)
        {
            // if I'm highlighting a placed gear I dont want to also trigger the RaycastInteractable highlight on top of it
            RaycastInteractable target = hitPlacedGear ? null : hit.collider.GetComponentInParent<RaycastInteractable>();

            if (target != null && target != currentTarget)
            {
                if (currentTarget != null) currentTarget.GazeExit();
                target.GazeEnter();
                currentTarget = target;
            }
            else if (target == null && currentTarget != null)
            {
                currentTarget.GazeExit();
                currentTarget = null;
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.GazeExit();
                currentTarget = null;
            }
        }
    }

    // I tried raycasts and grab events to stop objects going through walls but none of them worked reliably,
    // the grab events werent even firing from the XR simulator interactor. so I just check every single
    // XRGrabInteractable in the scene every frame and if any of them are way below the player Y i teleport
    // them back. brute force but it actually works
    void LateUpdate()
    {
        float playerY = transform.position.y;
        var allGrabbables = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);

        for (int i = 0; i < allGrabbables.Length; i++)
        {
            XRGrabInteractable grabbable = allGrabbables[i];

            // if the object fell way below the player teleport it back
            if (grabbable.transform.position.y < playerY - 10f)
            {
                Transform obj = grabbable.transform;
                obj.position = transform.position + Vector3.up * 0.5f;

                // I set it to kinematic first so it fully stops, then start a coroutine to turn physics
                // back on next frame. if I just zero the velocity it still has momentum from falling
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    StartCoroutine(RestorePhysicsNextFrame(rb));
                }

                Debug.Log($"[VRInteractionBridge] '{obj.name}' fell out of bounds (Y < playerY - 10), teleported back to player");
            }

            // if the object isnt being held and isnt on a gear slot but has no gravity, something went wrong
            // (XR grab events dont always fire properly with the simulator so the rigidbody gets left in a bad state)
            // I just force gravity back on so it doesnt float forever
            // if the object isnt being held and isnt on a gear slot but is kinematic or has no gravity,
            // something went wrong so I fix it. I check both isKinematic and useGravity because sometimes
            // the rigidbody gets left kinematic from placement and the gravity check alone wasnt catching it
            if (!grabbable.isSelected)
            {
                GearInfo gearInfo = grabbable.GetComponent<GearInfo>();
                bool isOnSlot = gearInfo != null && gearInfo.currentSlot != null;

                if (!isOnSlot)
                {
                    Rigidbody rb = grabbable.GetComponent<Rigidbody>();
                    if (rb != null && (rb.isKinematic || !rb.useGravity))
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        Debug.Log($"[VRInteractionBridge] Restored physics on '{grabbable.name}' — was stuck kinematic or no gravity");
                    }
                }
            }
        }
    }

    // I wait one frame before turning physics back on so the object doesnt carry its falling velocity into the new position
    IEnumerator RestorePhysicsNextFrame(Rigidbody rb)
    {
        yield return null;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // I copied the highlight/unhighlight gear logic from DesktopPlayer since placed gears need the manual emission toggle
    void HighlightGear(GearInfo gear)
    {
        highlightedGear = gear;
        Renderer[] renderers = gear.GetComponentsInChildren<Renderer>();
        highlightedGearMats = new Material[renderers.Length];
        highlightedGearOrigEmissions = new Color[renderers.Length];
        highlightedGearHadEmission = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            highlightedGearMats[i] = renderers[i].material;
            highlightedGearHadEmission[i] = highlightedGearMats[i].IsKeywordEnabled("_EMISSION");
            highlightedGearOrigEmissions[i] = highlightedGearMats[i].GetColor("_EmissionColor");
            highlightedGearMats[i].EnableKeyword("_EMISSION");
            highlightedGearMats[i].SetColor("_EmissionColor", Color.white * 0.2f);
        }
    }

    void UnhighlightGear()
    {
        if (highlightedGear == null) return;
        if (highlightedGearMats != null)
        {
            for (int i = 0; i < highlightedGearMats.Length; i++)
            {
                if (highlightedGearMats[i] == null) continue;
                if (!highlightedGearHadEmission[i])
                    highlightedGearMats[i].DisableKeyword("_EMISSION");
                highlightedGearMats[i].SetColor("_EmissionColor", highlightedGearOrigEmissions[i]);
            }
        }
        highlightedGear = null;
        highlightedGearMats = null;
    }
}
