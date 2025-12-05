using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AlarmBox))]
public class TripLaser : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Object with this tag will trigger the laser.")]
    public string playerTag = "Player";
    [Tooltip("Laser starts armed; when disarmed (by hack) it stays off permanently.")]
    public bool armed = true;

    [Header("Alarm Behaviour")]
    [Tooltip("Seconds the alarm stays active before auto-clearing itself.")]
    public float alarmDuration = 6f;
    [Tooltip("If true, the laser rearms after auto-clear (unless fully disarmed by hack).")]
    public bool autoRearm = true;
    [Tooltip("Delay (after alarm clears) before the laser rearms.")]
    public float rearmDelay = 1.0f;

    [Header("Beam Visual")]
    [Tooltip("The single SpriteRenderer to show/hide for the laser beam.")]
    public SpriteRenderer beamSprite;

    [Header("SFX (optional)")]
    public AudioSource audioSource;
    public AudioClip tripSfx;
    [Range(0f, 1f)] public float tripSfxVolume = 1f;
    public AudioClip disarmSfx;
    [Range(0f, 1f)] public float disarmSfxVolume = 1f;

    [Header("Events")]
    public UnityEvent onTripped;
    public UnityEvent onDisarmed;

    AlarmBox alarm;
    Collider2D tripCollider;
    Coroutine alarmRoutine;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void Awake()
    {
        alarm = GetComponent<AlarmBox>();
        tripCollider = GetComponent<Collider2D>();

        // Ensure AlarmBox won't outlive intended duration
        if (alarm && (alarm.autoSilenceAfter <= 0f || alarm.autoSilenceAfter > alarmDuration))
            alarm.autoSilenceAfter = alarmDuration;

        ApplyVisuals();
    }

    void OnEnable() => ApplyVisuals();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;

        if (IsPlayer(other.gameObject))
            Trip();
    }

    bool IsPlayer(GameObject obj)
    {
        if (obj.CompareTag(playerTag)) return true;

        var im = FindFirstObjectByType<InteractionManager>();
        return (im && im.player && obj == im.player);
    }

    void Trip()
    {
        if (!armed) return;

        if (audioSource && tripSfx) audioSource.PlayOneShot(tripSfx, tripSfxVolume);

        if (alarm && !alarm.isActive)
            alarm.TriggerAlarm();

        onTripped?.Invoke();

        // Hide the beam immediately while the alarm is active
        ApplyVisuals();

        if (alarmRoutine != null) StopCoroutine(alarmRoutine);
        alarmRoutine = StartCoroutine(AutoClearThenMaybeRearm());
    }

    IEnumerator AutoClearThenMaybeRearm()
    {
        float t = Mathf.Max(0.05f, alarmDuration);
        while (t > 0f && alarm && alarm.isActive) { t -= Time.deltaTime; yield return null; }

        if (alarm && alarm.isActive) alarm.ClearAlarm();

        if (autoRearm && armed) // still armed (i.e., not hacked)
        {
            if (rearmDelay > 0f) yield return new WaitForSeconds(rearmDelay);
            SetArmed(true);       // this will re-enable the beam
        }
        else
        {
            // Just refresh visuals with current state
            ApplyVisuals();
        }

        alarmRoutine = null;
    }

    public void SetArmed(bool value)
    {
        armed = value;
        if (tripCollider) tripCollider.enabled = value;
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        // Rules:
        // - If disarmed (hacked): beam OFF.
        // - Else if an alarm is currently active: beam OFF.
        // - Else (armed & calm): beam ON.
        bool showBeam =
            armed &&
            !(alarm != null && alarm.isActive);

        if (beamSprite) beamSprite.enabled = showBeam;
    }

    // Called by the interactable (after puzzle success) to permanently disable the beam.
    public void DisarmPermanently()
    {
        if (alarm && alarm.isActive) alarm.ClearAlarm();

        SetArmed(false); // disables collider and turns beam OFF

        if (audioSource && disarmSfx) audioSource.PlayOneShot(disarmSfx, disarmSfxVolume);
        onDisarmed?.Invoke();
    }
}
