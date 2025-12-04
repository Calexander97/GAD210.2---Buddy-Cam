using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class AlertManager : MonoBehaviour
{
    private static AlertManager _instance;
    public static AlertManager Instance
    {
        get
        {
            if (_instance) return _instance;
            _instance = FindObjectOfType<AlertManager>();
            if (_instance) return _instance;
            var go = new GameObject("AlertManager");
            _instance = go.AddComponent<AlertManager>();
            DontDestroyOnLoad(go);
            if (_instance.verboseLogging) Debug.Log("[Radio] Spawned AlertManager at runtime.");
            return _instance;
        }
    }

    [Tooltip("Log registrations and broadcasts to the console.")]
    public bool verboseLogging = true;

    [Tooltip("If On: guards that were radioed can also relay later LKP calls. If Off: only the original spotter may radio.")]
    public bool allowRadioRelay = false;

    private readonly List<GuardAI> _guards = new();

    void Awake()
    {
        if (_instance && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(GuardAI g)
    {
        if (!g || _guards.Contains(g)) return;
        _guards.Add(g);
        if (verboseLogging) Debug.Log($"[Radio] Registered guard: {g.name}");
    }

    public void Unregister(GuardAI g)
    {
        if (!g) return;
        _guards.Remove(g);
        if (verboseLogging) Debug.Log($"[Radio] Unregistered guard: {g.name}");
    }

    /// Broadcast an LKP to other guards within radius (excludes the sender).
    public void BroadcastLKP(Vector2 lkp, GuardAI from, float radius)
    {
        if (!from) return;

        var picked = new List<string>();

        for (int i = 0; i < _guards.Count; i++)
        {
            var g = _guards[i];
            if (!g || g == from) continue;

            if (Vector2.Distance(g.transform.position, lkp) <= radius)
            {
                // Relay eligibility depending on toggle.
                g.BeginInvestigateExternal(lkp, relayEligible: allowRadioRelay);
                picked.Add(g.name);
            }
        }

        if (verboseLogging)
        {
            Debug.Log(picked.Count > 0
                ? $"[Radio] {from.name} LKP {lkp} → {string.Join(", ", picked)}"
                : $"[Radio] {from.name} LKP {lkp} → no receivers.");
        }
    }

    /// Alarm broadcast: pick a primary (nearest), everyone else also moves to the alarm LKP.
    public void BroadcastAlarm(AlarmBox box)
    {
        if (!box) return;

        // Collect eligible guards in radius.
        var inRange = _guards
            .Where(g => g && g.isActiveAndEnabled
                && Vector2.Distance(g.transform.position, box.transform.position) <= box.radioRadius)
            .ToList();

        if (inRange.Count == 0)
        {
            if (verboseLogging) Debug.Log("[Alarm] No guards in range of alarm.");
            return;
        }

        // Choose primary (nearest) to clear the alarm.
        var primary = inRange
            .OrderBy(g => Vector2.Distance(g.transform.position, box.transform.position))
            .FirstOrDefault();

        foreach (var g in inRange)
        {
            bool isPrimary = (g == primary);
            // Alarm responses should not relay radios unless you change this later.
            g.RespondToAlarm(box, isPrimary, allowRelay: false);
        }

        if (verboseLogging)
        {
            var names = string.Join(", ", inRange.Select(x => x.name));
            Debug.Log($"[Alarm] Fired at {box.name}. Primary: {primary?.name ?? "None"} | Notified: {names}");
        }
    }
}
