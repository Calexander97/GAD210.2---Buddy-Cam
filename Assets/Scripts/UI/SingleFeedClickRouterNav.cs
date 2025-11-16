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

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!hero || !switcher) return;

        if (requirePointerOverFeed &&
            (!feedPanel || !RectTransformUtility.RectangleContainsScreenPoint(feedPanel, Input.mousePosition)))
            return;

        var cam = switcher.ActiveCamera;
        if (!cam) return;

        if (!TryFeedClickToWorld(cam, out var world)) return;

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
            if (near != null)
            {
                for (int i = 0; i < near.Length; i++)
                    if (near[i] && near[i].TryGetComponent<IInteractable>(out var nInter))
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
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(feedPanel, Input.mousePosition, null, out var local))
                return false;

            // Convert rect-local space (centered) to 0..1 viewport UV.
            var r = feedPanel.rect;
            float u = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
            float v = Mathf.InverseLerp(r.yMin, r.yMax, local.y);

            var view = new Vector3(u, v, cam.orthographic ? cam.nearClipPlane : Mathf.Abs(cam.transform.position.z));
            var w3 = cam.ViewportToWorldPoint(view);
            world = new Vector2(w3.x, w3.y);
            return true;
        }

        // Fallback: whole screen.
        var sp = Input.mousePosition;
        sp.z = cam.orthographic ? cam.nearClipPlane : Mathf.Abs(cam.transform.position.z);
        var w3Fallback = cam.ScreenToWorldPoint(sp);
        world = new Vector2(w3Fallback.x, w3Fallback.y);
        return true;
    }
}
