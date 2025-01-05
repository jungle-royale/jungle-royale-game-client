using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class MagicManager : MonoBehaviour
{
    private Dictionary<string, GameObject> magicItemObjects = new Dictionary<string, GameObject>();

    public void UpdateMagicList(List<Magic> items)
    {
        if (items == null || items.Count() == 0) return;

        HashSet<string> activeItemIds = new HashSet<string>();
        Dictionary<string, GameObject> targetDictionary = magicItemObjects;

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
                path += "StoneMagic";
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
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.GetItem);
                Debug.Log($"Removed inactive {itemId}.");
            }
        }
    }

}