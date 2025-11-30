using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Maps world -> minimap (static image). Place under a UI panel.
public class MinimapController : MonoBehaviour
{
    [Header("Map")]
    public RectTransform mapRect;   // the Image rect for the map panel
    public Vector2 worldMin = new(-8f, -4.5f);
    public Vector2 worldMax = new(8f, 4.5f);

    [Header("Icons")]
    public RectTransform iconContainer;  // parent for icons
    public MinimapIcon heroIcon;
    public List<MinimapIcon> guardIcons = new();

    public Vector2 WorldToMap(Vector2 world)
    {
        var size = worldMax - worldMin;
        float u = Mathf.InverseLerp(worldMin.x, worldMax.x, world.x);
        float v = Mathf.InverseLerp(worldMin.y, worldMax.y, world.y);
        var r = mapRect.rect;
        return new Vector2(Mathf.Lerp(r.xMin, r.xMax, u), Mathf.Lerp(r.yMin, r.yMax, v));
    }

    void LateUpdate()
    {
        if (heroIcon) heroIcon.UpdateIcon(this);
        for (int i = 0; i < guardIcons.Count; i++)
            if (guardIcons[i]) guardIcons[i].UpdateIcon(this);
    }
}
