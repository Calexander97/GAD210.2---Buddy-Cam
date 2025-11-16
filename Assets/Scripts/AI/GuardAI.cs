using UnityEngine;
using UnityEngine.AI;

/// Simple state machine: Patrol → Investigate → Alerted (chase).
/// Works on a top-down (XY) NavMesh: updateUpAxis=false / updateRotation=false.
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    public GuardProfile profile;
    public GuardSensors sensors;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.15f;

    [Header("Search")]
    public float dwellAtSpot = 1.2f; // time to wait at each search point

    [Header("Facing")]
    public float faceLerp = 18f;        // how quickly to rotate
    public bool forwardIsUp = true;     // true if your sprite art faces ↑
    public float stopThreshold = 0.02f; // avoid jitter when almost stopped

    public enum State { Patrol, Investigate, Alerted }
    public State state = State.Patrol;

    NavMeshAgent agent;
    int patrolIndex;
    float searchTimer;
    float totalSearchTime;
    Vector2 searchCenter;
    Vector2 currentSearchTarget;
    Vector2 lastMoveDir = Vector2.right; // remembered heading for idle facing

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;    // 2D top-down plane
        agent.updateRotation = false;  // we handle rotation manually
        if (!sensors) sensors = GetComponent<GuardSensors>();
    }

    void Start()
    {
        if (profile) agent.speed = profile.patrolSpeed;

        // Kick off patrol if waypoints exist
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void Update()
    {
        // --- Core behaviour per state ---
        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Investigate: TickInvestigate(); break;
            case State.Alerted: TickAlerted(); break;
        }

        // --- Perception-driven transitions ---
        if (sensors && profile)
        {
            if (sensors.targetVisible)
            {
                state = State.Alerted;
                agent.speed = profile.chaseSpeed;
            }
            else if (state == State.Alerted && !sensors.targetVisible)
            {
                BeginInvestigate(sensors.lastSeenPos);
            }
            else if (state == State.Patrol && sensors.heardRecently)
            {
                BeginInvestigate(sensors.lastHeardPos);
            }
        }

        // --- Face movement direction (smooth, like the hero) ---
        Vector3 v3 = agent.velocity;
        Vector2 v = new Vector2(v3.x, v3.y);

        Vector2 dir;
        if (v.sqrMagnitude > stopThreshold * stopThreshold)
        {
            dir = v.normalized;
            lastMoveDir = dir; // remember last meaningful heading
        }
        else
        {
            dir = lastMoveDir;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var targetRot = Quaternion.Euler(0f, 0f, forwardIsUp ? (angle - 90f) : angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, faceLerp * Time.deltaTime);

        // ---------------- Local functions ----------------
        void TickPatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;

            agent.speed = profile ? profile.patrolSpeed : agent.speed;

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }

        void BeginInvestigate(Vector2 center)
        {
            state = State.Investigate;
            searchCenter = center;
            searchTimer = 0f;
            totalSearchTime = 0f;
            PickNewSearchPoint();
        }

        void TickInvestigate()
        {
            if (!profile) return;

            totalSearchTime += Time.deltaTime;

            // Arrived → dwell a moment, then choose another local point
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                searchTimer += Time.deltaTime;
                if (searchTimer >= dwellAtSpot)
                {
                    PickNewSearchPoint();
                    searchTimer = 0f;
                }
            }

            // Time-out → resume patrol
            if (totalSearchTime >= profile.searchTime)
            {
                state = State.Patrol;
                agent.speed = profile.patrolSpeed;
                if (patrolPoints != null && patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }

        void TickAlerted()
        {
            if (!sensors || !sensors.target) return;
            agent.SetDestination(sensors.target.position);
        }

        void PickNewSearchPoint()
        {
            if (!profile) return;

            Vector2 offset = Random.insideUnitCircle * profile.searchRadius;
            currentSearchTarget = searchCenter + offset;

            // Snap to mesh so the destination is always valid
            if (NavMesh.SamplePosition(currentSearchTarget, out var hit, 1.5f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(searchCenter);
        }
    }
}
