using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private Dictionary<int, GameObject> tileObjects = new Dictionary<int, GameObject>();
    const float TILE_Y = 0;

    void Start()
    {
        EventBus<TileEventType>.Subscribe<IEnumerable<Tile>>(TileEventType.UpdateTileStates, UpdateTiles);
    }

    private void UpdateTiles(IEnumerable<Tile> tiles)
    {
        HashSet<int> activeTileIds = new HashSet<int>();

        foreach (var tile in tiles)
        {
            activeTileIds.Add(tile.tileId);

            if (!tileObjects.TryGetValue(tile.tileId, out GameObject tileObejct))
            {
                GameObject tilePrefab = Resources.Load<GameObject>("Prefabs/Tile");
                if (tilePrefab != null)
                {
                    tileObejct = Instantiate(tilePrefab, tile.Position(), Quaternion.identity);
                    tileObejct.transform.localScale = tile.Scale();

                    tileObjects[tile.tileId] = tileObejct;
                }
                else
                {
                    Debug.LogError("Tile prefab could not be loaded.");
                    return;
                }
            }

            tileObejct.transform.position = tile.Position();
        }
        RemoveInactiveTiles(activeTileIds);
    }

    private void RemoveInactiveTiles(HashSet<int> activeTileIds)
    {
        List<int> tilesToRemove = new List<int>();

        foreach (var tileId in tileObjects.Keys)
        {
            if (!activeTileIds.Contains(tileId))
            {
                tilesToRemove.Add(tileId);
            }
        }

        foreach (var tileId in tilesToRemove)
        {
            if (tileObjects.TryGetValue(tileId, out GameObject player))
            {
                Destroy(player);
                tileObjects.Remove(tileId);
            }
        }
    }

    private void OnDestroy()
    {
        EventBus<TileEventType>.Unsubscribe<IEnumerable<Tile>>(TileEventType.UpdateTileStates, UpdateTiles);
    }
}