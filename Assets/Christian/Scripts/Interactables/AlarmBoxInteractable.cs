using UnityEngine;

/// Drop this on the same GameObject as AlarmBox (and a 2D trigger collider).
/// It integrates with the existing InteractionManager.
[RequireComponent(typeof(AlarmBox))]
[RequireComponent(typeof(Collider2D))]
public class AlarmBoxInteractable : Interactable
{
    [Header("Setup")]
    [Tooltip("Tag used by your player GameObject. Must match InteractionManager.player.")]
    public string playerTag = "Player";

    public InteractionManager interactionManager;

    AlarmBox alarm;

    void Awake()
    {
        alarm = GetComponent<AlarmBox>();
        // Ensure trigger setup
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;

        if (!interactionManager)
            interactionManager = FindFirstObjectByType<InteractionManager>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!interactionManager) return;

       
        var isPlayer =
            (interactionManager.player && other.gameObject == interactionManager.player) ||
            (!interactionManager.player && other.CompareTag(playerTag));

        if (isPlayer)
        {
            interactionManager.SetCurrentInteractable(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!interactionManager) return;

        var isPlayer =
            (interactionManager.player && other.gameObject == interactionManager.player) ||
            (!interactionManager.player && other.CompareTag(playerTag));

        if (isPlayer)
        {
            interactionManager.ClearCurrentInteractable(this);
        }
    }

    // InteractionManager calls this when the button is pressed.
    public override void Interact(GameObject actor)
    {
        if (!alarm) return;
        alarm.TriggerAlarm(actor);
    }
}
