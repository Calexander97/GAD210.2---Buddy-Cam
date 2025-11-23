using UnityEngine;

/// Kinematic bullet that damages HeroHealth on contact.
public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float life = 3f;
    public int damage = 1;
    public LayerMask hitMask;         // include "Hero" layer

    Vector2 dir;

    public void Launch(Vector2 direction) { dir = direction.normalized; }

    void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (((1 << c.gameObject.layer) & hitMask) == 0) return;

        if (c.TryGetComponent<HeroHealth>(out var hp))
            hp.TakeHit(damage);

        Destroy(gameObject);
    }
}
