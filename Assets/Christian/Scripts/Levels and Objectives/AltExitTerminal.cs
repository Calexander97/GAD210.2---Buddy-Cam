using UnityEngine;

/// Put this on the Alt Exit terminal. Interacting unlocks the alternate exit
/// and pops a short message. That's it.
public class AltExitTerminal : Interactable
{
    [Header("Feedback")]
    [Tooltip("Short message shown when the exit is opened.")]
    public string openedMessage = "Alternate exit opened";
    public AudioClip openedSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    AudioSource _audio;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (!_audio && openedSfx)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f; // 3D by default
        }
    }

    public override void Interact(GameObject player)
    {
        // Unlock the alt exit
        ObjectiveManager.I?.UnlockAltExit();

        // Message (uses ObjectiveManager's banner if assigned)
        if (ObjectiveManager.I && ObjectiveManager.I.banner)
            ObjectiveManager.I.banner.Show(openedMessage);
        else
            Debug.Log($"[AltExit] {openedMessage}");

        // Optional SFX
        if (_audio && openedSfx) _audio.PlayOneShot(openedSfx, sfxVolume);
    }
}
