using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private Dictionary<string, GameObject> tileObjects = new Dictionary<string, GameObject>();
    const float TILE_Y = 0;

    private const float blinkSpeed = 1.5f;

    private Color baseColor = new Color(142 / 255f, 183 / 255f, 180 / 255f);

    public void UpdateTiles(List<Tile> tiles)
    {
        HashSet<string> activeTileIds = new HashSet<string>();

        foreach (var tile in tiles)
        {
            activeTileIds.Add(tile.tileId);

            if (!tileObjects.TryGetValue(tile.tileId, out GameObject tileObject))
            {
                GameObject tilePrefab = Resources.Load<GameObject>("Prefabs/Tile");
                if (tilePrefab != null)
                {
                    tileObject = Instantiate(tilePrefab, tile.Position(), Quaternion.identity);
                    tileObjects[tile.tileId] = tileObject;
                }
                else
                {
                    Debug.LogError("Tile prefab could not be loaded.");
                    return;
                }
            }

            UpdateTile(tile, tileObject);
        }
        RemoveInactiveTiles(activeTileIds);
    }

    private void UpdateTile(Tile tile, GameObject tileObject)
    {
        tileObject.transform.localScale = tile.Scale();
        tileObject.transform.position = tile.Position();
        if(tile.warning == 1) {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f); // 0~1 사이의 값 반복
            tileObject.GetComponent<Renderer>().material.color = Color.Lerp(baseColor, Color.magenta, t
            );
        }
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