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
using UnityEngine.Tilemaps;

public class GameManager : Singleton<GameManager>
{

    private NetworkManager networkManager;

    private DateTime _sessionStartTime;

    private bool _gameStart = false;

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
        AudioManager.Instance.PlayBGM("WaitingRoomBGM");
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
        InputManager.Instance.Dash += (dash) =>
        {
            SendDoDashMessage(dash);
        };
        InputManager.Instance.Move += (angle, isMoved) =>
        {
            SendChangeDirMessage(angle, isMoved);

            if (isMoved)
            {
                // start audio
                AudioManager.Instance.StartWalkingSound();
            }
            else
            {
                // stopch audio
                AudioManager.Instance.StopWalkingSound();
            }

        };
        InputManager.Instance.Bullet += (clientId, x, y, angle) =>
        {
            SendCreateBulletMessage(clientId, x, y, angle);
        };
        InputManager.Instance.Direction += (angle) =>
        {
            SendChangeAngleMessage(angle);
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

                ClientManager.Instance.SetState(wrapper);

                switch (wrapper.MessageTypeCase)
                {
                    case Wrapper.MessageTypeOneofCase.GameState:
                        // Debug.Log(wrapper.GameState);
                        HandleGameState(wrapper.GameState);
                        break;

                    case Wrapper.MessageTypeOneofCase.GameCount:
                        Debug.Log($"game play in: {wrapper.GameCount.Count}");
                        HandleGameCount(wrapper.GameCount);
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
        ClientManager.Instance.SetClientId(init.Id);
        InputManager.Instance.ConfigureClientId(init.Id);
    }

    private void HandleGameCount(GameCount count)
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameCountDown, 1.0f);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateGameCountDownLabel, count.Count);
    }

    private void HandleGameStart(GameStart gameStart)
    {
        // Debug.Log(gameStart.MapLength);
        _gameStart = true;
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameStart);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameStart");
        AudioManager.Instance.PlayBGM("InGameBGM");
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState.PlayerState != null)
        {

            // 게임 시작 했는데 플레이어가 혼자면
            if (_gameStart && gameState.PlayerState.Count == 1)
            {
                // 승리

            }
            // Debug.Log($"PlayerState: {gameState.PlayerState.Count}");
            List<Player> playerStateList = new List<Player>();
            List<MainCamera> mainCameraPlayerStateList = new List<MainCamera>();

            foreach (var player in gameState.PlayerState)
            {
                playerStateList.Add(new Player(player.Id, player.X, player.Y, player.Health, player.MagicType, player.Angle, player.DashCoolTime));
                mainCameraPlayerStateList.Add(new MainCamera(player.Id, player.X, player.Y));
            }
            EventBus<PlayerEventType>.Publish(PlayerEventType.UpdatePlayerStates, playerStateList);
            EventBus<MainCameraEventType>.Publish(MainCameraEventType.MainCameraState, mainCameraPlayerStateList);
        }

        if (gameState.BulletState != null)
        {
            List<Bullet> bulletStateList = new List<Bullet>();

            foreach (var bulletState in gameState.BulletState)
            {
                bulletStateList.Add(new Bullet(bulletState.BulletId, bulletState.X, bulletState.Y));
            }

            EventBus<BulletEventType>.Publish(BulletEventType.UpdateBulletStates, bulletStateList);
        }

        if (gameState.HealPackState != null)
        {
            // Debug.Log($"HealPackState: {gameState.HealPackState}");
            List<HealPack> healpackStateList = new List<HealPack>();

            foreach (var healpackState in gameState.HealPackState)
            {
                healpackStateList.Add(new HealPack(healpackState.ItemId, healpackState.X, healpackState.Y));
            }

            EventBus<HealPackEventType>.Publish(HealPackEventType.UpdateHealPackStates, healpackStateList);
        }

        if (gameState.MagicItemState != null)
        {
            // Debug.Log($"MagicItemState: {gameState.MagicItemState}");
            List<Magic> magicitemStateList = new List<Magic>();

            foreach (var magicitemState in gameState.MagicItemState)
            {
                magicitemStateList.Add(new Magic(magicitemState.ItemId, magicitemState.MagicType, magicitemState.X, magicitemState.Y));
            }

            EventBus<MagicEventType>.Publish(MagicEventType.UpdateMagicStates, magicitemStateList);
        }

        if (gameState.PlayerDeadState != null)
        {
            if (gameState.PlayerDeadState.Count > 0)
            {
                Debug.Log($"PlayerDeadState: {gameState.PlayerDeadState}");
            }

            List<PlayerDead> playerDeadStateList = new List<PlayerDead>();

            foreach (var playerDeadState in gameState.PlayerDeadState)
            {
                playerDeadStateList.Add(new PlayerDead(playerDeadState.KillerId, playerDeadState.DeadId, playerDeadState.DyingStatus));
            }
        }

        if (gameState.TileState != null)
        {
            List<Tile> tileStateList = new List<Tile>();

            foreach (var tileState in gameState.TileState)
            {
                tileStateList.Add(new Tile(tileState.TileId, tileState.X, tileState.Y));
            }

            EventBus<TileEventType>.Publish(TileEventType.UpdateTileStates, tileStateList);
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

    private void SendChangeAngleMessage(float angle)
    {
        if (networkManager == null || !networkManager.IsOpen())
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        // DirChange 메시지 생성
        var changeAngle = new ChangeAngle
        {
            Angle = angle,
        };

        // Wrapper 메시지 생성 및 DirChange 메시지 포함
        var wrapper = new Wrapper
        {
            ChangeAngle = changeAngle
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
        // Debug.Log("DoDash message 전송");
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

