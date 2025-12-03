using System.Collections;
using UnityEngine;

public class Lure : MonoBehaviour
{
    public float loudness = 1.0f;
    public float pulseInterval = 1.0f;
    public bool active = true;

    void Start()
    {
        StartCoroutine(NoisePulseRoutine());
    }

    IEnumerator NoisePulseRoutine()
    {
        while (active)
        {
            NoiseSystem.Emit((Vector2)transform.position, loudness);

            yield return new WaitForSeconds(pulseInterval);
        }
    }

    public void TurnOff()
    {
        active = false;
        StopAllCoroutines();
        Destroy(gameObject, 0.2f);
    }
}