using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip puzzleComplete;
    public AudioClip buttonClick;
    public AudioClip errorBeep;
    public AudioClip itemThrow;
    public AudioClip pickupItem;
    public AudioClip smokeActivate;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }
}
