using UnityEngine;
using UnityEngine.UI;
using System;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Assign in inspector")]
    public Camera[] cams;            // Floor cameras
    public RawImage feedImage;       // Single CCTV feed (uses cam.targetTexture)
    public int activeIndex = 0;

    public event Action<int, Camera> OnCameraChanged;

    void Awake()
    {
        // Ensure consistency at boot even if cams is empty
        ApplyActiveCamera(true);
    }

    void Update()
    {
        if (cams == null || cams.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCamera(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCamera(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetActiveCamera(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetActiveCamera(3);
    }

    /// Replace the active camera list (used by LevelManager per floor switch).
    public void SetCameras(Camera[] newCams, int startIndex = 0)
    {
        cams = newCams ?? Array.Empty<Camera>();
        activeIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, cams.Length - 1));
        ApplyActiveCamera(true);
    }

    public void SetActiveCamera(int index) => SetActiveCamera(index, false);

    public void SetActiveCamera(int index, bool force)
    {
        if (cams == null || cams.Length == 0) return;
        index = Mathf.Clamp(index, 0, cams.Length - 1);
        if (!force && index == activeIndex) return;

        activeIndex = index;
        ApplyActiveCamera(true);
    }

    public void NextCamera()
    {
        if (cams == null || cams.Length == 0) return;
        activeIndex = (activeIndex + 1) % cams.Length;
        ApplyActiveCamera(true);
    }

    public void PrevCamera()
    {
        if (cams == null || cams.Length == 0) return;
        activeIndex = (activeIndex - 1 + cams.Length) % cams.Length;
        ApplyActiveCamera(true);
    }

    public Camera ActiveCamera
        => (cams != null && cams.Length > 0) ? cams[Mathf.Clamp(activeIndex, 0, cams.Length - 1)] : null;

    // --- internals ---

    void ApplyActiveCamera(bool notify)
    {
        // Enable only the active camera
        if (cams != null)
        {
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i]) cams[i].enabled = (i == activeIndex);
            }
        }

        // Feed the RawImage with the active camera's RenderTexture (if any)
        var cam = ActiveCamera;
        if (feedImage)
        {
            // If your cams use a RenderTexture, assign it here. If not, leave it alone.
            feedImage.texture = cam && cam.targetTexture ? cam.targetTexture : feedImage.texture;
        }

        if (notify) OnCameraChanged?.Invoke(activeIndex, cam);
    }
}
