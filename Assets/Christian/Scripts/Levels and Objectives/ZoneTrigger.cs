using UnityEngine;

/// Drop this on a trigger collider 2D. Tag = "Entry" or "Exit".
public class ZoneTrigger : MonoBehaviour
{
    public enum ZoneType { Entry, Exit }
    public ZoneType type;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (!c.CompareTag("Hero")) return;

        var om = ObjectiveManager.I;
        if (!om) return;

        if (type == ZoneType.Entry)
        {
            om.entryReached = true;
            // e.g., enable exit when primaries complete, or immediately:
            // om.exitEnabled = true;
        }
        else if (type == ZoneType.Exit)
        {
            if (om.exitEnabled && om.AllPrimariesComplete())
            {
                Debug.Log("Mission Complete!");
                // trigger next scene / summary
            }
        }
    }
}
