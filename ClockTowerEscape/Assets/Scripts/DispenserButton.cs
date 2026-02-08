using UnityEngine;

public class DispenserButton : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawn_position;
    public Transform button_visual_position;
    public Transform prefab_to_spawn;
    public Vector3 pressOffset = new Vector3(0.05f, 0, 0);

    private Transform current_spawned_prefab;
    private Vector3 originalButtonPosition;

    // Button state
    bool is_button_pressed = false;
    bool is_ready_to_unpress = false;
    int hand_colliders_inside = 0;
    bool has_spawned_this_press = false;

    void Start()
    {
        if (button_visual_position != null)
            originalButtonPosition = button_visual_position.position;
    }

    // ========== VR Trigger Interaction ==========

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hand"))
        {
            hand_colliders_inside++;

            if(hand_colliders_inside == 1 && !has_spawned_this_press)
            {
                Debug.Log("button is pressed!");
                is_button_pressed = true;
                has_spawned_this_press = true;

                if (GameManager.Instance != null) GameManager.Instance.PlaySound(0);

                button_visual_position.position = originalButtonPosition + pressOffset;
                Invoke(nameof(ReadyToUnpress), 1f);

                if (current_spawned_prefab != null)
                {
                    if (current_spawned_prefab.GetComponent<CollisionChecker>().isAtSpawnLocation == true)
                    {
                        Destroy(current_spawned_prefab.gameObject);
                    }
                    current_spawned_prefab = null;
                }
                current_spawned_prefab = SpawnPrefab();
            }
        }
        return;
    }

    Transform SpawnPrefab()
    {
        Transform spawned = Instantiate(prefab_to_spawn, spawn_position.position, spawn_position.rotation);
        return spawned;
    }

    void ReadyToUnpress()
    {
        is_ready_to_unpress = true;
        if (hand_colliders_inside <= 0)
        {
            Unpress();
        }
    }

    void Unpress()
    {
        is_button_pressed = false;
        is_ready_to_unpress = false;
        Debug.Log("button is unpressed!");
        button_visual_position.position = originalButtonPosition;
    }

    // Called by Desktop mode raycast when player clicks on the button
    public void DesktopPress()
    {
        if (is_button_pressed) return;

        Debug.Log("button is pressed (desktop)!");
        is_button_pressed = true;

        if (GameManager.Instance != null) GameManager.Instance.PlaySound(0);

        button_visual_position.position = originalButtonPosition + pressOffset;

        if (current_spawned_prefab != null)
        {
            if (current_spawned_prefab.GetComponent<CollisionChecker>().isAtSpawnLocation == true)
            {
                Destroy(current_spawned_prefab.gameObject);
            }
            current_spawned_prefab = null;
        }
        current_spawned_prefab = SpawnPrefab();

        Invoke(nameof(DesktopUnpress), 1f);
    }

    void DesktopUnpress()
    {
        if (!is_button_pressed) return;
        is_button_pressed = false;
        Debug.Log("button is unpressed (desktop)!");
        button_visual_position.position = originalButtonPosition;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Hand"))
        {
            hand_colliders_inside--;
            if (hand_colliders_inside < 0) hand_colliders_inside = 0;

            if(hand_colliders_inside <= 0)
            {
                has_spawned_this_press = false;

                if(is_button_pressed && is_ready_to_unpress)
                {
                    Unpress();
                }
            }
        }
    }
}
