using UnityEngine;

/*
This is the elevator trigger for the clock room, I hook this up to the GearPuzzleManagers
OnPuzzleSolved event in the inspector so when the gear puzzle is solved it slides the
elevator door open by moving it upward over time.

I just drag the elevator door object into the doorTransform slot and set how high I want
it to go with openHeight, pretty simple
*/
public class ElevatorTrigger : MonoBehaviour
{
    public Transform doorTransform;
    public float openHeight = 3f;
    public float openSpeed = 1.5f;

    bool isOpening = false;
    Vector3 targetPos;

    // this gets called when the gear puzzle is solved, it starts the door sliding up
    public void OpenDoor()
    {
        if (doorTransform == null)
        {
            Debug.LogWarning("ElevatorTrigger: no door assigned!");
            return;
        }

        targetPos = doorTransform.position + Vector3.up * openHeight;
        isOpening = true;
        Debug.Log("Elevator door opening!");
    }

    void Update()
    {
        if (!isOpening || doorTransform == null) return;

        // I just lerp the door upward until its close enough to the target
        doorTransform.position = Vector3.MoveTowards(doorTransform.position, targetPos, openSpeed * Time.deltaTime);

        if (Vector3.Distance(doorTransform.position, targetPos) < 0.01f)
        {
            doorTransform.position = targetPos;
            isOpening = false;
            Debug.Log("Elevator door fully open");
        }
    }
}
