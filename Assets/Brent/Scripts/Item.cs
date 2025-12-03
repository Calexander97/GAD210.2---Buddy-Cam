using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewItem", menuName = "InventoryItem")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 1;

    public enum ItemType { Consumable, Throwable, Keycard, Tool, SpecialUI }
    public ItemType type;

    [Header("Throwable Settings")]
    public GameObject throwablePrefab;
    public float throwForce = 10f;
    public float lifetime = 2f;

    [Header("Special UI Panel")]
    public GameObject uiPanel;

    [Header("Healing Settings")]
    public int healAmount = 1;
}