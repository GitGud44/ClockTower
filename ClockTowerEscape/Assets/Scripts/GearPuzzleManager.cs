using UnityEngine;
using UnityEngine.Events;

/*
This is gear puzzle manager: it controls the gear chain on the wall in the clock room
Idea is theres a gear thats always spinning and another one that is stuck. The goal is to get the
stuck one moving to trigger the elevator door to open. This is done by grabbing gears off the ground
and placing them on pegs on the wall beside the spinning gear.
Once the chain is all connected it moves the stuck gear to trigger the elevator door to open.

The allGears array is set up in the inspector like this [driveGear, slot1, slot2, ....etc. endGear]
The middle ones start as null and get filled when OnGearPlaced is called
*/
public class GearPuzzleManager : MonoBehaviour
{
    public GearSlot[] slots;
    public Transform[] allGears;
    public float spinSpeed = 90f;
    // public float spinSpeed = 45f;  // this was too slow
    public UnityEvent OnPuzzleSolved;

    int connectedCount = 1;
    bool puzzleSolved = false;

    // This is called by GearSlot when a gear gets placed
    public void OnGearPlaced(GearSlot slot)
    {
        // Figure out which of the slots this is and then stick the gear transform into allGears
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slot && slot.snappedGear != null)
            {
                allGears[i + 1] = slot.snappedGear;
                break;
            }
        }

        // I recount the connected gears from the start here
        connectedCount = 1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].isFilled) break;
            connectedCount = i + 2; // +1 for drive gear, +1 for this slot
        }

        // if all the slots are filled then the chain goes all the way to the end gear
        if (connectedCount > slots.Length)
        {
            connectedCount = allGears.Length;
            puzzleSolved = true;
            Debug.Log("Gear puzzle solved!");
            OnPuzzleSolved?.Invoke();
        }

        Debug.Log($"Chain has {connectedCount} connected gears");
    }


    void Update()
    {
        // here I spin each of the connected gears, and I alternate the direction because
        // thats how real gears work. even index = clockwise, odd = counter clockwise
        for (int i = 0; i < connectedCount && i < allGears.Length; i++)
        {
            if (allGears[i] == null) continue;
            // Vector3.forward didnt work, Vector3.right was wrong too
            float dir = (i % 2 == 0) ? 1f : -1f;
            allGears[i].Rotate(Vector3.up * spinSpeed * dir * Time.deltaTime);
        }
    }
}
