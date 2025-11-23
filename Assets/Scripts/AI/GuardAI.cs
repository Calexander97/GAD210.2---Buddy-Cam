using UnityEngine;
using UnityEngine.AI;

/// Minimal guard: Patrol until first detection, then never auto-return to patrol.
/// States:
///   Patrol  -> (see target) -> Alerted
///   Alerted -> (lose LOS)   -> SearchLastSeen
///   SearchLastSeen keeps waiting/wandering at LKP; no auto return to Patrol.
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    [Header("Data / Sensors")]
    public GuardProfile profile;     // speeds etc.
    public GuardSensors sensors;     // targetVisible, lastSeenPos, heardRecently, target

    [Header("Patrol (only before first detection)")]
    public Transform[] patrolPoints;
    public float patrolWaypointTolerance = 0.15f;

    [Header("Search (after LOS breaks)")]
    public float lostSightGrace = 0.8f;  // how long to keep chasing after LOS breaks
    public float searchDwellTime = 1.2f; // pause between local search hops
    public float searchRadius = 2.5f;    // wander radius around LKP
    public float navSampleRadius = 1.5f; // sampling radius for NavMesh.SamplePosition

    [Header("Facing (sprite)")]
    public float faceLerp = 18f;
    public bool forwardIsUp = true;
    public float stopThreshold = 0.02f;

    public enum State { Patrol, Alerted, SearchLastSeen }
    public State state = State.Patrol;

    [Header("Debug")]
    public bool debugLogs = false;

    // internals
    NavMeshAgent agent;
    int patrolIndex;
    float lostSightTimer;
    float dwellTimer;
    Vector2 searchCenter;      // LKP
    Vector2 lastMoveDir = Vector2.right;
    bool firstDetectionOccurred = false; // once true, we never auto-return to patrol

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;   // XY plane
        agent.updateRotation = false; // we rotate sprites manually
        if (!sensors) sensors = GetComponent<GuardSensors>();
    }

    void OnEnable() => AlertManager.Instance?.Register(this);
    void OnDisable() => AlertManager.Instance?.Unregister(this);

    void Start()
    {
        if (profile) agent.speed = profile.patrolSpeed;

        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void Update()
    {
        // Perception and state transitions
        if (sensors && sensors.target)
        {
            if (sensors.targetVisible)
            {
                // Lock into Alerted forever (for this prototype)
                if (state != State.Alerted)
                {
                    state = State.Alerted;
                    if (profile) agent.speed = profile.chaseSpeed;
                    firstDetectionOccurred = true;
                    AlertManager.Instance?.BroadcastLKP(sensors.lastSeenPos, this);
                    if (debugLogs) Debug.Log($"{name} -> ALERTED");
                }

                lostSightTimer = 0f;
            }
            else
            {
                if (state == State.Alerted)
                {
                    lostSightTimer += Time.deltaTime;
                    if (lostSightTimer >= lostSightGrace)
                    {
                        BeginSearchAt(sensors.lastSeenPos);
                    }
                }
            }
        }

        // Run behaviour for current state
        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Alerted: TickAlerted(); break;
            case State.SearchLastSeen: TickSearch(); break;
        }

        // Smooth facing like the hero
        var v = agent.velocity;
        Vector2 dir = (v.sqrMagnitude > stopThreshold * stopThreshold)
            ? new Vector2(v.x, v.y).normalized
            : lastMoveDir;

        if (v.sqrMagnitude > stopThreshold * stopThreshold)
            lastMoveDir = dir;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var rot = Quaternion.Euler(0f, 0f, forwardIsUp ? (angle - 90f) : angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceLerp * Time.deltaTime);
    }

    // ===== States =====

    void TickPatrol()
    {
        // If we’ve ever detected the player once, do NOT auto-return to Patrol anymore.
        if (firstDetectionOccurred) { state = State.SearchLastSeen; return; }

        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (profile) agent.speed = profile.patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= patrolWaypointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            if (debugLogs) Debug.Log($"{name} Patrol → {patrolPoints[patrolIndex].name}");
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
        else if (!agent.hasPath)
        {
            if (debugLogs) Debug.Log($"{name} Patrol (recover) → {patrolPoints[patrolIndex].name}");
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void TickAlerted()
    {
        if (!sensors || !sensors.target) return;

        // While Alerted, always push toward the hero’s current position.
        agent.isStopped = false;
        Vector2 goal = sensors.target.position;
        agent.SetDestination(goal);
    }

    void BeginSearchAt(Vector2 lkp)
    {
        state = State.SearchLastSeen;
        if (profile) agent.speed = profile.patrolSpeed;
        searchCenter = lkp;
        dwellTimer = 0f;

        // Move to exact LKP first
        agent.isStopped = false;
        if (NavMesh.SamplePosition(lkp, out var hit, navSampleRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(lkp);

        if (debugLogs) Debug.Log($"{name} -> SEARCH at LKP");
    }

    void TickSearch()
    {
        // If player becomes visible again, go straight back to Alerted chase
        if (sensors && sensors.targetVisible)
        {
            state = State.Alerted;
            if (profile) agent.speed = profile.chaseSpeed;
            if (debugLogs) Debug.Log($"{name} SEARCH -> ALERTED (reacquired)");
            return;
        }

        // Wander around LKP a bit — but NEVER go back to patrol automatically.
        if (!agent.pathPending && agent.remainingDistance <= patrolWaypointTolerance)
        {
            dwellTimer += Time.deltaTime;
            if (dwellTimer >= searchDwellTime)
            {
                dwellTimer = 0f;
                Vector2 offset = Random.insideUnitCircle * (profile && profile.searchRadius > 0 ? profile.searchRadius : searchRadius);
                Vector2 desired = searchCenter + offset;
                if (NavMesh.SamplePosition(desired, out var hit, navSampleRadius, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                    agent.SetDestination(searchCenter);
            }
        }
    }

    // Call this from a Level/Encounter manager when you WANT them to resume patrol.
    public void ForceReturnToPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        state = State.Patrol;
        firstDetectionOccurred = false; // allow patrol again
        if (profile) agent.speed = profile.patrolSpeed;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        if (debugLogs) Debug.Log($"{name} FORCED -> PATROL");
    }

    public void BeginInvestigateExternal(Vector2 center)
    {
        if (state == State.Alerted && sensors && sensors.targetVisible)
            return;

        BeginSearchAt(center);
    }
}
