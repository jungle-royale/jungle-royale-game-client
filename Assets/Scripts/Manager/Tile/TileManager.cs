using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public CameraManager cameraManager;
    private Dictionary<int, GameObject> tileObjects = new Dictionary<int, GameObject>();

    private Debouncer debouncer = new Debouncer();

    private WebGLHapticManager HaptickManager = new WebGLHapticManager();

    const float TILE_Y = 0;
    private const float blinkSpeed = 1.5f;

    public Color warningColor = new Color(255 / 255f, 130 / 255f, 130 / 255f); // 세미빨강
    private Color baseColor = new Color(1f, 1f, 1f); // 흰색

    public Material crackedMaterial;

    private void Awake()
    {
        // PlayerManager를 찾거나 연결
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found in the scene.");
        }

        // CameraManager를 찾거나 연결
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager not found in the scene.");
        }

        warningColor = new Color(255 / 255f, 130 / 255f, 130 / 255f); // 세미빨강
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

                // #if UNITY_EDITOR
                //                 tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile04");
                // #else
                //                                 // 빌드된 환경에서 실행 중일 때
                //                                 if (Debug.isDebugBuild)
                //                                 {
                //                                     Debug.Log("[TileManager.cs] Development Build에서 실행 중");
                //                                     tilePrefab = Resources.Load<GameObject>("Prefabs/Tiles/Tile04");
                //                                 }
                //                                 else
                //                                 {
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
                // }
                // #endif
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

        if (tile.warning == 1)
        {
            var shakeMagnitude = 0.1f;
            Vector3 shakeOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude,
                UnityEngine.Random.Range(-2f, 0f) * shakeMagnitude, // 1f를 0으로
                UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude
            );

            var NewPosition = tile.Position();
            tileObject.transform.position = NewPosition + shakeOffset;
        }
        else
        {
            var NewPosition = tile.Position();
            tileObject.transform.position = NewPosition;
        }


        if (tile.warning == 1)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f); // 0~1 사이의 값 반복
            // UpdateGroundColors(tileObject, t);
            OnCrackEffect(tileObject);
            UpdatePlayerHaptick(tileObject);
        }
    }


    private void OnCrackEffect(GameObject tileObject)
    {
        Transform groundTransform = tileObject.transform.Find("Ground");
        if (groundTransform == null)
        {
            Debug.LogError("Ground 없음");
            return;
        }

        Transform crackEffect = groundTransform.transform.Find("TileCrackEffect");
        if (crackEffect != null)
        {
            Debug.Log("crackEffect 실행");
            crackEffect.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("crackEffect 없음");
            return;
        }
    }

    private void UpdatePlayerHaptick(GameObject tileObject)
    {
        Vector3? targetPosition = cameraManager.GetFocusedPosition();
        if (targetPosition == null)
        {
            return;
        }

        Vector3 tilePosition = tileObject.transform.position;

        if (IsOnTile(tileObject, targetPosition.GetValueOrDefault()))
        {
            debouncer.Debounce(200, () =>
            {
                HaptickManager.TriggerHaptic(200);
            });

            cameraManager.StartCameraShake(2.0f, 0.2f); // 2초, 0.2강도
        }
        else
        {
            cameraManager.StopCameraShake();
        }
    }

    private bool IsOnTile(GameObject tileObject, Vector3 targetPosition)
    {
        // "Ground"라는 이름의 자식 객체 찾기
        Transform groundTransform = tileObject.transform.Find("Ground");

        Vector3 tilePosition = tileObject.transform.position;
        Vector3 tileScale = groundTransform.localScale;

        float tileMinX = tilePosition.x - (tileScale.x / 2f);
        float tileMaxX = tilePosition.x + (tileScale.x / 2f);
        float tileMinZ = tilePosition.z - (tileScale.z / 2f);
        float tileMaxZ = tilePosition.z + (tileScale.z / 2f);

        bool isOnTile = targetPosition.x >= tileMinX && targetPosition.x <= tileMaxX &&
                        targetPosition.z >= tileMinZ && targetPosition.z <= tileMaxZ;
        return isOnTile;
    }

    private void UpdateGroundColors(GameObject tileObject, float lerpFactor)
    {
        Transform groundsTransform = tileObject.transform.Find("Ground");
        if (groundsTransform != null)
        {

            Renderer renderer = groundsTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 기존 Material 배열 가져오기
                Material[] currentMaterials = renderer.materials;

                // CrackedGround Material이 이미 추가되었는지 확인
                bool hasCrackedMaterial = false;
                foreach (Material mat in currentMaterials)
                {
                    if (mat.name.Contains(crackedMaterial.name))
                    {
                        hasCrackedMaterial = true;
                        break;
                    }
                }

                // CrackedGround Material이 없을 경우 한 번만 추가
                if (!hasCrackedMaterial)
                {
                    Material[] newMaterials = new Material[currentMaterials.Length + 1];
                    for (int i = 0; i < currentMaterials.Length; i++)
                    {
                        // 기존 Material을 복사하여 독립적인 Material 생성
                        if (!currentMaterials[i].name.Contains("(Instance)"))
                        {
                            newMaterials[i] = new Material(currentMaterials[i]);
                        }
                        else
                        {
                            newMaterials[i] = currentMaterials[i];
                        }
                    }

                    // CrackedGround Material 추가
                    newMaterials[newMaterials.Length - 1] = crackedMaterial;

                    // 새 Material 배열 설정
                    renderer.materials = newMaterials;
                }

                // 첫 번째 Material의 색상 변경 (복사된 Material)
                renderer.materials[0].color = Color.Lerp(baseColor, warningColor, lerpFactor);
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