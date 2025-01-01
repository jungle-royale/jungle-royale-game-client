using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Dictionary<string, GameObject> healPackObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> magicItemObjects = new Dictionary<string, GameObject>();

    public enum ItemType
    {
        HealPack,
        MagicItem,
        StoneMagic,
        FireMagic,
    }

    void Start()
    {
        // EventBus 구독 설정
        EventBus<ItemEventType>.Subscribe<IEnumerable<Item>>(ItemEventType.UpdateHealPackStates, OnUpdateHealPackStates);
        EventBus<ItemEventType>.Subscribe<IEnumerable<Item>>(ItemEventType.UpdateMagicItemStates, OnUpdateMagicItemStates);
    }

    private void OnUpdateHealPackStates(IEnumerable<Item> items)
    {
        Debug.Log($"Updating HealPacks: {items?.Count() ?? 0} items.");
        UpdateItems(items, ItemType.HealPack);
    }

    private void OnUpdateMagicItemStates(IEnumerable<Item> items)
    {
        Debug.Log($"Updating MagicItems: {items?.Count() ?? 0} items.");
        UpdateItems(items, ItemType.MagicItem);
    }

    private void UpdateItems(IEnumerable<Item> items, ItemType itemType)
    {
        if (items == null) return;

        HashSet<string> activeItemIds = new HashSet<string>();
        Dictionary<string, GameObject> targetDictionary = GetTargetDictionary(itemType);

        foreach (var item in items)
        {
            activeItemIds.Add(item.ItemId);

            if (!targetDictionary.TryGetValue(item.ItemId, out GameObject itemObject))
            {

                GameObject itemPrefab = LoadPrefab(itemType);

                if (itemPrefab != null)
                {
                    itemObject = Instantiate(itemPrefab, item.Position(), Quaternion.identity);
                    targetDictionary[item.ItemId] = itemObject;
                    Debug.Log($"{itemType} created with ID: {item.ItemId}");
                }
                else
                {
                    Debug.LogError($"{itemType} prefab could not be loaded.");
                    continue;
                }
            }

            // Update the position of the item
            itemObject.transform.position = item.Position();
        }

        // Remove inactive items
        RemoveInactiveItems(activeItemIds, targetDictionary);
    }

    private GameObject LoadPrefab(ItemType itemType)
    {
        string path = itemType switch
        {
            ItemType.HealPack => "Prefabs/Item_HealPack",
            ItemType.MagicItem => "Prefabs/Item_StoneMagic",
            _ => null
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

    private Dictionary<string, GameObject> GetTargetDictionary(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.HealPack => healPackObjects,
            ItemType.MagicItem => magicItemObjects,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), $"Unsupported item type: {itemType}")
        };
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
        EventBus<ItemEventType>.Unsubscribe<IEnumerable<Item>>(ItemEventType.UpdateHealPackStates, OnUpdateHealPackStates);
        EventBus<ItemEventType>.Unsubscribe<IEnumerable<Item>>(ItemEventType.UpdateMagicItemStates, OnUpdateMagicItemStates);
    }
}