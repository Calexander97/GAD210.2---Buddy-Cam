using UnityEngine;

public class UIBlocker : MonoBehaviour
{
    public static UIBlocker Instance;
    public bool uiOpen = false;

    void Awake()
    {
        Instance = this;
    }
}
