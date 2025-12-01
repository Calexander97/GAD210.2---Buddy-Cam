using System.Linq;
using UnityEngine;
using UnityEngine.AI;                 // NavMeshAgent + NavMesh.SamplePosition
using NavMeshPlus.Components;         // NavMeshSurface (NavMeshPlus)
using UnityEngine.UI;                 // Button
using TMPro;                          // TMP_Text

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class FloorSet
    {
        [Header("Grouping")]
        public GameObject root;            // e.g., Floor_01 / Floor_02 / Floor_03

        [Header("Navigation")]
        public NavMeshSurface surface;     // Pre-baked NavMeshPlus surface (enabled/disabled)

        [Header("Spawning")]
        public Transform spawn;            // Default spawn for this floor (optional)

        [Header("Optional Auto-Find (under root)")]
        public Camera[] cameras;           // If empty, auto-find under root
        public GuardAI[] guards;           // If empty, auto-find under root
    }

    [Header("Core")]
    public CameraSwitcher switcher;        // CCTV feed switcher
    public Transform hero;                 // Persistent hero Transform
    public NavMeshAgent heroAgent;         // Configured for XY (updateUpAxis=false)

    [Header("Floors (order = progression)")]
    public FloorSet[] floors;

    [Header("Boot Start")]
    [Tooltip("Which floor to load at startup.")]
    public int bootFloorIndex = 0;

    [Tooltip("Optional: where to place the hero at startup (used only once on boot).")]
    public Transform bootSpawnOverride;

    [Header("Options")]
    [Tooltip("Snap hero onto mesh within this radius when switching floors.")]
    public float warpSampleRadius = 2f;

    [Header("Camera UI (optional)")]
    public Button prevCameraButton;
    public Button nextCameraButton;
    public Button[] cameraIndexButtons;    // e.g., 4 buttons for Cam1..Cam4
    public TMP_Text[] cameraButtonTMP;     // labels for those buttons (optional)

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        // Hide all floors at boot; the active one is enabled on SwitchToFloor.
        if (floors == null) return;
        for (int i = 0; i < floors.Length; i++)
            if (floors[i] != null && floors[i].root != null)
                floors[i].root.SetActive(false);
    }

    void Start()
    {
        if (floors == null || floors.Length == 0) return;

        // Use the bootFloorIndex and (optional) bootSpawnOverride exactly once at startup.
        var idx = Mathf.Clamp(bootFloorIndex, 0, floors.Length - 1);
        SwitchToFloor(idx, bootSpawnOverride);

        ValidateGuardsUnderRoots(); // editor-time helper
    }

    public void NextFloor() => SwitchToFloor(Mathf.Clamp(GetCurrentIndex() + 1, 0, floors.Length - 1));
    public void PrevFloor() => SwitchToFloor(Mathf.Clamp(GetCurrentIndex() - 1, 0, floors.Length - 1));

    int GetCurrentIndex()
    {
        // Determine which root is currently active
        if (floors == null) return 0;
        for (int i = 0; i < floors.Length; i++)
            if (floors[i]?.root && floors[i].root.activeSelf) return i;
        return 0;
    }

    // ───────────────────────────── Switch (no explicit spawn) ────────────────────
    public void SwitchToFloor(int index) => SwitchToFloor(index, (Transform)null);

    // ───────────────────────────── Switch (with spawn Transform) ─────────────────
    public void SwitchToFloor(int index, Transform spawnOverride)
    {
        if (floors == null || index < 0 || index >= floors.Length) return;

        // Deactivate all floors first (keeps logic simple)
        for (int i = 0; i < floors.Length; i++)
        {
            var f = floors[i];
            if (f == null) continue;
            if (f.surface) f.surface.enabled = false;
            if (f.root) f.root.SetActive(false);
        }

        // Activate target floor
        var floor = floors[index];
        if (floor == null || !floor.root)
        {
            Debug.LogWarning("LevelManager: Floor root missing.");
            return;
        }

        floor.root.SetActive(true);

        // Ensure refs (auto-find where needed under the floor root)
        var surface = ResolveSurface(floor);
        var cams = (floor.cameras != null && floor.cameras.Length > 0)
                    ? floor.cameras
                    : floor.root.GetComponentsInChildren<Camera>(true);

        var guards = (floor.guards != null && floor.guards.Length > 0)
                    ? floor.guards
                    : floor.root.GetComponentsInChildren<GuardAI>(true);

        var spawn = spawnOverride ? spawnOverride : floor.spawn; // bootSpawnOverride is only passed in from Start()

        // Enable surface (adds pre-baked data)
        if (surface) surface.enabled = true;

        // Place hero (snap to mesh if possible)
        if (hero && heroAgent)
        {
            heroAgent.enabled = false;

            Vector3 dest = spawn ? spawn.position : hero.position;
            if (NavMesh.SamplePosition(dest, out var hit, warpSampleRadius, NavMesh.AllAreas))
                dest = hit.position;

            hero.position = dest;
            heroAgent.Warp(dest);
            heroAgent.enabled = true;
        }

        // Feed cameras into the single feed switcher
        if (switcher != null && cams != null && cams.Length > 0)
        {
            switcher.cams = cams;
            switcher.SetActiveCamera(0, true);
            WireCameraButtons(cams); // update UI hooks if assigned
        }

        // Guards: nothing to do here—since they live under the floor root, toggling the root handles them.
    }

    // ───────────────────────────────────── Helpers ───────────────────────────────

    NavMeshSurface ResolveSurface(FloorSet f)
    {
        if (f.surface) return f.surface;
        var s = f.root ? f.root.GetComponentInChildren<NavMeshSurface>(true) : null;
        f.surface = s;
        return s;
    }

    void WireCameraButtons(Camera[] cams)
    {
        if (switcher == null) return;

        bool hasMultiple = cams != null && cams.Length > 1;

        // Prev / Next
        if (prevCameraButton)
        {
            prevCameraButton.onClick.RemoveAllListeners();
            prevCameraButton.onClick.AddListener(() => switcher.PrevCamera());
            prevCameraButton.gameObject.SetActive(hasMultiple);
        }

        if (nextCameraButton)
        {
            nextCameraButton.onClick.RemoveAllListeners();
            nextCameraButton.onClick.AddListener(() => switcher.NextCamera());
            nextCameraButton.gameObject.SetActive(hasMultiple);
        }

        // Direct index buttons
        if (cameraIndexButtons == null) return;

        for (int i = 0; i < cameraIndexButtons.Length; i++)
        {
            var btn = cameraIndexButtons[i];
            if (!btn) continue;

            bool show = (cams != null && i < cams.Length && cams[i] != null);
            btn.gameObject.SetActive(show);
            btn.onClick.RemoveAllListeners();

            if (show)
            {
                int idx = i; // capture
                btn.onClick.AddListener(() => switcher.SetActiveCamera(idx));

                // Optional TMP label
                if (cameraButtonTMP != null && i < cameraButtonTMP.Length && cameraButtonTMP[i])
                {
                    cameraButtonTMP[i].text = string.IsNullOrWhiteSpace(cams[i].name)
                        ? $"Cam {i + 1}"
                        : cams[i].name;
                }
            }
            else
            {
                if (cameraButtonTMP != null && i < cameraButtonTMP.Length && cameraButtonTMP[i])
                    cameraButtonTMP[i].text = string.Empty;
            }
        }
    }

    // Editor helper: warn if any GuardAI isn’t under a floor root.
    void ValidateGuardsUnderRoots()
    {
#if UNITY_EDITOR
        if (floors == null || floors.Length == 0) return;

        var floorRoots = floors.Where(f => f != null && f.root != null)
                               .Select(f => f.root.transform)
                               .ToArray();

        var allGuards = FindObjectsOfType<GuardAI>(true);
        foreach (var g in allGuards)
        {
            if (!g) continue;
            Transform t = g.transform;
            bool underAnyRoot = false;
            while (t != null)
            {
                if (floorRoots.Contains(t)) { underAnyRoot = true; break; }
                t = t.parent;
            }
            if (!underAnyRoot)
            {
                Debug.LogWarning($"[LevelManager] Guard '{g.name}' is not under any Floor root. " +
                                 "Move it under the correct Floor_X root so it toggles with floors.",
                                 g);
            }
        }
#endif
    }
}
