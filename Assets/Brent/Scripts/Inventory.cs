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
    public LayerMask throwBlockMask;

    [Header("Starting Items")]
    public List<Item> startingItems = new List<Item>();

    private bool isUsingItem = false;

    void Awake()
    {
        int slotCount = 5;
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


        // SPECIAL UI ITEMS

        if (item.type == Item.ItemType.SpecialUI)
        {
            GameObject panel = uiPanelManager.GetPanelForItem(item.itemName);

            if (panel != null)
            {
                panel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("No panel assigned for " + item.itemName);
            }

            ui?.UpdateUI();
            isUsingItem = false;
            return;
        }


        // THROWABLE ITEMS  -  arc projectile - placed object

        if (item.type == Item.ItemType.Throwable)
        {
            if (item.projectilePrefab == null || item.placedItemPrefab == null)
            {
                Debug.LogError("Throwable item is missing projectilePrefab or placedPrefab: " + item.itemName);
                isUsingItem = false;
                return;
            }

            if (NextClickAction.Instance != null)
            {
                NextClickAction.Instance.onNextClick = (Vector2 rawPos) =>
                {
                    Vector2 origin = player.transform.position;
                    Vector2 dir = rawPos - origin;
                    float dist = dir.magnitude;

                    Vector2 finalPos = rawPos;

                    // clamp to first wall hit
                    RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist, throwBlockMask);
                    if (hit.collider != null)
                    {
                        // pull back slightly from wall so it doesn't clip into it
                        finalPos = hit.point - dir.normalized * 0.1f;
                    }

                    // spawn projectile at player
                    GameObject proj = Instantiate(item.projectilePrefab, origin, Quaternion.identity);
                    SFXManager.Instance.PlaySFX(SFXManager.Instance.itemThrow);

                    // initialise arc with the clamped target
                    LureProjectile lp = proj.GetComponent<LureProjectile>();
                    lp.Init(finalPos, item.placedItemPrefab);
                };
            }

            // consume one from the stack
            slot.count--;
            if (slot.count <= 0) slot.Clear();
            ui?.UpdateUI();

            isUsingItem = false;
            return;
        }


        // CONSUMABLES (healing items)

        if (item.type == Item.ItemType.Consumable)
        {
            HeroHealth health = player.GetComponent<HeroHealth>();

            if (health != null)
            {
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
                    Debug.Log("Health full — cannot use item.");
                    isUsingItem = false;
                    return;
                }
            }

            isUsingItem = false;
            return;
        }


        // DEFAULT

        Debug.Log("Used item: " + item.itemName);
        isUsingItem = false;
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