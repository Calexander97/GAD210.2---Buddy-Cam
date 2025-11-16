using UnityEngine;

/// Tunable settings for a guard: vision, hearing, speeds, and search behaviour
[CreateAssetMenu(fileName = "GuardProfile", menuName = "Stealth/Guard Profile")]
public class GuardProfile : ScriptableObject
{
    [Header("Vision")]
    public float visionRange = 12f;               // max sight distance
    [Range(10, 360)] public float fov = 120f;   // degrees of field-of-view cone
    public LayerMask losMask;                   // walls + player layers for LOS ranger
    public LayerMask targetMask;                // usually just Player

    [Header("Hearing")]
    public float hearingRadius = 6f;            // hearing distance

    [Header("Movement")]
    public float patrolSpeed = 2.2f;            // metres/sec while patrolling
    public float chaseSpeed =  3.2f;            // metres/sec whil

    [Header("Search")]
    public float searchRadius = 2.2f;            // random wander around LKP
    public float searchTime = 6f;                // total time to keep searching

    [Header("Debug")]
    public Color gizmoColor = new Color(1f, .4f, .4f, .25f);
    
}
