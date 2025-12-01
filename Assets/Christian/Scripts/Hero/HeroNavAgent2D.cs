using UnityEngine;
using UnityEngine.AI;

/// Drives a 3D NavMeshAgent in a top-down (XY) 2D scene.
public class HeroNavAgent2D : MonoBehaviour
{
    public NavMeshAgent agent;
    public float sampleRadius = 2f;     // max distance to snap clicks/start onto the mesh
    public float interactRange = 0.2f;  // how close we must be to trigger IInteractable
    public float faceLerp = 18f;        // turn speed for facing
    public bool forwardIsUp = true;     // set true if sprite art faces ↑ by default
    public float stopThreshold = 0.02f; // ignore tiny velocities to avoid jitter

    private Vector2 lastMoveDir = Vector2.right; // remembered facing when we stop
    IInteractable pending;                       // pending interaction after move

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        // Make the agent 2D-friendly
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.baseOffset = 0f;

        // Don’t let it run until warped onto the mes
        agent.enabled = false;
    }

    void Start()
    {
        // Snap start position to the nearest point on the baked mesh.
        if (NavMesh.SamplePosition(transform.position, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            agent.enabled = true;          // must enable before Warp/SetDestination
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning("HeroNavAgent2D: No NavMesh near start position.");
        }
    }

    void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Fire pending interaction once arrived
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (pending != null &&
                Vector2.Distance(transform.position, pending.GetInteractPoint()) <= interactRange)
            {
                if (pending.CanInteract(transform)) pending.Interact(transform);
                pending = null;
            }
        }

        // Face movement direction (smoothed, with idle fallback)
        var v = agent.velocity;
        Vector2 dir;
        if (v.sqrMagnitude > stopThreshold * stopThreshold)
        {
            dir = v.normalized;
            lastMoveDir = dir;
        }
        else
        {
            dir = lastMoveDir;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var target = Quaternion.Euler(0f, 0f, forwardIsUp ? (angle - 90f) : angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, target, faceLerp * Time.deltaTime);
    }

    /// Move to a world point (clicks get snapped to the mesh).
    public void MoveTo(Vector2 world, IInteractable then = null)
    {
        pending = then;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(world, out var hit, sampleRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            Debug.Log("MoveTo: click not near NavMesh.");
    }

    /// Move to an interactable and trigger it on arrival.
    public void MoveToInteract(IInteractable target)
    {
        pending = target;
        MoveTo(target.GetInteractPoint(), target);
    }
}
