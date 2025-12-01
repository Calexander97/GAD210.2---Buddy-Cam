using UnityEngine;

/// Place this on the main terminal object (must have a 2D collider for click/overlap).
/// Implements Interactable logic
public class MainDataTerminal : InteractableBase
{   
    // Integrate with Brent's inventory system
    [Header("Inventory integration")]
    public bool addInventoryItem = false;
    public string itemId = "DataPackage";

    public override void Interact(Transform actor)
    {
        // Mark objective
        ObjectiveManager.I?.AcquireData();

        // Add item to inventory system
        if (addInventoryItem)
        {
            // Replace with your Brent's API call if available, e.g.:
            // actor.GetComponent<Inventory>()?.Add(itemId);
            Debug.Log($"[Inventory] Added {itemId} to hero.");
        }
    }
}