using UnityEngine;
using UnityEngine.Events;

/*
This is gear puzzle manager: it controls the gear chain on the wall in the clock room
Idea is theres a gear thats always spinning and another one that is stuck. The goal is to get the
stuck one moving to trigger the elevator door to open. This is done by grabbing gears off the ground
and placing them on pegs on the wall beside the spinning gear.
Once the chain is all connected it moves the stuck gear to trigger the elevator door to open.

The allGears array is set up in the inspector like this:
[fixedGear1, slot1, fixedGear3, slot2, fixedGear5, slot3, fixedGear7]

the NEW order with sizes is:
Fixed Gear size 5 -> Slot 1 need size 2 -> Fixed Gear size 4 ->Slot 2 needs size 7 ->Fixed Gear size 6 -> Slot 3 needs size 4 -> Last Gear size 7. 

The fixed gears are always there, the slot entries start as null and get filled when gears are placed.
The chain only passes through a slot if the correct size gear is in it.
*/
public class GearPuzzleManager : MonoBehaviour
{
    public GearSlot[] slots;
    public Transform[] allGears;
    public float spinSpeed = 90f;
    public Transform chainTransform; // this is the chain that goes from the end gear down into the wall toward the elevator -> i made it so it spins like an axle rod
    public UnityEvent OnPuzzleSolved;

    public bool startSolved = false;

    int connectedCount = 1;
    bool puzzleSolved = false;

    void Start()
    {
        if (startSolved)
        {
            connectedCount = allGears.Length;
            puzzleSolved = true;
        }
    }

    // This is called by GearSlot when a gear gets placed on any of the pegs
    public void OnGearPlaced(GearSlot slot)
    {
        // I figure out which of the slots this is and stick the gear into the right allGears index
        // slots[0] goes into allGears[1], slots[1] into allGears[3], slots[2] into allGears[5]
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slot && slot.snappedGear != null)
            {
                int gearIndex = (i * 2) + 1;
                allGears[gearIndex] = slot.snappedGear;
                break;
            }
        }

        // I recount how far the chain reaches from the start
        // the chain goes through fixed gears automatically but stops at a slot
        // unless the right size gear is there
        connectedCount = 1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isFilled || !slots[i].isCorrectGear) break;
            // this slot is correct so the chain passes through it AND the next fixed gear
            connectedCount = (i * 2) + 3;
        }

        // I cap it to the array length so it doesnt go out of bounds
        if (connectedCount > allGears.Length)
            connectedCount = allGears.Length;

        // if the chain reaches all the way to the end gear thats the puzzle solved
        if (connectedCount >= allGears.Length && !puzzleSolved)
        {
            puzzleSolved = true;
            Debug.Log("Gear puzzle solved!");
            OnPuzzleSolved?.Invoke();
        }

        Debug.Log($"Chain has {connectedCount} connected gears");
    }

    // This is called when a gear gets taken off a peg, I need to recount the chain
    public void OnGearRemoved(GearSlot slot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slot)
            {
                int gearIndex = (i * 2) + 1;
                allGears[gearIndex] = null;
                break;
            }
        }

        // I recount the whole chain from scratch since a gear got removed
        connectedCount = 1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isFilled || !slots[i].isCorrectGear) break;
            connectedCount = (i * 2) + 3;
        }
        if (connectedCount > allGears.Length)
            connectedCount = allGears.Length;

        puzzleSolved = false;
        Debug.Log($"Gear removed, chain now has {connectedCount} connected gears");
    }


    void Update()
    {
        // here I spin each of the connected gears, alternating direction because
        // thats how real meshing gears work. even index = clockwise, odd = counter clockwise
        for (int i = 0; i < connectedCount && i < allGears.Length; i++)
        {
            if (allGears[i] == null) continue;
            float dir = (i % 2 == 0) ? 1f : -1f;
            allGears[i].Rotate(Vector3.up * spinSpeed * dir * Time.deltaTime);
        }

        // once the puzzle is solved I rotate the chain object like a spinning axle, it goes sideways into the wall toward the elevator
        if (puzzleSolved && chainTransform != null)
            chainTransform.Rotate(Vector3.left * spinSpeed * Time.deltaTime, Space.World);

        // I also spin wrong-size gears that are on pegs, they spin cause the chain reaches them but
        // the chain doesnt continue past since its the wrong size
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isFilled) continue;
            if (slots[i].isCorrectGear) continue;

            // the slot is at allGears index (i * 2) + 1
            // it only spins if the chain reaches it, meaning connectedCount > (i * 2) + 1
            int slotGearIndex = (i * 2) + 1;
            if (slotGearIndex >= connectedCount) continue;

            if (slots[i].snappedGear == null) continue;
            float dir = (slotGearIndex % 2 == 0) ? 1f : -1f;
            slots[i].snappedGear.Rotate(Vector3.up * spinSpeed * dir * Time.deltaTime);
        }
    }
}
