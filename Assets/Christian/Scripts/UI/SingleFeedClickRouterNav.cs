using UnityEngine;

/// Routes clicks inside a single RawImage CCTV feed into the active world camera,
/// then tells the hero to move or interact.
public class SingleFeedClickRouterNav : MonoBehaviour
{
    [Header("UI / Cameras")]
    public RectTransform feedPanel;   // the RawImage rect (click area)
    public CameraSwitcher switcher;   // provides ActiveCamera

    [Header("Agent / Layers")]
    public HeroNavAgent2D hero;       // the NavMesh-driven character object
    public LayerMask interactMask;    // layers that contain IInteractable + 2D collider

    [Header("Options")]
    public bool requirePointerOverFeed = true;   // ignore clicks outside the feed
    [Tooltip("If a ground click lands near an interactable, prefer the interactable.")]
    public float interactSnapRadius = 0.35f;

    // Cache the Canvas & its event camera (for Screen Space - Camera / World Space)
    Canvas _cachedCanvas;

    void Awake()
    {
        if (feedPanel)
            _cachedCanvas = feedPanel.GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // stop all click movement / interactions while puzzle or UI is open
        if (UIBlocker.Instance != null && UIBlocker.Instance.uiOpen)
            return;
        if (!hero || !switcher) return;

        if (requirePointerOverFeed &&
            (!feedPanel || !RectTransformUtility.RectangleContainsScreenPoint(feedPanel, Input.mousePosition, GetEventCamera())))
            return;

        // Ensure we have a truly usable camera (enabled + active in hierarchy)
        var cam = GetUsableActiveCamera();
        if (!cam) return;

        if (!TryFeedClickToWorld(cam, out var world)) return;

        // let NextClickAction consume this click if it wants
        if (NextClickAction.Instance != null && NextClickAction.Instance.TryConsume(world))
            return;

        // 1) Direct interactable under the cursor?
        var under = Physics2D.OverlapPoint(world, interactMask);
        if (under && under.TryGetComponent<IInteractable>(out var inter))
        {
            hero.MoveToInteract(inter);
            return;
        }

        // 2) Soft-snap ground clicks to nearby interactables (quality-of-life).
        if (interactSnapRadius > 0f)
        {
            var near = Physics2D.OverlapCircleAll(world, interactSnapRadius, interactMask);
            for (int i = 0; i < near.Length; i++)
            {
                if (near[i].TryGetComponent<IInteractable>(out var nInter))
                {
                    hero.MoveToInteract(nInter);
                    return;
                }
            }
        }

        // 3) Otherwise, just move to the ground point.
        hero.MoveTo(world);
    }

    // Map a click inside the RawImage rect to a world-space position using the active camera.
    bool TryFeedClickToWorld(Camera cam, out Vector2 world)
    {
        world = Vector2.zero;

        if (feedPanel)
        {
            // Use the event camera if this Canvas isn't overlay.
            Camera uiCam = GetEventCamera();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(feedPanel, Input.mousePosition, uiCam, out var local))
                return false;

            // Convert rect-local space (centered) to 0..1 viewport UV.
            var r = feedPanel.rect;
            // local is measured in the rect’s local space; convert to [0..1]
            float u = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
            float v = Mathf.InverseLerp(r.yMin, r.yMax, local.y);

            var view = new Vector3(u, v, cam.orthographic ? 0f : Mathf.Abs(cam.transform.position.z));
            var w3 = cam.ViewportToWorldPoint(view);
            world = new Vector2(w3.x, w3.y);
            return true;
        }

        // Fallback: whole screen.
        var sp = Input.mousePosition;
        sp.z = cam.orthographic ? 0f : Mathf.Abs(cam.transform.position.z);
        var w3Fallback = cam.ScreenToWorldPoint(sp);
        world = new Vector2(w3Fallback.x, w3Fallback.y);
        return true;
    }

    // ---- Helpers -------------------------------------------------------------

    // Returns the camera we should use for UI point conversions.
    Camera GetEventCamera()
    {
        if (!_cachedCanvas) return null; // overlay or no canvas → null is correct
        if (_cachedCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _cachedCanvas.worldCamera ? _cachedCanvas.worldCamera : Camera.main;
    }

    // Make sure the camera we use is actually enabled/active; if not, try to recover.
    Camera GetUsableActiveCamera()
    {
        var cam = switcher.ActiveCamera;

        // If the active camera is disabled or inactive (can happen on floor swaps),
        // try to force-select a valid one without the user pressing any UI button.
        if (!IsCameraUsable(cam))
        {
            // Try re-asserting the current index
            switcher.SetActiveCamera(switcher.activeIndex, true);
            cam = switcher.ActiveCamera;

            if (!IsCameraUsable(cam))
            {
                // Last resort: pick the first enabled camera in the list
                if (switcher.cams != null)
                {
                    for (int i = 0; i < switcher.cams.Length; i++)
                    {
                        if (IsCameraUsable(switcher.cams[i]))
                        {
                            switcher.SetActiveCamera(i, true);
                            cam = switcher.cams[i];
                            break;
                        }
                    }
                }
            }
        }

        return IsCameraUsable(cam) ? cam : null;
    }

    static bool IsCameraUsable(Camera c)
        => c && c.enabled && c.gameObject.activeInHierarchy;
}
