using UnityEngine;
using UnityEngine.AI;

/// Optional component that auto-emits “footstep” noise when moving with a NavMeshAgent.
/// You can also call Emit() manually for gadgets, doors, etc.
[DisallowMultipleComponent]
public class NoiseEmitter : MonoBehaviour
{
    [Header("Auto footsteps")]
    public bool emitOnMove = true;      // toggle footsteps
    public float stepLoudness = 1f;     // baseline loudness for a step
    public float stepInterval = 0.35f;  // seconds between steps
    public float minSpeedForSteps = 0.2f;

    NavMeshAgent agent;
    float stepTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent) { agent.updateRotation = false; agent.updateUpAxis = false; }
    }

    void Update()
    {
        if (!emitOnMove || !agent) return;

        // Simple cadence based on current speed
        if (agent.velocity.magnitude >= minSpeedForSteps)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                Emit(stepLoudness);
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    /// Manual trigger for one-shot sounds (throwables, doors, etc.)
    public void Emit(float loudness = 1f)
        => NoiseSystem.Emit(transform.position, loudness);
}
