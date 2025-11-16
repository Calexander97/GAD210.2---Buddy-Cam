using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Inventory inv = collision.GetComponent<Inventory>();
        if (inv != null)
        {
            inv.AddItem(item, amount);
            Destroy(gameObject);
        }
    }
}
