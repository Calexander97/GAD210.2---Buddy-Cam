using UnityEngine;

public class Interactable : MonoBehaviour
{
    public InteractionManager interactionManager;

    public bool PlayerInRange
    {
        get
        {
            return interactionManager != null && interactionManager.CurrentInteractable == this;
        }
    }

    public virtual void Interact(GameObject player)
    {
        Debug.Log("Interacted with: " + gameObject.name);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && interactionManager != null)
        {
            interactionManager.SetCurrentInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && interactionManager != null)
        {
            interactionManager.ClearCurrentInteractable(this);
        }
    }
}
