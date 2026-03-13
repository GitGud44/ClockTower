using UnityEngine;
using UnityEngine.Events;

public class GearPuzzleManager : MonoBehaviour
{
    [Header("Gear Slots (in order: slot for gear 2, slot for gear 3)")]
    public GearSlot[] emptySlots;

    [Header("All gear transforms in chain order (gear1, gear2 slot, gear3 slot, gear4)")]
    public Transform[] allGears;

    [Header("Spin Settings")]
    public float spinSpeed = 90f;

    [Header("Events")]
    [Tooltip("Fired when all gears are connected. Wire to ElevatorController.UnlockAndOpenDoors.")]
    public UnityEvent OnPuzzleSolved;

    // How many gears in the chain are currently connected (starts at 1: gear 1 always spins)
    private int connectedCount = 1;
    private bool puzzleSolved = false;

    public bool IsChainConnected()
    {
        // Full chain = all 4 gears connected
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
            Debug.Log("Gear puzzle solved!");
            OnPuzzleSolved?.Invoke();
        }

        Debug.Log($"Gear chain now has {connectedCount} connected gears.");
    }

    void Update()
    {
        // Spin connected gears, alternating direction like real meshing gears
        for (int i = 0; i < connectedCount && i < allGears.Length; i++)
        {
            if (allGears[i] == null) continue;
            float dir = (i % 2 == 0) ? 1f : -1f;
            allGears[i].Rotate(Vector3.up * spinSpeed * dir * Time.deltaTime);
        }
    }
}
