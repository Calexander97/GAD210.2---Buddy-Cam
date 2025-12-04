using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ItemDescriptionUI : MonoBehaviour
{
    public static ItemDescriptionUI Instance;

    [Header("UI Elements")]
    public GameObject panel;
    public TMP_Text itemNameText;
    public TMP_Text descriptionText;

    private bool isOpen = false;

    void Awake()
    {
        Instance = this;

        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowDescription(Item item)
    {
        if (item == null || panel == null) return;

        itemNameText.text = item.itemName;
        descriptionText.text = item.description;

        panel.SetActive(true);
        isOpen = true;
    }

    void Update()
    {
        if (!isOpen || !Input.GetMouseButtonDown(0))
            return;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool clickedInventoryUI = false;

        foreach (var r in results)
        {
            if (r.gameObject.CompareTag("InventoryUI"))
            {
                clickedInventoryUI = true;
                break;
            }
        }

        // if click was NOT on inventory UI - close panel
        if (!clickedInventoryUI)
        {
            ClosePanel();
        }
    }

    public void ClosePanel()
    {
        if (!isOpen) return;

        panel.SetActive(false);
        isOpen = false;

        // clear slot highlight
        FindAnyObjectByType<PlayerInventoryUI>()?.ClearSelection();
    }
}