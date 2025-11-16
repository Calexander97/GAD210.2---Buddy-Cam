using UnityEngine;

public class Vent : Interactable
{
    [Header("Vent Settings")]
    public Transform destination;

    public override void Interact(GameObject player)
    {
        if (!PlayerInRange || player == null || destination == null)
            return;

        HeroNavAgent2D agentComponent = player.GetComponent<HeroNavAgent2D>();
        if (agentComponent != null && agentComponent.agent != null)
        {
            agentComponent.agent.Warp(destination.position);
            Debug.Log("Player vented to: " + destination.position);
        }
        else
        {
            player.transform.position = destination.position;
            Debug.Log("Player vented (no agent found) to: " + destination.position);
        }
    }
}