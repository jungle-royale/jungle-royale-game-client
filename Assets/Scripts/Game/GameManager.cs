using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;
using Google.Protobuf;
using Unity.VisualScripting;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using TMPro;

public class GameManager : MonoBehaviour
{
    private NetworkManager networkManager;

    // 카메라
    private Camera mainCamera;
    const float CAMERA_ROTATION_X = 40f;
    const float CAMERA_OFFSET_Y = 10f;
    const float CAMERA_OFFSET_Z = 10f;

    // 플레이어 데이터 관리
    private Dictionary<string, GameObject> playerObjectList = new Dictionary<string, GameObject>();
    private string clientId;
    float PLAYER_Y;
    float BULLET_Y;

    // 총알
    private Dictionary<string, GameObject> bulletObjectList = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        ConfigureInput();
        ConfigureNetwork();

        GameObject mapPrefab = Resources.Load<GameObject>($"Prefabs/Map");
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

        PLAYER_Y = CalculateSurfaceY(mapPrefab);
        BULLET_Y = PLAYER_Y + 0.9f;

        mainCamera = Camera.main;

        // AudioManager를 통해 BackgroundBGM 재생
        AudioManager.Instance.PlayBGM("BackgroundBGM");
    }

    public void ConfigureInput()
    {
        InputManager.Dash += (dash) =>
        {
            SendDoDashMessage(dash);
        };
        InputManager.Move += (angle, isMoved) =>
        {
            SendChangeDirMessage(angle, isMoved);

            if (isMoved)
            {
                // start audio
                AudioManager.Instance.StartWalkingSound("RunningSFX");
            }
            else
            {
                // stopch audio
                AudioManager.Instance.StopWalkingSound();
            }

        };
        InputManager.Bullet += (x, y, angle) =>
        {
            SendCreateBulletMessage(clientId, x, y, angle);
        };
    }

    public void ConfigureNetwork()
    {
        networkManager = FindObjectOfType<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene!");
            return;
        }

        NetworkManager.OnOpen += () => { };
        NetworkManager.OnClose += (error) => { };
        NetworkManager.OnError += (closeCode) => { };
        NetworkManager.OnMessage += (bytes) =>
        {
            try
            {
                var wrapper = Wrapper.Parser.ParseFrom(bytes);

                switch (wrapper.MessageTypeCase)
                {
                    case Wrapper.MessageTypeOneofCase.GameInit:
                        HandleGameInit(wrapper.GameInit);
                        break;

                    case Wrapper.MessageTypeOneofCase.GameCount:
                        Debug.Log($"game play in: {wrapper.GameCount.Count}");
                        break;
                    case Wrapper.MessageTypeOneofCase.GameState:
                        HandleGameState(wrapper.GameState);
                        break;


                    default:
                        Debug.Log($"Unknown message type received: {wrapper.MessageTypeCase}");
                        break;
                }
            }
            catch (InvalidProtocolBufferException ex)
            {
                Debug.LogError($"Protobuf parsing error: {ex.Message}");
                Debug.Log($"Raw bytes: {BitConverter.ToString(bytes)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
            }
        };

        // WebSocket 연결 시도
        networkManager.Connect();
    }

    private void OnApplicationQuit()
    {
        networkManager.Close();
    }

    void Update()
    {
    }

    private void HandleGameInit(GameInit init)
    {
        clientId = init.Id;
        Debug.Log($"Assigned Client ID: {clientId}");
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState.PlayerState != null)
        {
            UpdatePlayers(gameState.PlayerState);
        }

        if (gameState.BulletState != null)
        {
            UpdateAllBullets(gameState.BulletState);
        }
    }

    private void UpdateAllBullets(IEnumerable<BulletState> bulletStates)
    {
        // 서버에서 받은 총알 ID 저장
        HashSet<string> bulletStateIds = new HashSet<string>();

        // 서버에서 받은 총알 상태를 순회
        foreach (var bulletState in bulletStates)
        {
            bulletStateIds.Add(bulletState.BulletId);
            UpdateBulletPosition(bulletState);
        }

        // Dictionary에서 서버에 없는 총알 제거
        RemoveInactiveBullets(bulletStateIds);
    }

    // GameState를 기반으로 플레이어 관리
    private void UpdatePlayers(IEnumerable<PlayerState> players)
    {
        HashSet<string> playerStateIds = new HashSet<string>();

        // 서버에서 받은 players 리스트를 순회하며 업데이트
        foreach (var player in players)
        {
            playerStateIds.Add(player.Id);
            UpdatePlayerPosition(player);
        }

        // 서버에 없는 플레이어 제거
        RemoveInactivePlayers(playerStateIds);
    }

    private void UpdatePlayerPosition(PlayerState playerState)
    {
        GameObject playerObject;

        // 플레이어가 Dictionary에 없으면 새로 생성
        if (!playerObjectList.TryGetValue(playerState.Id, out playerObject))
        {
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
            if (playerPrefab != null)
            {
                playerObject = Instantiate(playerPrefab, new Vector3(playerState.X, PLAYER_Y, playerState.Y), Quaternion.identity);

                if (playerState.Id == clientId) // 내 플레이어면 태그를 Player로 설정
                {
                    playerObject.tag = "Player";
                    Debug.Log($"Created client player for ID: {playerState.Id}");
                }

                // Dictionary에 추가
                playerObjectList[playerState.Id] = playerObject;
            }
            else
            {
                Debug.LogError("Player prefab could not be loaded.");
            }
        }

        // 위치 업데이트
        playerObject.transform.position = new Vector3(playerState.X, PLAYER_Y, playerState.Y);

        // 카메라 세팅 (내 플레이어일 경우에만)
        if (playerState.Id == clientId)
        {
            mainCamera.transform.position = new Vector3(playerState.X, PLAYER_Y + CAMERA_OFFSET_Y, playerState.Y - CAMERA_OFFSET_Z);
            mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
        }
    }

    private void RemoveInactivePlayers(HashSet<string> playerStateIds)
    {
        List<string> playersToRemove = new List<string>();

        // Dictionary에서 서버에 없는 플레이어 찾기
        foreach (var playerId in playerObjectList.Keys)
        {
            if (!playerStateIds.Contains(playerId))
            {
                playersToRemove.Add(playerId);
            }
        }

        // Dictionary에서 제거 및 GameObject 파괴
        foreach (var playerId in playersToRemove)
        {
            if (playerObjectList.TryGetValue(playerId, out GameObject player))
            {
                Destroy(player);
                playerObjectList.Remove(playerId);
                Debug.Log($"Removed player with ID: {playerId}");
            }
        }
    }

    private void UpdateBulletPosition(BulletState bullet)
    {
        GameObject firedBullet;

        // 총알이 Dictionary에 없으면 새로 생성
        if (!bulletObjectList.TryGetValue(bullet.BulletId, out firedBullet))
        {
            GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
            if (bulletPrefab != null)
            {
                firedBullet = Instantiate(bulletPrefab, new Vector3(bullet.X, BULLET_Y, bullet.Y), Quaternion.identity);
                firedBullet.tag = "Bullet";

                // Dictionary에 추가
                bulletObjectList[bullet.BulletId] = firedBullet;
                // Debug.Log($"발사: {bullet.BulletId}");
            }
            else
            {
                Debug.LogError("Bullet prefab could not be loaded.");
            }
        }

        firedBullet.transform.position = new Vector3(bullet.X, BULLET_Y, bullet.Y);
        // Debug.Log($"총알 위치: {firedBullet.transform.position}");
    }

    private void RemoveInactiveBullets(HashSet<string> bulletStateIds)
    {
        // 서버에서 제공되지 않은 총알 ID를 제거
        List<string> bulletsToRemove = new List<string>();

        foreach (var bulletId in bulletObjectList.Keys)
        {
            if (!bulletStateIds.Contains(bulletId))
            {
                bulletsToRemove.Add(bulletId);
            }
        }

        // Dictionary에서 제거하고 GameObject 파괴
        foreach (var bulletId in bulletsToRemove)
        {
            if (bulletObjectList.TryGetValue(bulletId, out GameObject bullet))
            {
                Destroy(bullet);
                bulletObjectList.Remove(bulletId);
                // Debug.Log($"총알 제거: {bulletId}");
            }
        }
    }

    private float CalculateSurfaceY(GameObject mapPrefab)
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map or Player prefab is not assigned or could not be loaded.");
            return 0f;
        }

        // // Map의 Scale에서 높이 계산
        float mapHeight = mapPrefab.transform.localScale.y / 2;

        return mapHeight;
    }

    private void SendChangeDirMessage(float angle, bool isMoved)
    {
        if (networkManager == null || !networkManager.IsOpen())
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        // DirChange 메시지 생성
        var changeDir = new ChangeDir
        {
            Angle = angle,
            IsMoved = isMoved
        };

        // Wrapper 메시지 생성 및 DirChange 메시지 포함
        var wrapper = new Wrapper
        {
            ChangeDir = changeDir
        };

        // Protobuf 직렬화
        var data = wrapper.ToByteArray();

        try
        {
            // WebSocket으로 메시지 전송
            networkManager.Send(data);
            // Debug.Log($"Sent movement: angle={angle}, isMoved={isMoved}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }

    private void SendDoDashMessage(bool dash)
    {
        if (networkManager == null || !networkManager.IsOpen())
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        var doDash = new DoDash
        {
            Dash = dash
        };

        var wrapper = new Wrapper
        {
            DoDash = doDash
        };

        var data = wrapper.ToByteArray();

        try
        {
            networkManager.Send(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }

    private void SendCreateBulletMessage(string playerId, float startX, float startY, float angle)
    {
        if (networkManager == null || !networkManager.IsOpen())
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        try
        {
            // CreateBullet 메시지 생성
            var createBullet = new CreateBullet
            {
                PlayerId = playerId,
                StartX = startX,
                StartY = startY,
                Angle = angle
            };

            // Wrapper 메시지 생성 및 CreateBullet 메시지 포함
            var wrapper = new Wrapper
            {
                CreateBullet = createBullet
            };

            // Protobuf 직렬화
            var data = wrapper.ToByteArray();

            // WebSocket으로 메시지 전송
            networkManager.Send(data);

            // Debug.Log($"Sent CreateBullet: PlayerId={playerId}, StartX={startX}, StartY={startY}, Angle={angle}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send CreateBullet message: {ex.Message}");
        }
    }
}

