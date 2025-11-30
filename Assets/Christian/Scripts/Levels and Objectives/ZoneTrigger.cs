using UnityEngine;

/// Put this on a 2D trigger.
/// Set ZoneType = Entry for the start zone (optional).
/// Set ZoneType = Exit for the extraction zone (uses ObjectiveManager conditions).
[RequireComponent(typeof(Collider2D))]
public class ZoneTrigger : MonoBehaviour
{
    public enum ZoneType { Entry, Exit }
    [Header("Zone")]
    public ZoneType type = ZoneType.Entry;

    [Header("Who can trigger")]
    public string playerTag = "Player";

    [Header("Exit requirements (used only when ZoneType = Exit)")]
    public bool requireData = true;        // must have downloaded data
    public bool requireAltExit = false;    // alt exit must be unlocked

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var om = ObjectiveManager.I;
        if (!om) return;

        if (type == ZoneType.Entry)
        {
            // Optional: if your ObjectiveManager has a Start/Begin call, use it.
            // om.StartMission(); // only if you implemented it.
            Debug.Log("[ZoneTrigger] Entry reached.");
            return;
        }

        // Exit logic
        if (type == ZoneType.Exit)
        {
            if (requireData && !om.HasData)
            {
                Debug.Log("[ZoneTrigger] Exit blocked: data not acquired yet.");
                return;
            }

            if (requireAltExit && !om.AltExitUnlocked)
            {
                Debug.Log("[ZoneTrigger] Exit blocked: alternate exit locked.");
                return;
            }

            Debug.Log("[ZoneTrigger] Mission Complete!");
            om.CompleteMission();
        }
    }
}
