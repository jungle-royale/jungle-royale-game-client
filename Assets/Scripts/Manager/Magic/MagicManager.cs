using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class MagicManager : MonoBehaviour
{
    private Dictionary<int, GameObject> magicItemObjects = new Dictionary<int, GameObject>();

    public void UpdateMagicList(List<Magic> items)
    {
        if (items == null || items.Count() == 0) return;

        HashSet<int> activeItemIds = new HashSet<int>();
        Dictionary<int, GameObject> targetDictionary = magicItemObjects;

        foreach (var item in items)
        {
            activeItemIds.Add(item.ItemId);

            if (!targetDictionary.TryGetValue(item.ItemId, out GameObject itemObject))
            {

                GameObject itemPrefab = LoadPrefab(item.MagicType);

                if (itemPrefab != null)
                {
                    itemObject = Instantiate(itemPrefab, item.Position(), Quaternion.identity);
                    targetDictionary[item.ItemId] = itemObject;
                    // Debug.Log($"Magic {item.MagicType} created with ID: {item.ItemId}");
                }
                else
                {
                    Debug.LogError($"Magic {item.MagicType} prefab could not be loaded.");
                    continue;
                }
            }

            // Update the position of the item
            itemObject.transform.position = item.Position();
        }

        // Remove inactive items
        RemoveInactiveItems(activeItemIds, targetDictionary);
    }

    private GameObject LoadPrefab(MagicType type)
    {
        string path = "Prefabs/Items/Item_";

        switch (type)
        {
            case MagicType.Fire:
                path += "FireMagic";
                break;
            case MagicType.Stone:
                path += "StoneMagic";
                break;
            default:
                Debug.LogError($"{type}에 해당하는 매직 타입 없음");
                break;
        };

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
                Destroy(itemObject);
                targetDictionary.Remove(itemId);
                Debug.Log($"Removed inactive {itemId}.");
            }
        }
    }

}