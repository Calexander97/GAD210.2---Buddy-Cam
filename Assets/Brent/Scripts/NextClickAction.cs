using System;
using UnityEngine;

public class NextClickAction : MonoBehaviour
{
    public static NextClickAction Instance;

    // What to do with the NEXT world click (if any)
    public Action<Vector2> onNextClick;

    void Awake()
    {
        Instance = this;
    }

    public bool TryConsume(Vector2 worldPos)
    {
        if (onNextClick == null)
            return false;

        onNextClick.Invoke(worldPos);
        onNextClick = null;   // only once
        return true;
    }
}