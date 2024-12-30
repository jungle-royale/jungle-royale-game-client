using System.Collections.Generic;
using Message;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Dictionary<string, GameObject> healpackObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        EventBus<ItemEventType>.Subscribe<IEnumerable<HealPack>>(ItemEventType.UpdateHealPackStates, UpdateHealPacks);
    }

    private void UpdateHealPacks(IEnumerable<HealPack> healpacks)
    {
        HashSet<string> activeHealPackIds = new HashSet<string>();

        foreach (var healpack in healpacks)
        {
            activeHealPackIds.Add(healpack.ItemId);

            if (!healpackObjects.TryGetValue(healpack.ItemId, out GameObject healpackObject))
            {
                GameObject healpackPrefab = Resources.Load<GameObject>("Prefabs/HealPack");
                if (healpackPrefab != null)
                {
                    healpackObject = Instantiate(healpackPrefab, new Vector3(healpack.X, 0, healpack.Y), Quaternion.identity);

                    healpackObjects[healpack.ItemId] = healpackObject;
                }
                else
                {
                    Debug.LogError("healpack prefab could not be loaded.");
                    return;
                }
            }

            healpackObject.transform.position = new Vector3(healpack.X, 0, healpack.Y);
        }

        RemoveInactiveHealPacks(activeHealPackIds);
    }

    private void RemoveInactiveHealPacks(HashSet<string> activeHealPackIds)
    {
        List<string> healpacksToRemove = new List<string>();

        foreach (var healpackId in healpackObjects.Keys)
        {
            if (!activeHealPackIds.Contains(healpackId))
            {
                healpacksToRemove.Add(healpackId);
            }
        }

        foreach (var healpackId in healpacksToRemove)
        {
            if (healpackObjects.TryGetValue(healpackId, out GameObject bullet))
            {
                Destroy(bullet);
                healpackObjects.Remove(healpackId);
            }
        }
    }

    private void OnDestroy()
    {
        EventBus<ItemEventType>.Unsubscribe<IEnumerable<HealPack>>(ItemEventType.UpdateHealPackStates, UpdateHealPacks);
    }
}