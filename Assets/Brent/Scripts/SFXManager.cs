using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public AudioSource sfxSource;

    [Header("Global Volume")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Header("Clips + Volumes")]
    public AudioClip puzzleComplete;
    [Range(0f, 1f)] public float puzzleVolume = 1f;

    public AudioClip buttonClick;
    [Range(0f, 1f)] public float buttonVolume = 1f;

    public AudioClip itemThrow;
    [Range(0f, 1f)] public float throwVolume = 1f;

    public AudioClip pickupItem;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    public AudioClip smokeActivate;
    [Range(0f, 1f)] public float smokeVolume = 1f;

    public AudioClip lureActivate;
    [Range(0f, 1f)] public float lureVolume = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Play(AudioClip clip, float categoryVolume)
    {
        if (clip == null || sfxSource == null) return;

        float finalVolume = masterVolume * categoryVolume;
        sfxSource.PlayOneShot(clip, finalVolume);
    }


    public void PlayPuzzleComplete() => Play(puzzleComplete, puzzleVolume);
    public void PlayButtonClick() => Play(buttonClick, buttonVolume);
    public void PlayThrow() => Play(itemThrow, throwVolume);
    public void PlayPickup() => Play(pickupItem, pickupVolume);
    public void PlaySmoke() => Play(smokeActivate, smokeVolume);
    public void PlayLure() => Play(lureActivate, lureVolume);
}