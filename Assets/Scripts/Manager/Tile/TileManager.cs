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
                GameObject tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile01");
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
        // tileObject.transform.localScale = tile.Scale();
        tileObject.transform.position = tile.Position();

        Debug.Log($"TileId: {tile.tileId}, IsWaring: {tile.warning}");
        if (tile.warning == 1)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f); // 0~1 사이의 값 반복
            UpdateGroundColors(tileObject, t);
        }
    }

    private void UpdateGroundColors(GameObject tileObject, float lerpFactor)
    {
        Transform groundsTransform = tileObject.transform.Find("Grounds");
        if (groundsTransform != null)
        {
            foreach (Transform child in groundsTransform)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 기존 Material을 복사하여 독립적인 Material 생성
                    if (!renderer.material.name.Contains("(Instance)")) // 이미 인스턴스화된 Material인지 확인
                    {
                        renderer.material = new Material(renderer.material);
                    }

                    // 인스턴스화된 Material의 색상 변경
                    renderer.material.color = Color.Lerp(baseColor, Color.magenta, lerpFactor);
                }
                else
                {
                    Debug.LogWarning($"Renderer not found on child: {child.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Grounds 오브젝트 없음");
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