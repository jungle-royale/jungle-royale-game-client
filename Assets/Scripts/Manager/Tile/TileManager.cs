using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public PlayerManager playerManager;
    private Dictionary<int, GameObject> tileObjects = new Dictionary<int, GameObject>();

    private Debouncer debouncer = new Debouncer();

    private WebGLHapticManager HaptickManager = new WebGLHapticManager();

    const float TILE_Y = 0;
    private const float blinkSpeed = 1.5f;
    private Color baseColor = new Color(1f, 1f, 1f); // 흰색

    public Color warningColor;

    private void Awake()
    {
        // PlayerManager를 찾거나 연결
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found in the scene.");
        }
    }

    public void UpdateTiles(List<Tile> tiles)
    {
        HashSet<int> activeTileIds = new HashSet<int>();

        foreach (var tile in tiles)
        {
            activeTileIds.Add(tile.tileId);

            if (!tileObjects.TryGetValue(tile.tileId, out GameObject tileObject))
            {
                GameObject tilePrefab = null;

#if UNITY_EDITOR
                tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile04");
#else
            // 빌드된 환경에서 실행 중일 때
            if (Debug.isDebugBuild)
            {
                Debug.Log("[TileManager.cs] Development Build에서 실행 중");
                tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile04");
            }
            else
            {
                // Release Build에서 실행 중
                switch (tile.tileType)
                {
                    case 0:
                        tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile00");
                        break;

                    case 1:
                        tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile01");
                        break;

                    case 2:
                        tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile02");
                        break;

                    case 3:
                        tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile03");
                        break;

                    default:
                        Debug.LogError($"Unknown tile type: {tile.tileType}");
                        break;
                }
            }
#endif
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
        var NewPosition = tile.Position();
        NewPosition.y = tileObject.transform.position.y;
        tileObject.transform.position = NewPosition;

        if (tile.warning == 1)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f); // 0~1 사이의 값 반복
            UpdateGroundColors(tileObject, t);
            UpdatePlayerHaptick(tileObject);
        }
    }

    private void UpdatePlayerHaptick(GameObject tileObject)
    {
        // PlayerManager에서 플레이어 객체 가져오기
        GameObject player = playerManager.GetPlayerById(ClientManager.Instance.ClientId);
        if (player == null)
        {
            return;
        }

        // "Ground"라는 이름의 자식 객체 찾기
        Transform groundTransform = tileObject.transform.Find("Ground");

        if (groundTransform != null)
        {
            // Ground의 Scale 가져오기
            Vector3 groundScale = groundTransform.localScale;
        }
        else
        {
            Debug.LogError("Ground object not found under the tileObject!");
        }

        Vector3 playerPosition = player.transform.position;

        Vector3 tilePosition = tileObject.transform.position;
        Vector3 tileScale = groundTransform.localScale;

        float tileMinX = tilePosition.x - (tileScale.x / 2f);
        float tileMaxX = tilePosition.x + (tileScale.x / 2f);
        float tileMinZ = tilePosition.z - (tileScale.z / 2f);
        float tileMaxZ = tilePosition.z + (tileScale.z / 2f);

        bool isOnTile = playerPosition.x >= tileMinX && playerPosition.x <= tileMaxX &&
                        playerPosition.z >= tileMinZ && playerPosition.z <= tileMaxZ;

        if (isOnTile)
        {
            debouncer.Debounce(200, () =>
            {
                HaptickManager.TriggerHaptic(200);
            });
        }
    }

    private void UpdateGroundColors(GameObject tileObject, float lerpFactor)
    {
        Transform groundsTransform = tileObject.transform.Find("Ground");
        if (groundsTransform != null)
        {

            Renderer renderer = groundsTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 기존 Material을 복사하여 독립적인 Material 생성
                if (!renderer.material.name.Contains("(Instance)")) // 이미 인스턴스화된 Material인지 확인
                {
                    renderer.material = new Material(renderer.material);
                }

                if (warningColor == null)
                    warningColor = new Color(255f, 130f, 130f); // 세미빨강

                renderer.material.color = Color.Lerp(baseColor, warningColor, lerpFactor);
            }
            else
            {
                Debug.LogWarning($"Renderer not found on groundsTransform: {groundsTransform.name}");
            }
        }

        else
        {
            Debug.LogWarning("Grounds 오브젝트 없음");
        }
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

        // 애니메이션 실행
        // 애니메이션 끝나면 event에서 destroy 처리

        foreach (var tileId in tilesToRemove)
        {
            if (tileObjects.TryGetValue(tileId, out GameObject tileObject))
            {
                tileObjects.Remove(tileId);
                Animator animator = tileObject.GetComponent<Animator>();
                if (animator == null)
                {
                    return;
                }
                animator.SetTrigger("bye");
            }
        }
    }

    public void DeleteReadyTile()
    {
        List<int> keys = new List<int>(tileObjects.Keys);

        foreach (var key in keys)
        {
            if (tileObjects.TryGetValue(key, out GameObject tileObject))
            {
                tileObjects.Remove(key);
                Destroy(tileObject);
            }
        }
    }
}