using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<InventorySlot> slots = new List<InventorySlot>();

    [Header("References")]
    public Transform throwOrigin;
    public PlayerInventoryUI ui;
    public SpecialUIPanelManager uiPanelManager;
    public GameObject player;

    [Header("Starting Items")]
    public List<Item> startingItems = new List<Item>();

    private bool isUsingItem = false;

    void Awake()
    {
        int slotCount = 4;
        for (int i = 0; i < slotCount; i++)
            slots.Add(new InventorySlot());
    }

    private void Start()
    {
        foreach (var item in startingItems)
        {
            AddItem(item, 1);
        }
    }
    public void AddItem(Item item, int amount = 1)
    {
        // try to stack first
        foreach (InventorySlot slot in slots)
        {
            if (slot.item != null && slot.item.itemName == item.itemName && slot.count < item.maxStack)
            {
                int spaceLeft = item.maxStack - slot.count;
                int toAdd = Mathf.Min(spaceLeft, amount);
                slot.count += toAdd;
                amount -= toAdd;

                if (amount <= 0)
                {
                    ui?.UpdateUI();
                    return;
                }
            }
        }

        // fill empty slots
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.count = Mathf.Min(item.maxStack, amount);
                amount -= slot.count;

                if (amount <= 0)
                {
                    ui?.UpdateUI();
                    return;
                }
            }
        }

        if (amount > 0)
            Debug.Log("Inventory full! Could not add all items.");

        ui?.UpdateUI();
    }

    public void UseItem(int slotIndex)
    {
        if (isUsingItem) return;
        isUsingItem = true;

        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            isUsingItem = false;
            return;
        }

        InventorySlot slot = slots[slotIndex];
        if (slot.item == null)
        {
            isUsingItem = false;
            return;
        }

        Item item = slot.item;

        // ----- SPECIAL UI ITEMS -----
        if (item.type == Item.ItemType.SpecialUI)
        {
            GameObject panel = uiPanelManager.GetPanelForItem(item.itemName);

            if (panel != null)
            {
                panel.SetActive(true);
                Debug.Log("Opened special UI for item: " + item.itemName);
            }
            else
            {
                Debug.LogWarning("No panel assigned for SpecialUI item: " + item.itemName);
            }

            // Do NOT remove item
            ui?.UpdateUI();
            isUsingItem = false;
            return;
        }

        // --- THROWABLE ITEM ---
        if (item.type == Item.ItemType.Throwable)
        {
            if (NextClickAction.Instance != null)
            {
                NextClickAction.Instance.onNextClick = (Vector2 pos) =>
                {
                    Instantiate(item.throwablePrefab, pos, Quaternion.identity);
                };
            }

            // consume one from the stack
            slot.count--;
            if (slot.count <= 0) slot.Clear();
            ui.UpdateUI();

            isUsingItem = false;
            return;
        }

        // --- CONSUMABLE ITEM ---
        if (item.type == Item.ItemType.Consumable)
        {
            HeroHealth health = player.GetComponent<HeroHealth>();

            if (health != null)
            {
                // only use if player is missing health
                if (health.Current < health.maxHearts)
                {
                    health.Heal(item.healAmount);

                    slot.count--;
                    if (slot.count <= 0) slot.Clear();

                    ui?.UpdateUI();
                    isUsingItem = false;
                    return;
                }
                else
                {
                    Debug.Log("Health is full — cannot use this item.");
                    isUsingItem = false;
                    return;
                }
            }

            isUsingItem = false;
            return;
        }

        // --- DEFAULT ---
        Debug.Log("Used item: " + item.itemName);
        isUsingItem = false;
    }

    private void DropItem(Item item, Vector3 position)
    {
        if (item.throwablePrefab == null) return;

        Instantiate(item.throwablePrefab, position, Quaternion.identity);
    }
}



[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int count;

    public void Clear()
    {
        item = null;
        count = 0;
    }
}