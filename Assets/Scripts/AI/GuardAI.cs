using UnityEngine;
using UnityEngine.AI;

/// Simple state machine: Patrol → Investigate (search LKP) → Alerted (chase).
/// Uses NavMeshAgent configured for XY (updateUpAxis=false/updateRotation=false).
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
    public float dwellAtSpot = 1.2f;        // wait time at each search point

    public enum State { Patrol, Investigate, Alerted }
    public State state = State.Patrol;

    NavMeshAgent agent;
    int patrolIndex;
    float searchTimer;
    float totalSearchTime;
    Vector2 searchCenter;
    Vector2 currentSearchTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
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
        // Core behaviour per state
        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Investigate: TickInvestigate(); break;
            case State.Alerted: TickAlerted(); break;
        }

        // Pereception driven transitions
        if (sensors && profile)
        {
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
        }

        // --- States ---

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

            // Arrived at current search point → dwell briefly, then pick another
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                searchTimer += Time.deltaTime;
                if (searchTimer >= dwellAtSpot)
                {
                    PickNewSearchPoint();
                    searchTimer = 0f;
                }
            }

            // Give up after total search time
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

        // --- Helpers ---

        void PickNewSearchPoint()
        {
            if (!profile) return;

            Vector2 offset = Random.insideUnitCircle * profile.searchRadius;
            currentSearchTarget = searchCenter + offset;

            // Snap to mesh so the agent always gets a valid goal
            if (NavMesh.SamplePosition(currentSearchTarget, out var hit, 1.5f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            else
                agent.SetDestination(searchCenter);
        }
    }
}
