using UnityEngine;

/// Handles vision + hearing for a guard and exposes positions,
[DisallowMultipleComponent]
public class GuardSensors : MonoBehaviour
{
    public GuardProfile profile;
    public Transform target;        // usually the hero

    // Read-only signals for the AI
    public bool targetVisible { get; private set; }
    public Vector2 lastSeenPos { get; private set; }
    public Vector2 lastHeardPos { get; private set; }
    public bool heardRecently { get; private set; }

    float heardTimer;

    void OnEnable() => NoiseSystem.OnNoise += OnNoiseHeard;
    void OnDisable() => NoiseSystem.OnNoise -= OnNoiseHeard;

    void Update()
    {
        // Vision chech every frame
        targetVisible = CheckVision();
        if (targetVisible) lastSeenPos = target.position;

        // Small grace window for hearing so AI has time to react
        if (heardRecently)
        {
            heardTimer -= Time.deltaTime;
            if (heardTimer <= 0f) heardRecently = false;
        }
    }

    // Cone + LOS raycast on XY (uses Physics2D)
    bool CheckVision()
    {
        if (!profile || !target) return false;

        Vector2 origin = transform.position;
        Vector2 toTgt = (Vector2)target.position - origin;

        // Range and cone angle 
        if (toTgt.magnitude > profile.visionRange) return false;
        if (Vector2.Angle(transform.right, toTgt) > profile.fov * 0.5f) return false;

        // First hit must be the target for valid line of sight
        var hit = Physics2D.Raycast(origin, toTgt.normalized, toTgt.magnitude, profile.losMask);
        if (!hit) return false;

        return (hit.transform == target || hit.transform.root == target);
    }

    void OnNoiseHeard(Vector2 pos, float loudness)
    {
        if (!profile) return;

        float maxDist = profile.hearingRadius * Mathf.Max(0.01f, loudness);
        if (Vector2.Distance(transform.position, pos) <= maxDist)
        {
            heardRecently = true;
            heardTimer = 1.2f;
            lastHeardPos = pos;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!profile) return;

        Gizmos.color = profile.gizmoColor;
        Gizmos.DrawWireSphere(transform.position, profile.visionRange);
        Gizmos.DrawWireSphere(transform.position, profile.hearingRadius);

        // FOV fan (assumes +X is forward)
        Vector3 p = transform.position;
        float a = profile.fov * 0.5f;
        Vector3 left = Quaternion.Euler(0, 0, a) * transform.right;
        Vector3 right = Quaternion.Euler(0, 0, -a) * transform.right;
        Gizmos.DrawRay(p, left * profile.visionRange);
        Gizmos.DrawRay(p, right * profile.visionRange);
    }
}
