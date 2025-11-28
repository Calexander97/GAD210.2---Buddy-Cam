using System; // <-- add this at top
using UnityEngine;
using UnityEngine.Events;

public class HeroHealth : MonoBehaviour
{
    [Range(1, 10)] public int maxHearts = 3;
    public float iFrames = 0.3f;
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    public event Action OnDied;   // <-- add this

    int hearts;
    float invulnTimer;

    void Awake() => hearts = maxHearts;

    void Update()
    {
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
    }

    public void TakeHit(int amount = 1)
    {
        if (invulnTimer > 0f) return;

        hearts = Mathf.Max(0, hearts - amount);
        invulnTimer = iFrames;
        onDamaged?.Invoke();

        if (hearts == 0)
        {
            onDeath?.Invoke();
            OnDied?.Invoke();          // <-- invoke the C# event too
            gameObject.SetActive(false);
        }
    }

    public int Current => hearts;
}
