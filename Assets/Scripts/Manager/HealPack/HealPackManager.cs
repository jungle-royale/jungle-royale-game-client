using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class HealPackManager : MonoBehaviour
{
    private Dictionary<int, GameObject> healPackObjects = new Dictionary<int, GameObject>();

    public void UpdateHealPackList(List<HealPack> items)
    {
        if (items == null || items.Count() == 0) return;

        HashSet<int> activeItemIds = new HashSet<int>();
        Dictionary<int, GameObject> targetDictionary = healPackObjects;

        foreach (var item in items)
        {
            activeItemIds.Add(item.ItemId);

            if (!targetDictionary.TryGetValue(item.ItemId, out GameObject itemObject))
            {

                GameObject itemPrefab = LoadPrefab();

                if (itemPrefab != null)
                {
                    itemObject = Instantiate(itemPrefab, item.Position(), Quaternion.identity);
                    targetDictionary[item.ItemId] = itemObject;
                }
                else
                {
                    Debug.LogError($"HealPack prefab could not be loaded.");
                    continue;
                }
            }

            // Update the position of the item
            itemObject.transform.position = item.Position();
        }

        // Remove inactive items
        RemoveInactiveItems(activeItemIds, targetDictionary);
    }

    private GameObject LoadPrefab()
    {
        string path = "Prefabs/Items/Item_HealPack";

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Invalid item type.");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at path: {path}");
        }
        return prefab;
    }



    private void RemoveInactiveItems(HashSet<int> activeItemIds, Dictionary<int, GameObject> targetDictionary)
    {
        List<int> itemsToRemove = new List<int>();

        foreach (var itemId in targetDictionary.Keys)
        {
            if (!activeItemIds.Contains(itemId))
            {
                itemsToRemove.Add(itemId);
            }
        }

        foreach (var itemId in itemsToRemove)
        {
            if (targetDictionary.TryGetValue(itemId, out GameObject itemObject))
            {
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.Heal, 1.0f);
                Destroy(itemObject);
                targetDictionary.Remove(itemId);
                Debug.Log($"Removed inactive {itemId}.");
            }
        }
    }

}
