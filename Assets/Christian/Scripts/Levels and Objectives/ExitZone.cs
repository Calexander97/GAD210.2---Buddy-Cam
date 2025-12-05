using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitZone : MonoBehaviour
{
    [Header("Required Item")]
    public Item biopackItem;               // drag the Biopack Item asset (if your inventory stores Item refs)
    [Tooltip("Fallback match by name/id if your inventory stores strings or different Item instances.")]
    public string biopackIdOrName;         // e.g., "Biopack", "DataPackage", etc.

    [Header("Who can exit")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool verbose = false;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var om = ObjectiveManager.I;
        if (!om) return;

        var inv = other.GetComponent<Inventory>();
        if (!inv)
        {
            if (verbose) Debug.Log("[Exit] Player has no Inventory component.");
            return;
        }

        if (HasBiopack(inv))
        {
            om.CompleteMission();
        }
        else
        {
            if (verbose) DumpInventory(inv);
            Debug.Log("[Exit] Blocked: Biopack not found in Inventory.");
        }
    }

    // --- Core checks ---------------------------------------------------------

    bool HasBiopack(object inventory)
    {
        // Prefer exact Item reference if provided
        if (biopackItem)
        {
            if (CallBool(inventory, "HasItem", biopackItem)) return true;
            if (CallInt(inventory, "GetCount", biopackItem) > 0) return true;
        }

        // Fallback: by name/id if provided
        if (!string.IsNullOrEmpty(biopackIdOrName))
        {
            if (CallBool(inventory, "HasItem", biopackIdOrName)) return true;
            if (CallInt(inventory, "GetCount", biopackIdOrName) > 0) return true;
        }

        // Enumerate common fields: items / inventory / slots / bag
        foreach (var listName in new[] { "items", "inventory", "slots", "bag", "Items", "Inventory" })
        {
            if (EnumerateField(inventory, listName, out IEnumerable enumerable))
            {
                foreach (var entry in enumerable)
                {
                    if (entry == null) continue;

                    // Case A: collection is Item
                    if (entry is Item it)
                    {
                        if (biopackItem && it == biopackItem) return true;
                        if (!string.IsNullOrEmpty(biopackIdOrName) && it.name == biopackIdOrName) return true;
                        continue;
                    }

                    // Case B: slot { Item item; int amount; string id/name; }
                    var eT = entry.GetType();

                    // item reference
                    var fItem = eT.GetField("item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fItem != null)
                    {
                        var slotItem = fItem.GetValue(entry) as Item;
                        if (slotItem)
                        {
                            if (biopackItem && slotItem == biopackItem) return true;
                            if (!string.IsNullOrEmpty(biopackIdOrName) && slotItem.name == biopackIdOrName) return true;
                        }
                    }

                    // string id/name
                    var fId = eT.GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          ?? eT.GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fId != null && !string.IsNullOrEmpty(biopackIdOrName))
                    {
                        var idVal = fId.GetValue(entry) as string;
                        if (!string.IsNullOrEmpty(idVal) && idVal == biopackIdOrName) return true;
                    }

                    // amount/count (optional)
                    var fAmt = eT.GetField("amount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          ?? eT.GetField("count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          ?? eT.GetField("quantity", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fAmt != null)
                    {
                        var v = fAmt.GetValue(entry);
                        if (v is int amt && amt <= 0) continue; // empty slot
                    }
                }
            }
        }

        return false;
    }

    // --- Small reflection helpers -------------------------------------------

    bool CallBool(object obj, string method, Item arg)
    {
        var m = obj.GetType().GetMethod(method, new[] { typeof(Item) });
        if (m == null) return false;
        try { return (bool)m.Invoke(obj, new object[] { arg }); } catch { return false; }
    }
    bool CallBool(object obj, string method, string arg)
    {
        var m = obj.GetType().GetMethod(method, new[] { typeof(string) });
        if (m == null) return false;
        try { return (bool)m.Invoke(obj, new object[] { arg }); } catch { return false; }
    }
    int CallInt(object obj, string method, Item arg)
    {
        var m = obj.GetType().GetMethod(method, new[] { typeof(Item) });
        if (m == null) return 0;
        try { return (int)m.Invoke(obj, new object[] { arg }); } catch { return 0; }
    }
    int CallInt(object obj, string method, string arg)
    {
        var m = obj.GetType().GetMethod(method, new[] { typeof(string) });
        if (m == null) return 0;
        try { return (int)m.Invoke(obj, new object[] { arg }); } catch { return 0; }
    }
    bool EnumerateField(object obj, string field, out IEnumerable list)
    {
        var f = obj.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null)
        {
            list = f.GetValue(obj) as IEnumerable;
            return list != null;
        }
        list = null;
        return false;
    }

    void DumpInventory(object inv)
    {
        // Best-effort dump of recognizable collections
        var sb = new System.Text.StringBuilder();
        sb.Append("[Exit] Inventory contents: ");

        bool any = false;
        foreach (var listName in new[] { "items", "inventory", "slots", "bag", "Items", "Inventory" })
        {
            if (!EnumerateField(inv, listName, out IEnumerable enumerable)) continue;
            any = true;
            sb.Append("{").Append(listName).Append(": ");

            var first = true;
            foreach (var entry in enumerable)
            {
                if (!first) sb.Append(", ");
                first = false;

                if (entry is Item it) sb.Append(it.name);
                else
                {
                    var eT = entry.GetType();
                    var fItem = eT.GetField("item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var fId = eT.GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             ?? eT.GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (fItem?.GetValue(entry) is Item sit) sb.Append(sit.name);
                    else if (fId?.GetValue(entry) is string sid) sb.Append(sid);
                    else sb.Append(entry.ToString());
                }
            }
            sb.Append("} ");
        }

        if (!any) sb.Append("(no recognizable collections found)");
        Debug.Log(sb.ToString());
    }
}
