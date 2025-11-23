using System.Collections.Generic;
using UnityEngine;

/// Simple global relay: a guard that spots the hero can broadcast an LKP.
/// Nearby guards will move to investigate.
public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [Tooltip("How far a radio call can reach (world units).")]
    public float radioRange = 18f;

    readonly List<GuardAI> guards = new();

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(GuardAI g)
    {
        if (g && !guards.Contains(g)) guards.Add(g);
    }

    public void Unregister(GuardAI g)
    {
        guards.Remove(g);
    }

    /// Called by a spotting guard. Everyone else in range goes to investigate.
    public void BroadcastLKP(Vector2 lkp, GuardAI from)
    {
        for (int i = 0; i < guards.Count; i++)
        {
            var g = guards[i];
            if (!g || g == from) continue;
            if ((Vector2)g.transform.position == lkp) continue;

            if (Vector2.Distance(g.transform.position, lkp) <= radioRange)
                g.BeginInvestigateExternal(lkp);
        }
    }
}