using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Dictionary<string, GameObject> healPackObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        // EventBus 구독 설정
        EventBus<HealPackEventType>.Subscribe<IEnumerable<HealPack>>(HealPackEventType.UpdateHealPackStates, OnUpdateHealPackStates);
    }

    private void OnUpdateHealPackStates(IEnumerable<HealPack> items)
    {
        // Debug.Log($"Updating HealPacks: {items?.Count() ?? 0} items.");
        UpdateItems(items);
    }


    private void UpdateItems(IEnumerable<HealPack> items)
    {
        if (items == null || items.Count() == 0) return;

        HashSet<string> activeItemIds = new HashSet<string>();
        Dictionary<string, GameObject> targetDictionary = healPackObjects;

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
                    Debug.Log($"HealPack created with ID: {item.ItemId}");
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
        string path = "Prefabs/Item_HealPack";

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



    private void RemoveInactiveItems(HashSet<string> activeItemIds, Dictionary<string, GameObject> targetDictionary)
    {
        List<string> itemsToRemove = new List<string>();

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
                Destroy(itemObject);
                targetDictionary.Remove(itemId);
                Debug.Log($"Removed inactive {itemId}.");
            }
        }
    }

    private void OnDestroy()
    {
        // EventBus 구독 해제
        EventBus<HealPackEventType>.Unsubscribe<IEnumerable<HealPack>>(HealPackEventType.UpdateHealPackStates, OnUpdateHealPackStates);
    }
}
