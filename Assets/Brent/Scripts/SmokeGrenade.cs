using UnityEngine;

public class SmokeGrenade : MonoBehaviour
{
    [Header("Smoke Settings")]
    public float fuseTime = 2.0f;
    public GameObject smokeCloudPrefab;
    public float smokeDuration = 6f;

    private void Start()
    {
        // Start the fuse timer
        Invoke(nameof(Detonate), fuseTime);
    }

    void Detonate()
    {
        if (smokeCloudPrefab != null)
        {
            GameObject smoke = Instantiate(smokeCloudPrefab, transform.position, Quaternion.identity);

            var cloud = smoke.GetComponent<SmokeCloud>();
            if (cloud != null)
                cloud.duration = smokeDuration;
        }

        Destroy(gameObject);
    }
}
