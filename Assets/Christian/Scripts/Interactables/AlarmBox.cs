using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class AlarmBox : MonoBehaviour
{
    [Header("Alarm")]
    public bool isActive = false;
    public float radioRadius = 20f;
    public float claimRangeBonus = 2f; // reserved if you later bias primary selection

    [Header("Visuals")]
    public GameObject alarmOnVisual;   // blinking sprite/light (optional)
    public GameObject alarmOffVisual;  // idle sprite/light (optional)

    [Header("Audio (optional)")]
    public AudioSource audioSource;      // assign on the box (2D/3D as you prefer)
    public AudioClip activateSFX;        // one-shot when turned on
    public AudioClip deactivateSFX;      // one-shot when turned off
    public AudioClip sirenLoop;          // looping while active
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float sirenVolume = 1f;

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true; // player can step into area
        // Auto add AudioSource if none
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// Called by the Interactable wrapper when the player presses the interact button.
    public void TriggerAlarm(GameObject actor = null)
    {
        if (isActive) return;
        isActive = true;
        SetVisuals();

        // SFX: one-shot + start siren loop
        if (audioSource)
        {
            if (activateSFX) audioSource.PlayOneShot(activateSFX, sfxVolume);
            if (sirenLoop)
            {
                audioSource.clip = sirenLoop;
                audioSource.loop = true;
                audioSource.volume = sirenVolume;
                audioSource.Play();
            }
        }
        onActivated?.Invoke();
        AlertManager.Instance?.BroadcastAlarm(this);
    }

    /// Called by the primary guard when they reach the box.
    public void ClearAlarm()
    {
        if (!isActive) return;
        isActive = false;
        SetVisuals();

        // Stop siren, play shutdown SFX
        if (audioSource)
        {
            if (audioSource.isPlaying && audioSource.clip == sirenLoop)
                audioSource.Stop();
            if (deactivateSFX) audioSource.PlayOneShot(deactivateSFX, sfxVolume);
        }

        onDeactivated?.Invoke();
    }

    void SetVisuals()
    {
        if (alarmOnVisual) alarmOnVisual.SetActive(isActive);
        if (alarmOffVisual) alarmOffVisual.SetActive(!isActive);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radioRadius);
    }
}
