using UnityEngine;

[DisallowMultipleComponent]
public class GuardSensors : MonoBehaviour
{
    public GuardProfile profile;
    public Transform target;

    [Header("FOV Axis")]
    [Tooltip("Enable if your sprite art faces Up (↑). If it faces Right (→), leave off.")]
    public bool forwardIsUp = true;

    // Signals
    public bool targetVisible { get; private set; }
    public Vector2 lastSeenPos { get; private set; }
    public Vector2 lastHeardPos { get; private set; }
    public bool heardRecently { get; private set; }

    float heardTimer;

    void OnEnable() => NoiseSystem.OnNoise += OnNoiseHeard;
    void OnDisable() => NoiseSystem.OnNoise -= OnNoiseHeard;

    void Update()
    {
        targetVisible = CheckVision();
        if (targetVisible && target) lastSeenPos = target.position;

        if (heardRecently)
        {
            heardTimer -= Time.deltaTime;
            if (heardTimer <= 0f) heardRecently = false;
        }
    }

    bool CheckVision()
    {
        if (!profile || !target) return false;

        Vector2 origin = transform.position;
        Vector2 toTgt = (Vector2)target.position - origin;

        // choose the sprite's "forward"
        Vector2 forward = forwardIsUp ? (Vector2)transform.up : (Vector2)transform.right;

        // range + cone
        if (toTgt.magnitude > profile.visionRange) return false;
        if (Vector2.Angle(forward, toTgt) > profile.fov * 0.5f) return false;

        // LOS: raycast against LOS mask (typically Walls)
        var hit = Physics2D.Raycast(origin, toTgt.normalized, toTgt.magnitude, profile.losMask);
        // If nothing blocked OR the first thing hit is the target (when target layer is included), we see them
        if (!hit) return true;
        return hit.transform == target || hit.transform.root == target;
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
}
