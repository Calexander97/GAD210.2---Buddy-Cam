using UnityEngine;

/// Attach to a small UI Image under the minimap. Assign target (world object).
public class MinimapIcon : MonoBehaviour
{
    public Transform target;     // hero or guard
    public RectTransform rect;   // self
    public bool rotateToFacing = true;
    public bool forwardIsUp = true;

    void Reset() { rect = GetComponent<RectTransform>(); }

    public void UpdateIcon(MinimapController map)
    {
        if (!target || !rect) return;
        Vector2 p = map.WorldToMap(target.position);
        rect.anchoredPosition = p;

        if (rotateToFacing)
        {
            float z = target.eulerAngles.z + (forwardIsUp ? 0f : 90f);
            rect.rotation = Quaternion.Euler(0, 0, z);
        }
    }
}
