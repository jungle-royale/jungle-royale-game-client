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

public class GameManager : Singleton<GameManager>
{
    private NetworkManager networkManager;

    private DateTime _sessionStartTime;

    // 카메라

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
        _sessionStartTime = DateTime.Now;
        Debug.Log("게임 시작 : " + _sessionStartTime);

        ConfigureInput();
        ConfigureNetwork();

        GameObject mapPrefab = Resources.Load<GameObject>($"Prefabs/Map");
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

        BULLET_Y = PLAYER_Y + 0.9f;
 
        // AudioManager를 통해 BackgroundBGM 재생
        AudioManager.Instance.PlayBGM("BackgroundBGM");
    }

    void Update()
    {

    }

    private void OnApplicationQuit()
    {
        var _sessionEndTime = DateTime.Now;
        networkManager.Close();

        Debug.Log("게임 종료 : " + DateTime.Now);
        Debug.Log("게임 플레이 시간 : " + _sessionEndTime.Subtract(_sessionStartTime));
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

    private void HandleGameInit(GameInit init)
    {
        EventBus<MapEventType>.Publish(MapEventType.UpdateMapState, new Map(100, 100));
        EventBus<PlayerEventType>.Publish(PlayerEventType.InitPlayer, new PlayerInit(init.Id));
        EventBus<MainCameraEventType>.Publish(MainCameraEventType.MainCameraInit, new MainCameraInit(init.Id));

    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState.PlayerState != null)
        {
            List<Player> playerList = new List<Player>();
            List<MainCamera> mainCameraPlayerList = new List<MainCamera>();

            foreach (var player in gameState.PlayerState)
            {
                playerList.Add(new Player(player.Id, player.X, player.Y, player.Health, player.MagicType));
                mainCameraPlayerList.Add(new MainCamera(player.Id, player.X, player.Y));
            }
            EventBus<PlayerEventType>.Publish(PlayerEventType.UpdatePlayerStates, playerList);
            EventBus<MainCameraEventType>.Publish(MainCameraEventType.MainCameraState, mainCameraPlayerList);
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

