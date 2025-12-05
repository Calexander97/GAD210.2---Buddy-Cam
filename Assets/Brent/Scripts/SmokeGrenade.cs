using UnityEngine;

public class SmokeGrenade : MonoBehaviour
{
    [Header("Smoke Settings")]
    public float fuseTime = 2.0f;
    public GameObject smokeCloudPrefab;
    public float smokeDuration = 6f;

    [Header("Audio")]
    public AudioClip activateSFX;
    public float sfxLeadTime = 0.1f;

    private void Start()
    {
        // SFX slightly before the smoke appears
        if (activateSFX != null && SFXManager.Instance != null)
        {
            float sfxTime = Mathf.Max(0f, fuseTime - sfxLeadTime);
            Invoke(nameof(PlaySFX), sfxTime);
        }

        // actual smoke explosion
        Invoke(nameof(Detonate), fuseTime);
    }

    void PlaySFX()
    {
        SFXManager.Instance.PlaySmoke();
    }

    void Detonate()
    {
        if (smokeCloudPrefab != null)
        {
            GameObject smoke = Instantiate(smokeCloudPrefab, transform.position, Quaternion.identity);

            Destroy(smoke, smokeDuration);
        }

        Destroy(gameObject);
    }
}
