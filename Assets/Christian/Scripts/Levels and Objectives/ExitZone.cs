using UnityEngine;

/// Attach to a trigger area (BoxCollider2D isTrigger = true).
/// If 'requireAltExit' is true, this exit only works after AltExitTerminal is hacked.
/// If 'requireData' is true, hero must have acquired data.
[RequireComponent(typeof(Collider2D))]
public class ExitZone : MonoBehaviour
{
    [Header("Requirements")]
    public bool requireData = true;
    public bool requireAltExit = false;

    [Header("Who can exit")]
    [Tooltip("Tag the hero with this tag (default: Player).")]
    public string playerTag = "Player";

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Correct API is CompareTag (not GetCompareTag)
        if (!other.CompareTag(playerTag)) return;

        // Correct singleton name: ObjectiveManager (not ObjectiveManger)
        var om = ObjectiveManager.I;
        if (!om) return;

        // Respect requirements
        if (requireData && !om.HasData)
        {
            Debug.Log("[Exit] Blocked: data not acquired.");
            return;
        }

        if (requireAltExit && !om.AltExitUnlocked)
        {
            Debug.Log("[Exit] Blocked: alt exit locked.");
            return;
        }

        om.CompleteMission();
    }
}
