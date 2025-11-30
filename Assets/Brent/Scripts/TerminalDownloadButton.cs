using UnityEngine;
using UnityEngine.UI;

public class TerminalDownloadButton : MonoBehaviour
{
    public Item rewardItem; 
    public Inventory playerInventory; 
    public GameObject panelToClose; 

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnDownloadClicked);
    }

    private void OnDownloadClicked()
    {
        if (rewardItem != null && playerInventory != null)
        {
            playerInventory.AddItem(rewardItem, 1);
            Debug.Log($"Added {rewardItem.itemName} to inventory!");
        }

        if (panelToClose != null)
            panelToClose.SetActive(false);
    }
}