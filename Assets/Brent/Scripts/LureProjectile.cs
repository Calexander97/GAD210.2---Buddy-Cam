using UnityEngine;

public class LureProjectile : MonoBehaviour
{
    [Header("Throw Settings")]
    public float speed = 6f;
    public float arcHeight = 1.8f;

    private Vector2 start;
    private Vector2 target;
    private float t = 0f;
    private GameObject finalPrefab;

    public void Init(Vector2 targetPos, GameObject placedPrefab)
    {
        target = targetPos;
        finalPrefab = placedPrefab;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(SFXManager.Instance.itemThrow);
    }

    void Start()
    {
        start = transform.position;
    }

    void Update()
    {
        t += Time.deltaTime * speed;
        float progress = Mathf.Clamp01(t);

        // linear movement
        Vector2 linear = Vector2.Lerp(start, target, progress);

        // vertical arc
        float height = Mathf.Sin(progress * Mathf.PI) * arcHeight;

        transform.position = new Vector3(linear.x, linear.y + height, 0);

        if (progress >= 1f)
        {
            if (finalPrefab != null)
                Instantiate(finalPrefab, target, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}