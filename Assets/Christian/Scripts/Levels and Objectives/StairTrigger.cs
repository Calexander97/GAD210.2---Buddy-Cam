using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StairTrigger : MonoBehaviour
{
    [Tooltip("Which floor index to switch to when the player enters this trigger.")]
    public int targetFloorIndex = 1;

    [Tooltip("Optional explicit reference. If not set, we'll locate one at runtime.")]
    [SerializeField] private LevelManager levelManager;

    void Reset()
    {
        // Make sure this collider is a trigger.
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var lm = levelManager;
        if (!lm)
        {
            lm = Object.FindFirstObjectByType<LevelManager>();
        }

        if (!lm)
        {
            Debug.LogWarning("StairTrigger: No LevelManager found in scene.");
            return;
        }

        lm.SwitchToFloor(targetFloorIndex);
    }
}
