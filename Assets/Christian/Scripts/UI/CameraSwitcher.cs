using UnityEngine;
using UnityEngine.UI;

/// Enables exactly one world camera at a time and drives the single CCTV feed.
public class CameraSwitcher : MonoBehaviour
{
    [Header("Assign in inspector")]
    public Camera[] cams;        // Cam_01 … Cam_04 (per floor)
    public RawImage feedImage;
    public int activeIndex = 0;

    // Remember the previously managed camera list so we can hard-disable it
    Camera[] _prevCams;

    void Awake()
    {
        _prevCams = cams;
        SetActiveCamera(activeIndex, true);
    }

    void Update()
    {
        // Hotkeys (optional)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCamera(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCamera(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveCamera(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetActiveCamera(3);
    }

    public void SetActiveCamera(int index) => SetActiveCamera(index, false);

    /// Core swap. If the camera list changed (e.g., floor switch),
    /// disable every camera from the previous list before enabling the new one.
    public void SetActiveCamera(int index, bool force)
    {
        if (cams == null || cams.Length == 0) return;

        // If LevelManager just swapped the camera array, kill the old list first
        if (!ReferenceEquals(_prevCams, cams))
        {
            DisableAll(_prevCams);   // turn off every cam from the previous floor
            _prevCams = cams;        // start managing the new list
        }

        index = Mathf.Clamp(index, 0, cams.Length - 1);
        if (!force && index == activeIndex) return;

        for (int i = 0; i < cams.Length; i++)
        {
            var c = cams[i];
            if (!c) continue;
            c.enabled = (i == index);
        }

        activeIndex = index;
    }

    void DisableAll(Camera[] list)
    {
        if (list == null) return;
        for (int i = 0; i < list.Length; i++)
        {
            var c = list[i];
            if (c) c.enabled = false;
        }
    }

    public Camera ActiveCamera =>
        (cams != null && cams.Length > 0) ? cams[Mathf.Clamp(activeIndex, 0, cams.Length - 1)] : null;

    // UI buttons
    public void NextCamera()
    {
        if (cams == null || cams.Length == 0) return;
        SetActiveCamera((activeIndex + 1) % cams.Length, true);
    }

    public void PrevCamera()
    {
        if (cams == null || cams.Length == 0) return;
        SetActiveCamera((activeIndex - 1 + cams.Length) % cams.Length, true);
    }
}
