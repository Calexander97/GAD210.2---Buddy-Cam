using UnityEngine;
using UnityEngine.AI;

/// Minimal AI: Patrol → Alerted (chase/hold) → Investigate (go to LKP then search) → Patrol.
/// - While ALERTED with LOS: keep a single hold distance (no rushing right up).
/// - While ALERTED w/o LOS: run to LKP; only leave Alerted after grace expires.
/// - Never return to Patrol from Alerted unless Investigate finishes.
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    [Header("Data / Sensors")]
    public GuardProfile profile;     // speeds, radii, FOV, etc.
    public GuardSensors sensors;     // provides target, targetVisible, lastSeenPos, heardRecently, lastHeardPos

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.15f;

    [Header("Engage (hold distance)")]
    public float holdDistance = 3.0f;    // desired distance to hero while shooting
    public float replanEvery = 0.2f;     // seconds between SetDestination calls
    public float sampleRadius = 1.5f;    // for NavMesh.SamplePosition

    [Header("Alert → Investigate transition")]
    public float lostSightGrace = 0.8f;  // how long we keep chasing after LOS breaks

    [Header("Investigate")]
    public float dwellAtSpot = 1.0f;     // pause at each search point
    public float searchRadius = 2.0f;    // local wander radius around LKP
    public float searchTime = 6.0f;      // total time to search before Patrol

    [Header("Facing")]
    public float faceLerp = 18f;         // rotate smoothly like hero
    public bool forwardIsUp = true;      // set true if sprite art looks ↑
    public float stopThreshold = 0.02f;  // below this, keep last facing

    public enum State { Patrol, Alerted, Investigate }
    public State state = State.Patrol;

    NavMeshAgent agent;
    int patrolIndex;
    float lostSightTimer;
    float searchTimer;
    float totalSearchTime;
    Vector2 searchCenter;
    float planTimer;
    Vector2 lastMoveDir = Vector2.right;
    HeroHealth targetHealth;
    Transform lastTarget;



    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;      // XY
        agent.updateRotation = false;    // we rotate 2D ourselves
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

        WireTargetHealth(); // <-- subscribe to current target (if set)
    }


    void Update()
    {
        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Alerted: TickAlerted(); break;
            case State.Investigate: TickInvestigate(); break;
        }

        // — Perception transitions —
        if (!sensors || !profile) return;

        if (state == State.Patrol && sensors.heardRecently)
        {
            BeginInvestigate(sensors.lastHeardPos);
        }

    if (sensors)
    {
        var current = sensors.target;
        if (current != lastTarget)
            WireTargetHealth();
    }
    // Face movement like the hero
    FaceByVelocity();
        
    }

    // ---------- States ----------

    void TickPatrol()
    {
        agent.speed = profile ? profile.patrolSpeed : agent.speed;

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // If we SEE the target, go Alerted immediately.
        if (sensors.targetVisible)
        {
            state = State.Alerted;
            agent.speed = profile ? profile.chaseSpeed : agent.speed;
            lostSightTimer = 0f;
            planTimer = 0f;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void TickAlerted()
    {
        agent.speed = profile ? profile.chaseSpeed : agent.speed;

        var tgt = sensors.target;
        if (!tgt || TargetIsDead())

        {
            // hero gone/dead → clean break from combat
            state = State.Patrol;
            agent.ResetPath();
            if (patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[patrolIndex].position);
            return;
        }

        // If we have LOS: stay near holdDistance (don’t rush up).
        if (sensors.targetVisible)
        {
            lostSightTimer = 0f;

            planTimer -= Time.deltaTime;
            if (planTimer <= 0f)
            {
                planTimer = replanEvery;

                Vector2 guardPos = transform.position;
                Vector2 targetPos = tgt.position;
                Vector2 to = (targetPos - guardPos);
                float dist = to.magnitude;

                // Desired “ring” point at holdDistance from the target.
                Vector2 desired = targetPos - to.normalized * Mathf.Max(holdDistance, 0.1f);

                if (NavMesh.SamplePosition(desired, out var hit, sampleRadius, NavMesh.AllAreas))
                {
                    // Only set a new path if we’re not already very close to the desired point.
                    if (!agent.hasPath || Vector2.Distance(agent.destination, (Vector2)hit.position) > 0.1f)
                        agent.SetDestination(hit.position);
                }
                else
                {
                    // Fallback: just head toward the target (NavMesh will sort obstacles)
                    agent.SetDestination(targetPos);
                }
            }
        }
        else
        {
            // No LOS: head to last seen position; only leave Alerted after grace
            lostSightTimer += Time.deltaTime;
            var lkp = sensors.lastSeenPos;

            if (!agent.pathPending && Vector2.Distance(agent.destination, lkp) > 0.1f)
                agent.SetDestination(lkp);

            if (lostSightTimer >= lostSightGrace)
            {
                BeginInvestigate(lkp);
            }
        }

        bool TargetIsDead()
        {
            return !targetHealth || targetHealth.Current <= 0 || !targetHealth.gameObject.activeInHierarchy;
        }
    }

    void TickInvestigate()
    {
        agent.speed = profile ? profile.patrolSpeed : agent.speed;

        // See the player again? Go straight back to Alerted.
        if (sensors.targetVisible)
        {
            state = State.Alerted;
            lostSightTimer = 0f;
            planTimer = 0f;
            return;
        }

        totalSearchTime += Time.deltaTime;

        // Arrived at search point → dwell, then pick another
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= dwellAtSpot)
            {
                searchTimer = 0f;
                PickNewSearchPoint();
            }
        }

        // Search timeout → Patrol
        if (totalSearchTime >= searchTime)
        {
            state = State.Patrol;
            if (patrolPoints != null && patrolPoints.Length > 0)
                agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    // ---------- Transitions / helpers ----------

    void BeginInvestigate(Vector2 center)
    {
        state = State.Investigate;
        searchCenter = center;
        searchTimer = 0f;
        totalSearchTime = 0f;
        PickNewSearchPoint();
    }

    void PickNewSearchPoint()
    {
        var offset = Random.insideUnitCircle * Mathf.Max(0.1f, searchRadius);
        var target = searchCenter + offset;

        if (NavMesh.SamplePosition(target, out var hit, 1.5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(searchCenter);
    }

    void FaceByVelocity()
    {
        var v = agent.velocity;
        Vector2 dir;
        if (v.sqrMagnitude > stopThreshold * stopThreshold)
        {
            dir = new Vector2(v.x, v.y).normalized;
            lastMoveDir = dir;
        }
        else
        {
            dir = lastMoveDir;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var targetRot = Quaternion.Euler(0f, 0f, forwardIsUp ? (angle - 90f) : angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, faceLerp * Time.deltaTime);
    }

    // Called by AlertManager (radio cascade) so other guards join the search.
    // Ignore if this guard is already in a direct chase.
    public void BeginInvestigateExternal(Vector2 center)
    {
        if (state == State.Alerted) return; // keep chasing if we already have LOS
        BeginInvestigate(center);
    }

    void OnTargetDied()
    {
        // stop shooting/chasing and go back to patrol now
        state = State.Patrol;
        if (profile) agent.speed = profile.patrolSpeed;

        // head to current/next patrol waypoint if any
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            if (!agent.hasPath) agent.SetDestination(patrolPoints[patrolIndex].position);
        }
        else
        {
            agent.ResetPath(); // idle if no patrol
        }
    }

    void OnDisable()
    {
        if (targetHealth) targetHealth.OnDied -= OnTargetDied;
    }

    void WireTargetHealth()
    {
        // unsubscribe previous
        if (targetHealth) targetHealth.OnDied -= OnTargetDied;

        targetHealth = null;
        lastTarget = sensors && sensors.target ? sensors.target : null;

        if (lastTarget)
        {
            targetHealth = lastTarget.GetComponent<HeroHealth>();
            if (targetHealth) targetHealth.OnDied += OnTargetDied;
        }
    }
}
