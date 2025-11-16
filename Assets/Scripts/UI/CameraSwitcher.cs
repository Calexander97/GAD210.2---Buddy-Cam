using UnityEngine;
using UnityEngine.UI;

/// Enables exactly one world camera at a time and drives the single CCTV feed.
public class CameraSwitcher : MonoBehaviour
{
    [Header("Assign in inspector")]
    public Camera[] cams;        // Cam_01 … Cam_04
    public RawImage feedImage;  
    public int activeIndex = 0;

    void Awake() => SetActiveCamera(activeIndex, true);

    void Update()
    {
        // Hotkeys
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCamera(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCamera(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveCamera(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetActiveCamera(3);
    }

    public void SetActiveCamera(int index) => SetActiveCamera(index, false);

    // Core swap
    public void SetActiveCamera(int index, bool force)
    {
        if (cams == null || cams.Length == 0) return;
        index = Mathf.Clamp(index, 0, cams.Length - 1);
        if (!force && index == activeIndex) return;

        for (int i = 0; i < cams.Length; i++)
            if (cams[i]) cams[i].enabled = (i == index);

        activeIndex = index;
    }

    public Camera ActiveCamera =>
        (cams != null && cams.Length > 0) ? cams[activeIndex] : null;

    // UI buttons
    public void NextCamera() => SetActiveCamera((activeIndex + 1) % cams.Length, true);
    public void PrevCamera() => SetActiveCamera((activeIndex - 1 + cams.Length) % cams.Length, true);
}
