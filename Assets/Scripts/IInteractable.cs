using UnityEngine;

/// Minimal contract for world interactions.
public interface IInteractable
{
    bool CanInteract(Transform actor); // gate conditions (locked, power off, etc.)
    Vector2 GetInteractPoint();        // where the hero should stand
    void Interact(Transform actor);    // perform the action
}
