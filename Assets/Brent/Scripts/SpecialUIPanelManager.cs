using UnityEngine;

public class SpecialUIPanelManager : MonoBehaviour
{
    [System.Serializable]
    public class UIEntry
    {
        public string itemName;
        public GameObject panel;
    }

    public UIEntry[] specialUIPanels;

    public GameObject GetPanelForItem(string itemName)
    {
        foreach (var entry in specialUIPanels)
        {
            if (entry.itemName == itemName)
                return entry.panel;
        }
        return null;
    }
}
