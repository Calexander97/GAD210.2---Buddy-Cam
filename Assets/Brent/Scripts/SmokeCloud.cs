using UnityEngine;

public class SmokeCloud : MonoBehaviour
{
    public float duration = 6f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }
}
