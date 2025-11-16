using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public List<Button> slotButtons;
    public Button useButton;

    private int selectedIndex = -1;
    private bool isUsing = false;

    void Start()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            int index = i;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => SelectSlot(index));
        }

        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnUseButtonPressed);
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            Image iconImage = slotButtons[i].transform.Find("Icon")?.GetComponent<Image>();
            TMP_Text countText = slotButtons[i].transform.Find("Count")?.GetComponent<TMP_Text>();

            if (i < inventory.slots.Count && inventory.slots[i].item != null)
            {
                Item item = inventory.slots[i].item;
                if (iconImage != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = true;
                }
                if (countText != null)
                {
                    countText.text = (item.maxStack > 1) ? inventory.slots[i].count.ToString() : "";
                }
            }
            else
            {
                if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
                if (countText != null) countText.text = "";
            }
        }

        HighlightSelectedSlot();
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= inventory.slots.Count) return;
        if (inventory.slots[index].item == null) return;

        selectedIndex = index;
        UpdateUI();
    }

    private void HighlightSelectedSlot()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            ColorBlock colors = slotButtons[i].colors;
            colors.normalColor = (i == selectedIndex) ? new Color(0.3f, 0.8f, 0.3f) : Color.white;
            slotButtons[i].colors = colors;
        }
    }

    private void OnUseButtonPressed()
    {
        if (isUsing) return;  // ignore multiple clicks
        isUsing = true;

        if (selectedIndex >= 0 && selectedIndex < inventory.slots.Count)
        {
            InventorySlot slot = inventory.slots[selectedIndex];
            if (slot.item != null)
            {
                inventory.UseItem(selectedIndex);
            }
        }

        // wait before allowing another click
        StartCoroutine(ReenableUseButton());
    }

    private IEnumerator ReenableUseButton()
    {
        yield return null;
        isUsing = false;
    }
}