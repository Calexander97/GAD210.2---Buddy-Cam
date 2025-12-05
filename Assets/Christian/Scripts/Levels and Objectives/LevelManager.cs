using UnityEngine;
using UnityEngine.AI;                 // NavMeshAgent, NavMesh.SamplePosition
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
        public NavMeshSurface surface;     // Pre-baked NavMeshPlus surface (runtime add/remove)

        [Header("Spawning")]
        public Transform defaultSpawn;     // Default spawn for this floor (optional)

        [Header("Optional Auto-Find")]
        public Camera[] cameras;           // If empty, auto-find under root
        public GuardAI[] guards;           // If empty, auto-find under root
    }

    [Header("Core")]
    public CameraSwitcher switcher;        // CCTV feed switcher
    public Transform hero;                 // Persistent hero Transform
    public NavMeshAgent heroAgent;         // Configured for XY (updateUpAxis=false)

    [Header("Floors (order = progression)")]
    public FloorSet[] floors;

    [Header("Boot Options")]
    [Tooltip("If set, hero will be warped here once on Start, before first floor switch.")]
    public Transform bootStartPoint;

    [Header("Switching")]
    [Tooltip("If < 0, first floor is selected at Start.")]
    public int currentIndex = -1;
    [Tooltip("Snap hero onto mesh within this radius when switching floors.")]
    public float warpSampleRadius = 2f;
    [Tooltip("If true, only guards on the active floor are enabled.")]
    public bool autoEnableOnlyCurrentFloorGuards = true;

    [Header("Camera UI (optional)")]
    [Tooltip("Prev/Next camera buttons (optional).")]
    public Button prevCameraButton;
    public Button nextCameraButton;
    [Tooltip("Direct index buttons (e.g., 4 buttons for Cam1..Cam4).")]
    public Button[] cameraIndexButtons;
    [Tooltip("Optional labels for the index buttons (one per button).")]
    public TMP_Text[] cameraButtonTMP;

    // ─── Runtime tracking of currently loaded navmesh surface instance ───
    NavMeshSurface _activeSurface;

    void Awake()
    {
        // Hide all floors at boot; the active one is enabled on SwitchToFloor.
        if (floors == null) return;
        for (int i = 0; i < floors.Length; i++)
        {
            if (floors[i] != null && floors[i].root != null)
                floors[i].root.SetActive(false);
        }
    }

    void OnEnable()
    {
        // If menus/UI disabled components and dropped the navmesh handle,
        // re-ensure the current floor’s surface is loaded.
        EnsureActiveSurfaceLoaded();
    }

    void Start()
    {
        if (!hero || !heroAgent) return;
        heroAgent.updateUpAxis = false;
        heroAgent.updateRotation = false;

        // Optional one-time boot placement before switching floors
        if (bootStartPoint)
        {
            heroAgent.enabled = false;
            var dest = bootStartPoint.position;
            if (NavMesh.SamplePosition(dest, out var bootHit, warpSampleRadius, NavMesh.AllAreas))
                dest = bootHit.position;
            hero.position = dest;
            heroAgent.Warp(dest);
            heroAgent.enabled = true;
        }

        if (floors == null || floors.Length == 0) return;

        if (currentIndex < 0) SwitchToFloor(0); // default to first
        else SwitchToFloor(Mathf.Clamp(currentIndex, 0, floors.Length - 1));
    }

    public void NextFloor() => SwitchToFloor(Mathf.Clamp(currentIndex + 1, 0, floors.Length - 1));
    public void PrevFloor() => SwitchToFloor(Mathf.Clamp(currentIndex - 1, 0, floors.Length - 1));

    // ────────── Switch (no explicit spawn) ──────────
    public void SwitchToFloor(int index) => SwitchToFloor(index, (Transform)null);

    // ────────── Switch (with explicit spawn Transform) ──────────
    public void SwitchToFloor(int index, Transform spawnOverride)
    {
        if (floors == null || index < 0 || index >= floors.Length) return;

        // Unload previous floor’s navmesh data, disable its root
        if (currentIndex >= 0 && currentIndex < floors.Length)
        {
            var old = floors[currentIndex];
            if (old != null)
            {
                if (old.surface) UnloadSurface(old.surface);
                if (old.root) old.root.SetActive(false);
            }
        }

        // Activate new floor
        var f = floors[index];
        if (f == null || !f.root)
        {
            Debug.LogWarning("LevelManager: Floor root missing.");
            return;
        }

        f.root.SetActive(true);
        currentIndex = index;

        // Ensure refs (auto-find where needed)
        var surface = ResolveSurface(f);
        var cams = (f.cameras != null && f.cameras.Length > 0)
                    ? f.cameras
                    : f.root.GetComponentsInChildren<Camera>(true);

        var guards = (f.guards != null && f.guards.Length > 0)
                    ? f.guards
                    : f.root.GetComponentsInChildren<GuardAI>(true);

        var spawn = spawnOverride ? spawnOverride : f.defaultSpawn;

        // Load only this floor’s baked data (no global clears)
        if (surface) LoadSurface(surface);

        // Place hero and re-warp on the (re)loaded mesh
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

        // Feed cameras to single feed switcher and retag active
        if (switcher != null && cams != null && cams.Length > 0)
        {
            switcher.cams = cams;
            switcher.SetActiveCamera(0, true);
            RetagActiveCamera(cams, 0);
            WireCameraButtons(cams);
        }

        // Enable only this floor’s guards (optional)
        if (autoEnableOnlyCurrentFloorGuards)
        {
            // Disable all guards first
            for (int i = 0; i < floors.Length; i++)
            {
                if (!floors[i]?.root) continue;
                foreach (var g in floors[i].root.GetComponentsInChildren<GuardAI>(true))
                    if (g) g.gameObject.SetActive(false);
            }

            // Enable current floor guards
            if (guards != null)
                foreach (var g in guards)
                    if (g) g.gameObject.SetActive(true);
        }
    }

    // ─────────────────────────── Helpers ───────────────────────────

    NavMeshSurface ResolveSurface(FloorSet f)
    {
        if (f.surface) return f.surface;
        var s = f.root ? f.root.GetComponentInChildren<NavMeshSurface>(true) : null;
        f.surface = s;
        return s;
    }

    void LoadSurface(NavMeshSurface s)
    {
        if (!s) return;

        try
        {
            s.RemoveData();   // safe if none yet
            s.AddData();      // uses pre-baked data
        }
        catch
        {
            // Fallback path if runtime AddData/RemoveData isn’t available
            s.enabled = false;
            s.enabled = true;
            s.BuildNavMesh();
        }

        _activeSurface = s;
    }

    void UnloadSurface(NavMeshSurface s)
    {
        if (!s) return;
        try
        {
            s.RemoveData();
        }
        catch
        {
            s.enabled = false;
        }

        if (_activeSurface == s) _activeSurface = null;
    }

    void EnsureActiveSurfaceLoaded()
    {
        if (_activeSurface == null) return;

        try
        {
            _activeSurface.RemoveData();
            _activeSurface.AddData();
        }
        catch
        {
            _activeSurface.enabled = false;
            _activeSurface.enabled = true;
            _activeSurface.BuildNavMesh();
        }
    }

    void RetagActiveCamera(Camera[] cams, int activeIndex)
    {
        // Keep only the active cam tagged as MainCamera to avoid input/raycast ambiguity
        for (int i = 0; i < cams.Length; i++)
        {
            if (!cams[i]) continue;
            cams[i].tag = (i == activeIndex) ? "MainCamera" : "Untagged";
        }
    }

    void WireCameraButtons(Camera[] cams)
    {
        if (switcher == null) return;

        bool hasMultiple = cams != null && cams.Length > 1;

        if (prevCameraButton)
        {
            prevCameraButton.onClick.RemoveAllListeners();
            prevCameraButton.onClick.AddListener(() =>
            {
                switcher.PrevCamera();
                RetagActiveCamera(cams, switcher.activeIndex);
            });
            prevCameraButton.gameObject.SetActive(hasMultiple);
        }

        if (nextCameraButton)
        {
            nextCameraButton.onClick.RemoveAllListeners();
            nextCameraButton.onClick.AddListener(() =>
            {
                switcher.NextCamera();
                RetagActiveCamera(cams, switcher.activeIndex);
            });
            nextCameraButton.gameObject.SetActive(hasMultiple);
        }

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
                btn.onClick.AddListener(() =>
                {
                    switcher.SetActiveCamera(idx);
                    RetagActiveCamera(cams, idx);
                });

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
}
