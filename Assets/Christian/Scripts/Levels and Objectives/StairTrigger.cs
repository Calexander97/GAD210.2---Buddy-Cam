using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StairTrigger : MonoBehaviour
{
    [Tooltip("Floor index to switch to when the player enters this trigger.")]
    public int targetFloorIndex = 1;

    [Tooltip("Where on the destination floor the hero should land.")]
    public Transform landingPoint;

    [Tooltip("Optional explicit reference. If not set, we locate one at runtime.")]
    [SerializeField] private LevelManager levelManager;

    [Header("Re-trigger safety")]
    [Tooltip("Minimum time (seconds) after any stair switch before another stair can trigger.")]
    public float globalCooldown = 0.5f;

    [Tooltip("Force the player to fully exit this trigger before it can fire again.")]
    public bool requireExitBeforeRetrigger = true;

    // shared across all StairTriggers so arriving inside another stair doesn't immediately fire
    private static float s_lastSwitchTime = -999f;

    // per-trigger gate so standing in the area won’t spam
    private bool _playerInside;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Global cooldown: ignore if we just switched floors
        if (Time.unscaledTime - s_lastSwitchTime < globalCooldown) return;

        // Must leave before we can trigger again (prevents flip-flop while standing still)
        if (requireExitBeforeRetrigger && _playerInside) return;

        _playerInside = true;

        var lm = levelManager ? levelManager : FindFirstObjectByType<LevelManager>();
        if (!lm) { Debug.LogWarning("StairTrigger: No LevelManager found."); return; }

        // Stamp cooldown BEFORE switching so the destination trigger ignores initial overlap
        s_lastSwitchTime = Time.unscaledTime;

        if (landingPoint)
            lm.SwitchToFloor(targetFloorIndex, landingPoint);  // uses your explicit landing Transform
        else
            lm.SwitchToFloor(targetFloorIndex);                // uses floor’s default spawn
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
    }
}
