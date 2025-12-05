using UnityEngine;

/// One-shot lure that emits a single noise ping when it lands (collision or trigger).
/// Also supports a short fallback delay in case no physics event fires.
[DisallowMultipleComponent]
public class LurePing : MonoBehaviour
{
    [Header("Noise")]
    [Tooltip("NoiseSystem loudness passed to nearby guards. 1 = baseNoiseRadius in AlertManager.")]
    public float loudness = 1.0f;

    [Tooltip("Emit even if no collision happens in this many seconds.")]
    public float fallbackPingDelay = 0.12f;

    [Header("SFX")]
    public AudioClip landSfx;
    [Range(0f, 1f)] public float landSfxVolume = 1f;
    [Tooltip("If true, we auto-add an AudioSource if none exists.")]
    public bool autoAddAudioSource = true;

    [Header("Cleanup")]
    [Tooltip("Delay before destroying the lure after the ping.")]
    public float destroyAfter = 0.2f;

    bool _pinged;
    AudioSource _audio;

    void Awake()
    {
        // Prepare audio
        _audio = GetComponent<AudioSource>();
        if (!_audio && autoAddAudioSource && landSfx)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f; // 3D
        }
    }

    void Start()
    {
        // Ensure we still ping even if no physics event fires
        Invoke(nameof(FallbackPing), Mathf.Max(0f, fallbackPingDelay));
    }

    void OnTriggerEnter2D(Collider2D _) => PingOnce();
    void OnCollisionEnter2D(Collision2D _) => PingOnce(); // optional but handy

    void FallbackPing() => PingOnce();

    void PingOnce()
    {
        if (_pinged) return;
        _pinged = true;

        // Broadcast the sound (guards will InvestigateNoise once, then resume patrol)
        NoiseSystem.Emit((Vector2)transform.position, loudness);

        // Play landing SFX (optional)
        if (_audio && landSfx) _audio.PlayOneShot(landSfx, landSfxVolume);

        // Disable physics so it doesn't retrigger
        var rb2d = GetComponent<Rigidbody2D>(); if (rb2d) rb2d.simulated = false;
        var col2d = GetComponent<Collider2D>(); if (col2d) col2d.enabled = false;

        // Clean up
        Destroy(gameObject, Mathf.Max(0f, destroyAfter));
    }
}
