using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private Dictionary<string, GameObject> tileObjects = new Dictionary<string, GameObject>();
    const float TILE_Y = 0;


    public void UpdateTiles(List<Tile> tiles)
    {
        HashSet<string> activeTileIds = new HashSet<string>();

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

    private void RemoveInactiveTiles(HashSet<string> activeTileIds)
    {
        List<string> tilesToRemove = new List<string>();

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

}