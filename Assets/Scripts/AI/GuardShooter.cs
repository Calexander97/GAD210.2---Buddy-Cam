using UnityEngine;

/// Fires if the AI is Alerted, has LOS, and target is within shootRange.
/// Movement is GuardAI’s job. No approach/hold logic here.
[RequireComponent(typeof(GuardAI))]
public class GuardShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform muzzle;          // optional; uses transform if null
    public float shootRange = 4f;     // keep ≥ than whatever distance you expect to shoot at
    public float cooldown = 0.35f;    // fire rate limiter

    GuardAI ai;
    float cd;

    void Awake() => ai = GetComponent<GuardAI>();

    void Update()
    {
        if (cd > 0f) cd -= Time.deltaTime;
        if (!ai || ai.state != GuardAI.State.Alerted) return;
        var s = ai.sensors; if (!s || !s.target) return;

        // Must have LOS and be in range
        Vector2 to = s.target.position - transform.position;
        if (!s.targetVisible || to.sqrMagnitude > shootRange * shootRange) return;

        if (cd <= 0f)
        {
            cd = cooldown;
            var spawn = muzzle ? muzzle.position : transform.position;
            var proj = Instantiate(projectilePrefab, spawn, Quaternion.identity);
            proj.transform.right = to.normalized;
            proj.Launch(to);
        }
    }
}
