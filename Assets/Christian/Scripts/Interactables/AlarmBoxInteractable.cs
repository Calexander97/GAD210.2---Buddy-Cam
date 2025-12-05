using UnityEngine;

/// Drop this on the same GameObject as AlarmBox (and a 2D trigger collider).
/// It integrates with the existing InteractionManager without modifying it.
[RequireComponent(typeof(AlarmBox))]
[RequireComponent(typeof(Collider2D))]
public class AlarmBoxInteractable : Interactable
{
    [Header("Setup")]
    [Tooltip("Tag used by your player GameObject if InteractionManager.player is not set.")]
    public string playerTag = "Player";

    [SerializeField] private InteractionManager _managerOverride; // renamed to avoid clash

    private InteractionManager Manager
    {
        get
        {
            if (_managerOverride) return _managerOverride;
            // If the base class already exposes a manager, prefer that (rename if your base uses a different name)
            // return base.interactionManager; // <- uncomment if your Interactable has this field
            // Otherwise find one in the scene:
            _managerOverride = FindFirstObjectByType<InteractionManager>();
            return _managerOverride;
        }
    }

    private AlarmBox alarm;

    void Awake()
    {
        alarm = GetComponent<AlarmBox>();

        // Ensure trigger setup
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;

        // Warm the cache
        var _ = Manager;
    }

    void OnEnable()
    {
        // If re-enabled while the player is inside, InteractionManager will set us again on next OnTriggerEnter2D
    }

    void OnDisable()
    {
        // Defensive: if disabled while selected, clear selection
        if (Manager && Manager.CurrentInteractable == this)
            Manager.ClearCurrentInteractable(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var mgr = Manager;
        if (!mgr) return;

        bool isPlayer =
            (mgr.player && other.gameObject == mgr.player) ||
            (!mgr.player && other.CompareTag(playerTag));

        if (isPlayer)
            mgr.SetCurrentInteractable(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var mgr = Manager;
        if (!mgr) return;

        bool isPlayer =
            (mgr.player && other.gameObject == mgr.player) ||
            (!mgr.player && other.CompareTag(playerTag));

        if (isPlayer)
            mgr.ClearCurrentInteractable(this);
    }

    // InteractionManager calls this when the button is pressed.
    public override void Interact(GameObject actor)
    {
        if (!alarm) return;
        alarm.TriggerAlarm(actor);
    }
}
