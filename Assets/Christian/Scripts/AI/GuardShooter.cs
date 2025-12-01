using UnityEngine;

[RequireComponent(typeof(GuardAI))]
public class GuardShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public float shootRange = 6f;
    public float cooldown = 0.6f;
    public Transform muzzle;

    GuardAI ai;
    float cd;
    HeroHealth targetHealth;

    void Awake() => ai = GetComponent<GuardAI>();

    void LateUpdate()
    {
        // refresh target + health ref if needed
        if (ai && ai.sensors && (!targetHealth || ai.sensors.target != targetHealth.transform))
        {
            var t = ai.sensors.target;
            targetHealth = t ? t.GetComponent<HeroHealth>() : null;
        }

        if (cd > 0f) cd -= Time.deltaTime;

        // Only shoot when actively Alerted, target visible, and target alive
        if (!ai || ai.state != GuardAI.State.Alerted) return;
        if (!ai.sensors || !ai.sensors.targetVisible) return;
        if (!targetHealth || targetHealth.Current <= 0 || !targetHealth.gameObject.activeInHierarchy) return;

        Vector2 to = ai.sensors.target.position - transform.position;
        if (to.sqrMagnitude > shootRange * shootRange) return;
        if (cd > 0f) return;

        cd = cooldown;
        var proj = Instantiate(projectilePrefab, muzzle ? muzzle.position : transform.position, Quaternion.identity);
        proj.transform.right = to.normalized;
        proj.Launch(to);
    }
}
