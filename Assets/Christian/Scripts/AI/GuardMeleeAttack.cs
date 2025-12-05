using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// Attach to a melee guard (with GuardAI.engageStyle = Melee).
[RequireComponent(typeof(GuardAI))]
public class GuardMeleeAttack : MonoBehaviour
{
    [Header("Attack")]
    [Tooltip("Distance required to land a hit. (Agent stopping distance will be set a bit smaller than this.)")]
    public float attackRange = 0.9f;
    [Tooltip("Radius used for the hit check (defaults to attackRange if <= 0).")]
    public float hitRadius = 0f;
    [Tooltip("Damage per hit (hearts).")]
    public int damage = 1;
    [Tooltip("Windup time before the hit connects.")]
    public float windupTime = 0.25f;
    [Tooltip("Cooldown after the hit connects.")]
    public float recoveryTime = 0.35f;

    [Header("Hit Detection")]
    [Tooltip("Layers eligible to receive melee damage (include Player).")]
    public LayerMask damageMask;

    [Header("Sprites (optional)")]
    public Sprite idleSprite;      // standing / moving
    public Sprite windupSprite;    // bat pulled back
    public Sprite swingSprite;     // bat forward
    [Tooltip("Where to render the sprite (defaults to same GameObject).")]
    public SpriteRenderer spriteRenderer;

    GuardAI ai;
    NavMeshAgent agent;
    float attackCd;       // cooldown timer
    bool isWinding;
    bool isSwinging;

    // cached so we can ignore collisions (no pushing)
    Collider2D[] guardCols;
    Collider2D[] playerCols;

    void Awake()
    {
        ai = GetComponent<GuardAI>();
        agent = GetComponent<NavMeshAgent>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

        // gather our 2D colliders
        guardCols = GetComponentsInChildren<Collider2D>(includeInactive: true);
    }

    void Start()
    {
        // Ensure this behaves as melee regardless of prefab oversight
        if (ai) ai.engageStyle = GuardAI.EngageStyle.Melee;

        // Make sure agent stops just shy of hit distance (prevents back-off band).
        float desiredStop = Mathf.Clamp(attackRange * 0.7f, 0.05f, Mathf.Max(0.05f, attackRange - 0.05f));
        if (ai) ai.meleeStopDistance = desiredStop;
        if (agent) agent.stoppingDistance = desiredStop;

        // First-time collision ignore set up
        RefreshPlayerCollidersAndIgnore();
    }

    void Update()
    {
        // keep ignoring collisions even if the target swapped/enabled later
        if (Time.frameCount % 15 == 0) RefreshPlayerCollidersAndIgnore();

        if (attackCd > 0f) attackCd -= Time.deltaTime;

        // Only attack while Alerted and with a visible, alive target
        if (!ai || ai.state != GuardAI.State.Alerted || ai.sensors == null || !ai.sensors.targetVisible)
        {
            SetIdleSprite();
            return;
        }

        var tgt = ai.sensors.target;
        if (!tgt)
        {
            SetIdleSprite();
            return;
        }

        // If we’re not in range or mid-animation/cooldown, do nothing (GuardAI keeps pushing in)
        float dist = Vector2.Distance(transform.position, tgt.position);
        float neededRange = Mathf.Max(0.05f, attackRange);
        if (dist > neededRange || attackCd > 0f || isWinding || isSwinging)
        {
            if (!isWinding && !isSwinging) SetIdleSprite();
            return;
        }

        // Start an attack
        StartCoroutine(DoAttack());
    }

    IEnumerator DoAttack()
    {
        // Freeze agent movement during the attack window so we don't slide/push
        bool hadPath = agent && agent.hasPath;
        if (agent) agent.isStopped = true;

        // Windup
        isWinding = true;
        SetWindupSprite();
        float t = windupTime;
        while (t > 0f)
        {
            // If target visibility completely lost, we still commit (feels snappier);
            // comment next two lines back in to cancel on LOS loss during windup.
            // if (ai == null || ai.sensors == null || !ai.sensors.targetVisible) { isWinding = false; goto EndAttack; }

            t -= Time.deltaTime;
            yield return null;
        }
        isWinding = false;

        // Swing / impact frame
        isSwinging = true;
        SetSwingSprite();

        // Deal damage via overlap (robust even if colliders/layers differ)
        float r = (hitRadius > 0f) ? hitRadius : attackRange;
        var hits = Physics2D.OverlapCircleAll(transform.position, r, damageMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var hh = hits[i].GetComponentInParent<HeroHealth>() ?? hits[i].GetComponent<HeroHealth>();
            if (hh && hh.gameObject.activeInHierarchy)
            {
                hh.TakeHit(Mathf.Max(1, damage));
                break; // one hit is enough
            }
        }

        // tiny moment to display the swing
        yield return null;

        // Recovery
        yield return new WaitForSeconds(recoveryTime);
        isSwinging = false;
        attackCd = 0.1f; // small spacer so we don't double-tick same frame

    EndAttack:
        SetIdleSprite();

        // Let the agent move again
        if (agent)
        {
            agent.isStopped = false;
            // re-issue a destination toward target so it resumes pursuit smoothly
            if (ai && ai.sensors && ai.sensors.target) agent.SetDestination(ai.sensors.target.position);
        }
    }

    // ── Collision ignore to prevent pushing ─────────────────────────────────────
    void RefreshPlayerCollidersAndIgnore()
    {
        Transform target = (ai && ai.sensors) ? ai.sensors.target : null;
        if (!target) return;

        // get player's colliders
        playerCols = target.GetComponentsInChildren<Collider2D>(includeInactive: true);
        if (playerCols == null || guardCols == null) return;

        // ignore collisions between all guard colliders and player colliders
        for (int i = 0; i < guardCols.Length; i++)
        {
            for (int j = 0; j < playerCols.Length; j++)
            {
                if (guardCols[i] && playerCols[j])
                    Physics2D.IgnoreCollision(guardCols[i], playerCols[j], true);
            }
        }
    }

    // ── Sprites ─────────────────────────────────────────────────────────────────
    void SetIdleSprite()
    {
        if (spriteRenderer && idleSprite) spriteRenderer.sprite = idleSprite;
    }
    void SetWindupSprite()
    {
        if (spriteRenderer && windupSprite) spriteRenderer.sprite = windupSprite;
    }
    void SetSwingSprite()
    {
        if (spriteRenderer && swingSprite) spriteRenderer.sprite = swingSprite;
    }

    // editor convenience
    void OnValidate()
    {
        if (attackRange < 0.05f) attackRange = 0.05f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        float r = (hitRadius > 0f) ? hitRadius : attackRange;
        Gizmos.DrawWireSphere(transform.position, r);
    }
#endif
}
