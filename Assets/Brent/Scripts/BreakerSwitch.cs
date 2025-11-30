using UnityEngine;
using UnityEngine.UI;

public class BreakerSwitch : MonoBehaviour
{
    [System.Serializable]
    public struct DoorAction
    {
        public DoorController door;
        public bool open;
    }

    public DoorAction[] actions;
    private bool toggled = false;

    public void Activate()
    {
        toggled = !toggled; // flip each press

        foreach (var a in actions)
        {
            if (a.door == null) continue;

            bool finalState = toggled ? !a.open : a.open;

            a.door.SetOpen(finalState);
        }
    }
}