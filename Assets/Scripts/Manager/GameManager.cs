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


    // Start is called before the first frame update
    void Start()
    {
        _sessionStartTime = DateTime.Now;
        Debug.Log("게임 시작 : " + _sessionStartTime);

        ConfigureInput();
        ConfigureNetwork();

        GameObject mapPrefab = Resources.Load<GameObject>($"Prefabs/Map");
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

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
        InputManager.Bullet += (clientId, x, y, angle) =>
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
                    case Wrapper.MessageTypeOneofCase.GameState:
                        HandleGameState(wrapper.GameState);
                        break;

                    case Wrapper.MessageTypeOneofCase.GameCount:
                        Debug.Log($"game play in: {wrapper.GameCount.Count}");
                        break;

                    case Wrapper.MessageTypeOneofCase.GameInit:
                        HandleGameInit(wrapper.GameInit);
                        break;

                    case Wrapper.MessageTypeOneofCase.GameStart:
                        HandleGameStart(wrapper.GameStart);
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
        EventBus<PlayerEventType>.Publish(PlayerEventType.InitPlayer, new PlayerInit(init.Id));
        EventBus<MainCameraEventType>.Publish(MainCameraEventType.MainCameraInit, new MainCameraInit(init.Id));
        InputManager.Instance.ConfigureClientId(init.Id);
    }

    private void HandleGameStart(GameStart gameStart)
    {
        Debug.Log(gameStart.MapLength);
        EventBus<MapEventType>.Publish(MapEventType.UpdateMapState, new Map(gameStart.MapLength, gameStart.MapLength));
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
            List<Bullet> bulletStateIds = new List<Bullet>();

            foreach (var bulletState in gameState.BulletState)
            {
                bulletStateIds.Add(new Bullet(bulletState.BulletId, bulletState.X, bulletState.Y));
            }

            EventBus<BulletEventType>.Publish(BulletEventType.UpdateBulletStates, bulletStateIds);
        }

        if (gameState.HealPackState != null)
        {
            Debug.Log($"HealPackState: {gameState.HealPackState}");
            List<HealPack> healpackStateIds = new List<HealPack>();

            foreach (var healpackState in gameState.HealPackState)
            {
                healpackStateIds.Add(new HealPack(healpackState.ItemId, healpackState.X, healpackState.Y));
            }

            EventBus<ItemEventType>.Publish(ItemEventType.UpdateHealPackStates, healpackStateIds);
        }

        if (gameState.MagicItemState != null)
        {
            Debug.Log($"MagicItemState: {gameState.MagicItemState}");
            List<MagicItem> magicitemStateIds = new List<MagicItem>();

            foreach (var magicitemState in gameState.MagicItemState)
            {
                magicitemStateIds.Add(new MagicItem(magicitemState.ItemId, magicitemState.MagicType, magicitemState.X, magicitemState.Y));
            }

            EventBus<ItemEventType>.Publish(ItemEventType.UpdateMagicItemStates, magicitemStateIds);
        }
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

