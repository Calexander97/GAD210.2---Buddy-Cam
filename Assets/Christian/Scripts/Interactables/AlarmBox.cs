using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class AlarmBox : MonoBehaviour
{
    [Header("Alarm")]
    public bool isActive = false;
    public float radioRadius = 20f;
    public float claimRangeBonus = 2f; // reserved if you later bias primary selection

    [Header("Visuals")]
    public GameObject alarmOnVisual;   // blinking sprite/light (optional)
    public GameObject alarmOffVisual;  // idle sprite/light (optional)

    [Header("Events")]
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true; // player can step into area
    }

    /// Called by the Interactable wrapper when the player presses the interact button.
    public void TriggerAlarm(GameObject actor = null)
    {
        if (isActive) return;
        isActive = true;
        SetVisuals();
        onActivated?.Invoke();

        AlertManager.Instance?.BroadcastAlarm(this);
    }

    /// Called by the primary guard when they reach the box.
    public void ClearAlarm()
    {
        if (!isActive) return;
        isActive = false;
        SetVisuals();
        onDeactivated?.Invoke();
    }

    void SetVisuals()
    {
        if (alarmOnVisual) alarmOnVisual.SetActive(isActive);
        if (alarmOffVisual) alarmOffVisual.SetActive(!isActive);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, radioRadius);
    }
}
