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

    [Header("SFX (optional)")]
    [Tooltip("If assigned, this clip will play for each emit/footstep.")]
    public AudioClip sfxClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Tooltip("Random pitch range per emit for variation.")]
    public Vector2 sfxPitchRange = new Vector2(0.95f, 1.05f);
    [Tooltip("If true, volume scales with loudness passed to Emit().")]
    public bool scaleVolumeByLoudness = true;

    NavMeshAgent agent;
    float stepTimer;
    AudioSource _audio;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent) { agent.updateRotation = false; agent.updateUpAxis = false; }
        _audio = GetComponent<AudioSource>();
        if (!_audio && sfxClip)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f; // 3D by default; tweak per prefab if needed
        }
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
    {
        NoiseSystem.Emit(transform.position, loudness);
        PlaySFX(loudness);
    }

    void PlaySFX(float loudness)
    {
        if (!_audio || !sfxClip) return;

        // Randomise pitch
        float pMin = Mathf.Min(sfxPitchRange.x, sfxPitchRange.y);
        float pMax = Mathf.Max(sfxPitchRange.x,sfxPitchRange.y);
        float pitch = Random.Range(pMin, pMax);

        _audio.pitch = pitch;

        float vol = sfxVolume * (scaleVolumeByLoudness ? Mathf.Clamp01(loudness) : 1f);
        _audio.PlayOneShot(sfxClip, vol);
    }
}
