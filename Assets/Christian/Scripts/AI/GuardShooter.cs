using UnityEngine;

[RequireComponent(typeof(GuardAI))]
public class GuardShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public float shootRange = 6f;
    public float cooldown = 0.6f;
    public Transform muzzle;

    [Header("Sprites (optional)")]
    public Sprite idleSprite;
    public Sprite shootingSprite;
    [Tooltip("How long to display the shooting sprite after a shot.")]
    public float shootingSpriteTime = 0.12f;
    public SpriteRenderer spriteRenderer;

    GuardAI ai;
    float cd;
    HeroHealth targetHealth;
    float shootSpriteTimer;

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
        if (shootSpriteTimer > 0f) shootSpriteTimer -= Time.deltaTime;

        // Update sprite state
        if (spriteRenderer)
        {
            if (shootSpriteTimer > 0f && shootingSprite)
                spriteRenderer.sprite = shootingSprite;
            else if (idleSprite)
                spriteRenderer.sprite = idleSprite;
        }

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

        // Flash shooting sprite briefly
        shootSpriteTimer = shootingSpriteTime;
    }
}
