using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LanternPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Drag the 6 lanterns in the CORRECT lighting order from the scene")]
    public LanternInteractable[] correctOrder;

    [Header("References")]
    [Tooltip("The elevator door placeholder to deactivate when puzzle is solved")]
    public GameObject elevatorDoor;

    [Header("Audio Clip Indices")]
    [Tooltip("Index in GameManager.audioClips for the LightLantern sound")]
    public int lightLanternSoundIndex = 2;
    [Tooltip("Index in GameManager.audioClips for success sound (optional)")]
    public int successSoundIndex = -1;
    [Tooltip("Index in GameManager.audioClips for fail/reset sound (optional)")]
    public int failSoundIndex = -1;

    private List<LanternInteractable> litOrder = new List<LanternInteractable>();

    // Called by LanternInteractable when a lantern is lit
    public void OnLanternLit(LanternInteractable lantern)
    {
        litOrder.Add(lantern);

        // Play light sound
        if (GameManager.Instance != null && lightLanternSoundIndex >= 0)
            GameManager.Instance.PlaySound(lightLanternSoundIndex);

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
        // Play fail sound
        if (GameManager.Instance != null && failSoundIndex >= 0)
            GameManager.Instance.PlaySound(failSoundIndex);

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
        // Play success sound
        if (GameManager.Instance != null && successSoundIndex >= 0)
            GameManager.Instance.PlaySound(successSoundIndex);

        // Hide the elevator door placeholder
        if (elevatorDoor != null)
            elevatorDoor.SetActive(false);
    }
}
