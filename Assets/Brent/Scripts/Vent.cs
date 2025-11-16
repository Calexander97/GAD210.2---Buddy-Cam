using UnityEngine;

public class Vent : Interactable
{
    [Header("Vent Settings")]
    public Transform destination;

    public override void Interact(GameObject player)
    {
        if (!PlayerInRange || player == null || destination == null)
            return;

        player.transform.position = destination.position;
        Debug.Log("Player vented to: " + destination.position);
    }
}