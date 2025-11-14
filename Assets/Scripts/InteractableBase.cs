using UnityEngine;

/// Base class for things the hero can use.
/// Override Interact() to implement behaviour.
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected Transform interactPoint; // optional offset for where the hero should stand

    public virtual bool CanInteract(Transform actor) => true;

    public virtual Vector2 GetInteractPoint() =>
        interactPoint ? (Vector2)interactPoint.position : (Vector2)transform.position;

    public abstract void Interact(Transform actor);
}
