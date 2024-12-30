using System;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private GameObject mapObject;

    void Start()
    {
        EventBus<MapEventType>.Subscribe<MapInit>(MapEventType.UpdateMapState, UpdateMap);
    }

    private void UpdateMap(MapInit mapState)
    {
        if (mapObject == null)
        {
            GameObject mapPrefab = Resources.Load<GameObject>("Prefabs/Map");
            if (mapPrefab != null)
            {
                mapPrefab = Instantiate(mapPrefab, mapState.Position(), Quaternion.identity);
                mapPrefab.transform.localScale = mapState.Scale();
                Debug.Log("Map instantiated.");
            }
            else
            {
                Debug.LogError("Map prefab could not be loaded.");
                return;
            }
        }

        Debug.Log($"Map updated: Width = " + mapState.toString());
    }

    private void OnDestroy()
    {
        EventBus<MapEventType>.Unsubscribe<MapInit>(MapEventType.UpdateMapState, UpdateMap);
    }
}