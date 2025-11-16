using UnityEngine;
using UnityEngine.UI;

public class ImageAspect : MonoBehaviour
{
    private RawImage rawImage;

    void Awake() => rawImage = GetComponent<RawImage>();

    public void UpdateAspect(Camera cam)
    {
        if (cam == null) return;

        float camAspect = cam.aspect;
        RectTransform rt = rawImage.rectTransform;
        RectTransform parentRT = rt.parent as RectTransform;
        if (parentRT == null) return;

        float parentWidth = parentRT.rect.width;
        float parentHeight = parentRT.rect.height;
        float parentAspect = parentWidth / parentHeight;

        if (parentAspect > camAspect)
        {
            // Parent is wider → fit height
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentHeight);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentHeight * camAspect);
        }
        else
        {
            // Parent is taller → fit width
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentWidth / camAspect);
        }
    }
}
