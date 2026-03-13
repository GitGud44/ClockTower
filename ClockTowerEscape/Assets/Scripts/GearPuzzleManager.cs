using UnityEngine;
using UnityEngine.Events;

public class GearPuzzleManager : MonoBehaviour
{


    [Header("Gear Slots Place in order: slot for gear 2, slot for gear 3)")]
    public GearSlot[] emptySlots;

    [Header("The gear transforms in the chain order: (gear1, gear2 slot, gear3 slot, gear4)")]
    public Transform[] allGears;

    public float spinSpeed = 90f;

    [Header("Events")]
    [Tooltip("This is fired when all the gears are connected. Wire to ElevatorController.UnlockAndOpenDoors. to open the elevatr door")]
    public UnityEvent OnPuzzleSolved;

    // How many gears in the chain are currently connected (starts at 1: gear 1 because always spins)
    int connectedCount = 1;
    bool puzzleSolved = false;

    public bool IsChainConnected()
    {
        // full chain means all 4 gears are connected
        return connectedCount >= allGears.Length;
    }

    public void OnGearPlaced(GearSlot slot)
    {
        // Update the snapped gear reference
        for (int i = 0; i < emptySlots.Length; i++)
        {
            if (emptySlots[i] == slot && slot.snappedGear != null)
            {
                allGears[i + 1] = slot.snappedGear;
                break;
            }
        }

        // Count consecutive filled slots from gear 1
        connectedCount = 1; // Gear 1 always connected
        for (int i = 0; i < emptySlots.Length; i++)
        {
            if (!emptySlots[i].isFilled) break;
            connectedCount = i + 2; // +1 for gear1, +1 for this slot
        }

        // If all slots filled, the chain reaches the end gear too
        if (connectedCount > emptySlots.Length)
        {
            connectedCount = allGears.Length;
            puzzleSolved = true;
            Debug.Log("The gear puzzle is solved! Hooray!");


            OnPuzzleSolved?.Invoke();
        }

        Debug.Log($"Gear chain now has {connectedCount} connected gears");
    }


// In update here spin the connected gears
// they should alternate direction because thats how gears work in real life, to do this alternate directon based on odd or even (if its even positive clockwise
//else negative counterclockwise
    void Update()
    {
        
        for (int i = 0; i < connectedCount && i < allGears.Length; i++)
        {
            if (allGears[i] == null) continue;
            float dir = (i % 2 == 0) ? 1f : -1f;
            allGears[i].Rotate(Vector3.up * spinSpeed * dir * Time.deltaTime);
        }
    }
}
