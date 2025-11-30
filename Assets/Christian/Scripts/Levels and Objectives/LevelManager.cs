using System.Linq;
using UnityEngine;
using UnityEngine.AI;                 // NavMeshAgent + NavMesh.SamplePosition
using NavMeshPlus.Components;         // NavMeshSurface (NavMeshPlus)
using UnityEngine.UI;
using TMPro;                          // Button labels

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class FloorSet
    {
        public GameObject root;            // e.g. Floor_01 / Floor_02 / Floor_03
        public NavMeshSurface surface;     // NavMeshPlus surface (pre-baked)
        public Transform spawn;            // optional; auto-found by name if null
        public Camera[] cameras;           // optional; auto-found if empty
        public GuardAI[] guards;           // optional; auto-found if empty
    }

    [Header("Core")]
    public CameraSwitcher switcher;        // your single-feed camera switcher
    public Transform hero;                 // persistent hero
    public NavMeshAgent heroAgent;         // configured for XY (updateUpAxis=false)

    [Header("Floors (order = progression)")]
    public FloorSet[] floors;

    [Header("Options")]
    public int currentIndex = -1;
    public float warpSampleRadius = 2f;            // snap hero onto mesh when switching
    public bool autoEnableOnlyCurrentFloorGuards = true;
    public string defaultSpawnName = "Spawn_Entry";

    [Header("Camera UI (optional)")]
    public Button prevCameraButton;
    public Button nextCameraButton;
    public Button[] cameraIndexButtons;      // e.g. 4 buttons for Cam1..Cam4
    public TMP_Text[] cameraButtonTMP;       // labels for those buttons (optional)



    void Awake()
    {
        // Hide everything at boot; we’ll enable the active floor in Start/SwitchToFloor
        for (int i = 0; i < floors.Length; i++)
        {
            if (floors[i]?.root) floors[i].root.SetActive(false);
        }
    }

    void Start()
    {
        if (floors == null || floors.Length == 0) return;

        if (currentIndex < 0) SwitchToFloor(0);
        else SwitchToFloor(Mathf.Clamp(currentIndex, 0, floors.Length - 1));
    }

    public void NextFloor() => SwitchToFloor(Mathf.Clamp(currentIndex + 1, 0, floors.Length - 1));
    public void PrevFloor() => SwitchToFloor(Mathf.Clamp(currentIndex - 1, 0, floors.Length - 1));

    public void SwitchToFloor(int index)
    {
        if (floors == null || index < 0 || index >= floors.Length) return;

        // ---- Deactivate old floor ----
        if (currentIndex >= 0 && currentIndex < floors.Length)
        {
            var old = floors[currentIndex];
            if (old != null)
            {
                if (old.surface) old.surface.enabled = false;   // removes baked data
                if (old.root) old.root.SetActive(false);
            }
        }

        // ---- Activate new floor ----
        var f = floors[index];
        if (f == null || !f.root)
        {
            Debug.LogWarning("LevelManager: Floor root missing.");
            return;
        }

        f.root.SetActive(true);
        currentIndex = index;

        // Ensure we have references (auto-find where needed)
        var surface = ResolveSurface(f);
        var spawn = f.spawn ? f.spawn : FindByNameRecursive(f.root.transform, defaultSpawnName);
        var cams = (f.cameras != null && f.cameras.Length > 0)
                        ? f.cameras
                        : f.root.GetComponentsInChildren<Camera>(true);
        var guards = (f.guards != null && f.guards.Length > 0)
                        ? f.guards
                        : f.root.GetComponentsInChildren<GuardAI>(true);

        // Enable surface (adds baked data)
        if (surface) surface.enabled = true;

        // Place hero on this floor (snap to mesh)
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

        // Feed cameras to the single feed switcher
        if (switcher != null && cams != null && cams.Length > 0)
        {
            switcher.cams = cams;
            switcher.SetActiveCamera(0, true);
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

    // --- Helpers ---

    NavMeshSurface ResolveSurface(FloorSet f)
    {
        if (f.surface) return f.surface;
        var s = f.root ? f.root.GetComponentInChildren<NavMeshSurface>(true) : null;
        f.surface = s;
        return s;
    }

    Transform FindByNameRecursive(Transform root, string name)
    {
        if (!root) return null;
        if (root.name == name) return root;
        foreach (Transform c in root)
        {
            var found = FindByNameRecursive(c, name);
            if (found) return found;
        }
        return null;
    }

    void WireCameraButtons(Camera[] cams)
    {
        if (switcher == null) return;

        // Prev / Next
        bool hasMultiple = cams != null && cams.Length > 1;
        
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

        // Index Buttons
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
                int idx = i;
                btn.onClick.AddListener(() => switcher.SetActiveCamera(idx));

                // TMP label
                if (cameraButtonTMP != null && i < cameraButtonTMP.Length && cameraButtonTMP[i])
                {
                    // Use camera's came, or fallback like Cam 1
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
