using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class LanternPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Drag the 6 lanterns in the CORRECT lighting order from the scene")]
    public LanternInteractable[] correctOrder;

    [Header("Events")]
    [Tooltip("Fired when the puzzle is solved. Wire to ElevatorController.UnlockAndOpenDoors.")]
    public UnityEvent OnPuzzleSolved;

    [Header("Local Audio Clips (Optional)")]
    [Tooltip("If assigned, this plays for wrong order")]
    public AudioClip failClip;
    [Range(0f, 1f)]
    public float puzzleSfxVolume = 1f;

    private List<LanternInteractable> litOrder = new List<LanternInteractable>();

    // Called by LanternInteractable when a lantern is lit
    public void OnLanternLit(LanternInteractable lantern)
    {
        litOrder.Add(lantern);

        Debug.Log($"Lantern lit: {lantern.name}. Total lit: {litOrder.Count}/{correctOrder.Length}");

        // Check solution when all lanterns are lit
        if (litOrder.Count >= correctOrder.Length)
            CheckSolution();
    }

    private void CheckSolution()
    {
        bool correct = true;

        // Compare the lit order with the correct order
        for (int i = 0; i < correctOrder.Length; i++)
        {
            if (litOrder[i] != correctOrder[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            Debug.Log("Puzzle solved! Opening elevator door.");
            OpenElevatorDoor();
        }
        else
        {
            Debug.Log("Wrong order! Resetting all lanterns.");
            StartCoroutine(ResetAllLanterns());
        }
    }

    private IEnumerator ResetAllLanterns()
    {
        PlayFailSound();

        // Wait a moment before extinguishing
        yield return new WaitForSeconds(1f);

        // Extinguish all lanterns
        foreach (var lantern in correctOrder)
        {
            if (lantern != null)
                lantern.Extinguish();
        }

        litOrder.Clear();
    }

    private void OpenElevatorDoor()
    {
        // Trigger the elevator to unlock and open
        OnPuzzleSolved?.Invoke();
    }

    private void PlayFailSound()
    {
        if (failClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySpatialClip(failClip, transform.position, puzzleSfxVolume, 1f);
    }

}
