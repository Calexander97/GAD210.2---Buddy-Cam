using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    [Header("Data / Sensors")]
    public GuardProfile profile;
    public GuardSensors sensors;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.15f;

    [Header("Engage")]
    [Tooltip("Preferred standoff distance while firing.")]
    public float engageDistance = 3.0f;
    [Tooltip("Tiny band to avoid micro-oscillation around the distance.")]
    public float engageSlack = 0.35f;

    [Header("Alert → Investigate")]
    [Tooltip("How long to keep heading for LKP after LOS is lost before switching to Investigate search.")]
    public float lostSightGrace = 0.8f;

    [Header("Investigate")]
    [Tooltip("Pause at each random search point.")]
    public float dwellAtSpot = 1.0f;
    [Tooltip("Random search radius around the LKP.")]
    public float searchRadius = 2.0f;
    [Tooltip("Total time to search before returning to patrol.")]
    public float searchTime = 6.0f;

    [Header("Facing")]
    public bool forwardIsUp = true;
    public float turnSpeed = 360f;

    [Header("Stability")]
    [Tooltip("A short cool-down after killing the hero to avoid instant re-triggering.")]
    public float postKillCalm = 1.25f;

    [Header("Radio")]
    [Tooltip("Who hears the LKP when LOS is lost.")]
    public float radioRange = 18f;
    [Tooltip("Optional icon shown briefly when broadcasting.")]
    public GameObject radioIcon;
    public float radioPingDuration = 3f;

    public enum State { Patrol, Alerted, Investigate }
    public State state = State.Patrol;

    // Internals
    NavMeshAgent agent;
    int patrolIndex;
    float lostSightTimer;
    float searchTimer;
    float totalSearchTime;
    Vector2 searchCenter;          // fixed LKP for the current Investigate
    bool goingToLKP = false;       // first phase of Investigate: move to exact LKP

    HeroHealth targetHealth;
    Transform lastTarget;
    Vector2 lastFacingDir = Vector2.right;
    float ignorePerceptionUntil = 0f;

    // Edge detect for LOS → triggers radio once per loss event
    bool hadLOSLastFrame = false;

    void OnEnable() { AlertManager.Instance.Register(this); }

    void OnDisable()
    {
        AlertManager.Instance?.Unregister(this);
        if (targetHealth) targetHealth.OnDied -= OnTargetDied;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        if (!sensors) sensors = GetComponent<GuardSensors>();
    }

    void Start()
    {
        if (profile)
        {
            agent.speed = profile.patrolSpeed;
            searchRadius = profile.searchRadius;
            searchTime = profile.searchTime;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);

        if (radioIcon) radioIcon.SetActive(false);

        WireTargetHealth();
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Alerted: TickAlerted(); break;
            case State.Investigate: TickInvestigate(); break;
        }

        // Heard something while patrolling?
        if (sensors && profile && Time.time >= ignorePerceptionUntil)
        {
            if (state == State.Patrol && sensors.heardRecently)
                BeginInvestigate(sensors.lastHeardPos);
        }

        Face();
    }

    // ── Patrol ──────────────────────────────────────────────────────────────────
    void TickPatrol()
    {
        if (profile) agent.speed = profile.patrolSpeed;
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Immediate switch to Alerted when we see the target (unless calming down)
        if (sensors.targetVisible && Time.time >= ignorePerceptionUntil)
        {
            state = State.Alerted;
            if (profile) agent.speed = profile.chaseSpeed;
            lostSightTimer = 0f;
            hadLOSLastFrame = true; // we are starting Alerted with LOS
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    // ── Alerted ─────────────────────────────────────────────────────────────────
    void TickAlerted()
    {
        if (profile) agent.speed = profile.chaseSpeed;

        var tgt = sensors ? sensors.target : null;
        if (!tgt || TargetIsDead())
        {
            // Clean break: hero gone / dead
            state = State.Patrol;
            agent.ResetPath();
            if (patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[patrolIndex].position);
            ignorePerceptionUntil = Time.time + postKillCalm;
            hadLOSLastFrame = false;
            return;
        }

        bool inLOS = sensors.targetVisible;

        // EDGE: just lost LOS → broadcast immediately and head to LKP
        if (!inLOS && hadLOSLastFrame)
        {
            AlertManager.Instance?.BroadcastLKP(sensors.lastSeenPos, this, radioRange);
            if (radioIcon) StartCoroutine(RadioFlash());
            lostSightTimer = 0f; // start grace period from this instant
        }

        if (inLOS)
        {
            // Maintain a standoff band (simple approach/hold/backoff).
            Vector2 guardPos = transform.position;
            Vector2 targetPos = tgt.position;
            Vector2 to = targetPos - guardPos;
            float dist = to.magnitude;

            float min = Mathf.Max(0.1f, engageDistance - engageSlack);
            float max = engageDistance + engageSlack;

            if (dist > max)
            {
                agent.isStopped = false;
                agent.SetDestination(targetPos);
            }
            else if (dist < min)
            {
                agent.isStopped = false;
                Vector2 desired = targetPos - to.normalized * engageDistance;
                if (NavMesh.SamplePosition(desired, out var hit, 1.25f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                    agent.SetDestination(desired);
            }
            else
            {
                agent.isStopped = true;
                if (agent.hasPath) agent.ResetPath();
            }

            lostSightTimer = 0f; // reset grace while visible
        }
        else
        {
            // While we’re in grace, keep moving to the exact LKP
            lostSightTimer += Time.deltaTime;
            Vector3 lkp = sensors.lastSeenPos;

            agent.isStopped = false;
            if (!agent.pathPending && Vector3.Distance(agent.destination, lkp) > 0.05f)
                agent.SetDestination(lkp);

            // After grace, switch to Investigate (random search around that same LKP)
            if (lostSightTimer >= lostSightGrace)
            {
                BeginInvestigate(sensors.lastSeenPos);
            }
        }

        // Store for next frame’s edge detection
        hadLOSLastFrame = inLOS;
    }

    // ── Investigate ─────────────────────────────────────────────────────────────
    void TickInvestigate()
    {
        if (profile) agent.speed = profile.patrolSpeed;

        // Regain LOS → switch back to Alerted (unless in calm)
        if (sensors.targetVisible && Time.time >= ignorePerceptionUntil)
        {
            state = State.Alerted;
            lostSightTimer = 0f;
            hadLOSLastFrame = true;
            return;
        }

        totalSearchTime += Time.deltaTime;

        // Phase 1: go to exact LKP, then begin local search
        if (goingToLKP)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                goingToLKP = false;
                searchTimer = 0f;
                PickNewSearchPoint(); // first random spot around fixed searchCenter
            }
            else
            {
                if (!agent.hasPath || Vector3.Distance(agent.destination, (Vector3)searchCenter) > 0.05f)
                    agent.SetDestination((Vector3)searchCenter);
            }
        }
        else
        {
            // Phase 2: random local search around the LKP
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                searchTimer += Time.deltaTime;
                if (searchTimer >= dwellAtSpot)
                {
                    searchTimer = 0f;
                    PickNewSearchPoint();
                }
            }
        }

        if (totalSearchTime >= searchTime)
        {
            state = State.Patrol;
            if (patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[patrolIndex].position);
            hadLOSLastFrame = false;
        }
    }

    // ── Transitions / helpers ───────────────────────────────────────────────────
    void BeginInvestigate(Vector2 center)
    {
        state = State.Investigate;
        searchCenter = center;     // lock the LKP once per investigate
        searchTimer = 0f;
        totalSearchTime = 0f;
        goingToLKP = true;
        agent.isStopped = false;
        agent.SetDestination((Vector3)searchCenter);
        hadLOSLastFrame = false;   // we are entering search without LOS
    }

    void PickNewSearchPoint()
    {
        Vector2 offset = Random.insideUnitCircle * Mathf.Max(0.1f, searchRadius);
        Vector2 target = searchCenter + offset;

        if (NavMesh.SamplePosition((Vector3)target, out var hit, 1.5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination((Vector3)searchCenter);
    }

    bool TargetIsDead()
    {
        return !targetHealth || targetHealth.Current <= 0 || !targetHealth.gameObject.activeInHierarchy;
    }

    // ── Facing ──────────────────────────────────────────────────────────────────
    void Face()
    {
        Vector2 desired;

        if (state == State.Alerted && sensors && sensors.target)
            desired = (Vector2)sensors.target.position - (Vector2)transform.position;
        else
        {
            var v = agent.velocity;
            desired = (v.sqrMagnitude > 0.0001f) ? new Vector2(v.x, v.y) : lastFacingDir;
        }

        if (desired.sqrMagnitude < 0.0001f) return;

        desired.Normalize();
        lastFacingDir = desired;

        float angle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
        float z = forwardIsUp ? (angle - 90f) : angle;

        var goal = Quaternion.Euler(0f, 0f, z);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, turnSpeed * Time.deltaTime);
    }

    // ── Health wiring ───────────────────────────────────────────────────────────
    void OnTargetDied()
    {
        ignorePerceptionUntil = Time.time + postKillCalm;
    }

    void WireTargetHealth()
    {
        if (targetHealth) targetHealth.OnDied -= OnTargetDied;

        targetHealth = null;
        lastTarget = sensors && sensors.target ? sensors.target : null;

        if (lastTarget)
        {
            targetHealth = lastTarget.GetComponent<HeroHealth>();
            if (targetHealth) targetHealth.OnDied += OnTargetDied;
        }
    }

    // Called by AlertManager so other guards can join the search.
    public void BeginInvestigateExternal(Vector2 center)
    {
        if (state == State.Alerted) return; // keep chasing if we already have LOS
        BeginInvestigate(center);
    }

    IEnumerator RadioFlash()
    {
        if (!radioIcon) yield break;
        radioIcon.SetActive(true);
        yield return new WaitForSeconds(radioPingDuration);
        radioIcon.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radioRange);
    }
}
