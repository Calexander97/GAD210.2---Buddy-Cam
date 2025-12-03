using UnityEngine;

public class LureDisableTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GuardAI guard = other.GetComponent<GuardAI>();
        if (guard != null)
        {
            GetComponent<Lure>().TurnOff();
        }
    }
}
