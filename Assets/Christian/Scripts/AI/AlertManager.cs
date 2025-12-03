using System.Collections.Generic;
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
            return _instance;
        }
    }

    [Tooltip("If enabled, logs registrations and broadcasts.")]
    public bool verboseLogging = true;

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

        List<string> picked = null;       // only allocate if we need to log
        if (verboseLogging) picked = new List<string>();

        for (int i = 0; i < _guards.Count; i++)
        {
            var g = _guards[i];
            if (!g || g == from) continue;
            if (Vector2.Distance(g.transform.position, lkp) > radius) continue;

            g.BeginInvestigateExternal(lkp);
            if (verboseLogging) picked.Add(g.name);
        }

        if (verboseLogging)
        {
            if (picked.Count > 0)
                Debug.Log($"[Radio] {from.name} broadcast LKP {lkp} → {string.Join(", ", picked)}");
            else
                Debug.Log($"[Radio] {from.name} broadcast LKP {lkp} → no receivers in range.");
        }
    }
}
