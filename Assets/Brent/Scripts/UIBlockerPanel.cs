using UnityEngine;

public class UIBlockerPanel : MonoBehaviour
{
    void OnEnable()
    {
        if (UIBlocker.Instance != null)
            UIBlocker.Instance.uiOpen = true;
    }

    void OnDisable()
    {
        if (UIBlocker.Instance != null)
            UIBlocker.Instance.uiOpen = false;
    }
}
