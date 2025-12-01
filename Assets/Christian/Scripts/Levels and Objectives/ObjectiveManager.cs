using UnityEngine;
using UnityEngine.Events;


/// Central objective state for the current run.
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager I { get; private set; }

    [Header("State")]
    [SerializeField] bool _hasData = false;
    [SerializeField] bool _altExitUnlocked = false;
    [SerializeField] bool _missionComplete = false;

    [Header("UI")]
    public ObjectiveBanner banner;          // drag the banner UI (TMP) for messages

    [Header("Events")]
    public UnityEvent onDataAcquired;
    public UnityEvent onAltExitUnlocked;
    public UnityEvent onMissionComplete;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    public void AcquireData()
    {
        if (_hasData) return;
        _hasData = true;
        onDataAcquired?.Invoke();
        banner?.Show("Objective updated: Data acquired");
        Debug.Log("[Objective] Data acquired.");
    }

    public void UnlockAltExit()
    {
        if (_altExitUnlocked) return;
        _altExitUnlocked = true;
        onAltExitUnlocked?.Invoke();
        banner?.Show("Alternate exit unlocked");
        Debug.Log("[Objective] Alternate exit unlocked.");
    }

    public void CompleteMission()
    {
        if (_missionComplete) return;
        _missionComplete = true;
        onMissionComplete ?.Invoke();
        banner?.Show("Mission Complete");
        Debug.Log("[Objective] Mission complete.");
    }


    public bool HasData => _hasData;
    public bool AltExitUnlocked => _altExitUnlocked;
    public bool MissionComplete => _missionComplete;

    // Soft reset between runs
    public void ResetRun()
    {
        _hasData = false;
        _altExitUnlocked = false;
        _missionComplete = false;
    }

}
