using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public List<Button> slotButtons;
    public Button useButton;

    // Highlighting
    [Header("Highlight Colors")]
    public Color normalSlotColor = Color.white;
    public Color selectedSlotColor = new Color(0.3f, 0.8f, 1f, 1f);

    private int selectedIndex = -1;
    private bool isUsing = false;

    void Start()
    {
        // assign click events to slot buttons
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


    // UI UPDATE

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
                    countText.text =
                        (item.maxStack > 1) ? inventory.slots[i].count.ToString() : "";
                }
            }
            else
            {
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }

                if (countText != null)
                    countText.text = "";
            }
        }

        ApplySlotHighlighting();
    }


    // SLOT SELECTION

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= inventory.slots.Count) return;

        var slot = inventory.slots[index];
        if (slot.item == null) return;

        selectedIndex = index;

        ItemDescriptionUI.Instance.ShowDescription(slot.item);

        UpdateUI();
    }


    // VISUAL HIGHLIGHTING

    private void ApplySlotHighlighting()
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            Image bg = slotButtons[i].GetComponent<Image>();
            if (bg != null)
            {
                bg.color = (i == selectedIndex) ? selectedSlotColor : normalSlotColor;
            }
        }
    }


    public void ClearSelection()
    {
        selectedIndex = -1;
        UpdateUI();
    }


    // USE BUTTON

    private void OnUseButtonPressed()
    {
        if (isUsing) return;
        isUsing = true;

        if (selectedIndex >= 0 && selectedIndex < inventory.slots.Count)
        {
            InventorySlot slot = inventory.slots[selectedIndex];
            if (slot.item != null)
            {
                inventory.UseItem(selectedIndex);
            }
        }

        StartCoroutine(ReenableUseButton());
    }

    private IEnumerator ReenableUseButton()
    {
        yield return null;
        isUsing = false;
    }
}