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

    private bool isUsingItem = false;

    void Awake()
    {
        int slotCount = 4;
        for (int i = 0; i < slotCount; i++)
            slots.Add(new InventorySlot());
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

        string itemName = slot.item.itemName;
        DropItem(slot.item, throwOrigin.position);
        slot.count--;
        if (slot.count <= 0) slot.Clear();

        Debug.Log($"Used 1 {itemName}");
        ui?.UpdateUI();

        isUsingItem = false;
    }

    private void DropItem(Item item, Vector3 position)
    {
        if (item.throwablePrefab == null) return;

        GameObject spawned = Instantiate(item.throwablePrefab, position, Quaternion.identity);

        Rigidbody2D rb = spawned.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(Vector3.zero);

        Destroy(spawned, item.lifetime);
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