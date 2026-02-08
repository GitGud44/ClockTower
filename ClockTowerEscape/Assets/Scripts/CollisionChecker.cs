using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    public bool isAtSpawnLocation = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("DispenserSpawnPlatform"))
        {
            Debug.Log("Prefab is at spawn location");
            isAtSpawnLocation = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("DispenserSpawnPlatform"))
        {
            Debug.Log("Prefab is away from spawn location");
            isAtSpawnLocation = false;
        }
    }
}
