using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Collections;
using Message;
using Google.Protobuf;
using System.Collections.Specialized;
using System.Web;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class GameNetworkManager : Singleton<GameNetworkManager>
{
    private WebSocket websocket;

    private DateTime requestStartTime;

    public ChangingStateManager changingStateManager;
    public GameStateManager gameStateManager;
    public TileManager tileManager;
    public BulletManager bulletManager;
    public MagicManager magicManager;
    public HealPackManager healPackManager;
    public PlayerManager playerManager;
    public CameraManager cameraManager;

    private string Host;

    private string PathAndQuery;

    private string UrlString;
    private string PingUrlString;


    private bool IsDebug()
    {
        return Debug.isDebugBuild || !Application.absoluteURL.Contains("eternalsnowman.com");
    }


    void Start()
    {
        Host = GetHost();
        PathAndQuery = GetSocketPathAndQuery();
        UrlString = "ws://" + Host + PathAndQuery;
        PingUrlString = "http://" + Host + "/ping";

        Debug.Log("------------");
        Debug.Log(UrlString);
        Debug.Log(PingUrlString);
        Debug.Log("------------");

        InitializeAndConnect();

        InvokeRepeating(nameof(SendHttpPing), 1f, 10f);
    }

    public void Update()
    {
        // NativeWebsocket에서는 이 코드가 필요합니다.
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private void OnApplicationQuit()
    {
        Close();
    }
    public async void InitializeAndConnect()
    {
        Debug.Log($"Initializing WebSocket with URL: {UrlString}");
        websocket = new WebSocket(UrlString);

        websocket.OnOpen += onOpen;
        websocket.OnMessage += OnMessage;
        websocket.OnClose += OnClose;
        websocket.OnError += OnError;

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect WebSocket: {ex.Message}");
            // new RedirectHandler().RedirectToFailure(1);
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "ErrorCanvas");
        }
    }


    public async void Close()
    {
        if (websocket == null)
        {
            Debug.LogWarning("WebSocket is not initialized.");
            return;
        }

        if (websocket.State == WebSocketState.Closed || websocket.State == WebSocketState.Closing)
        {
            Debug.LogWarning("WebSocket is already closed or closing.");
            return;
        }

        try
        {
            await websocket.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to close WebSocket: {ex.Message}");
        }
    }

    private void onOpen()
    {
        Debug.Log($"Connection open! : {UrlString}");
        EventBus<InputButtonEventType>.Publish(InputButtonEventType.CompleteConnect);
    }

    private void OnError(string errorMsg)
    {
        Debug.Log($"Error! {errorMsg}");
        // EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "ErrorCanvas");
    }

    private void OnClose(WebSocketCloseCode OnClose)
    {
        Debug.Log($"Connection Close! : {UrlString}");
    }

    private void OnMessage(byte[] data)
    {
        try
        {
            var wrapper = Wrapper.Parser.ParseFrom(data);

            ClientManager.Instance.SetState(wrapper);

            switch (wrapper.MessageTypeCase)
            {
                case Wrapper.MessageTypeOneofCase.GameState:
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

                case Wrapper.MessageTypeOneofCase.GameReconnect:
                    HandleGameReconnect(wrapper.GameReconnect);
                    break;

                case Wrapper.MessageTypeOneofCase.NewUser:
                    HandleNewUser(wrapper.NewUser);
                    break;

                default:
                    Debug.Log($"Unknown message type received: {wrapper.MessageTypeCase}");
                    break;
            }
        }
        catch (InvalidProtocolBufferException ex)
        {
            Debug.LogError($"Protobuf parsing error: {ex.Message}");
            Debug.Log($"Raw bytes: {BitConverter.ToString(data)}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}");
        }
    }

    private void HandleGameInit(GameInit init)
    {
        ClientManager.Instance.SetClientId(init.Id);
        ClientManager.Instance.SetMinPlayerNumber(init.MinPlayerNum);
        ClientManager.Instance.SetBulletMaxTick(init.BulletMaxTick);
        ClientManager.Instance.SetBulletSpeed(init.BulletSpeed);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateMinPlayerLabel, init.MinPlayerNum);
        new LoadingScreenRemover().Remove();
    }

    private void HandleGameCount(GameCount count)
    {
        // TODO: GameManager에게 전달
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameCountDown, 1.0f);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateGameCountDownLabel, count.Count);
    }

    private void HandleGameStart(GameStart gameStart)
    {
        gameStateManager.HandleGameStart(gameStart);
        ClientManager.Instance.SetTotalPlayerNumber(gameStart.TotalPlayerNum);
        tileManager.DeleteReadyTile();
    }

    private void HandleGameReconnect(GameReconnect gameReconnect)
    {
        ClientManager.Instance.SetClientId(gameReconnect.Id);
        ClientManager.Instance.SetMinPlayerNumber(gameReconnect.MinPlayerNum);
        ClientManager.Instance.SetTotalPlayerNumber(gameReconnect.TotalPlayerNum);
        ClientManager.Instance.SetBulletMaxTick(gameReconnect.BulletMaxTick);
        ClientManager.Instance.SetBulletSpeed(gameReconnect.BulletSpeed);
        // EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateMinPlayerLabel, gameReconnect.MinPlayerNum);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePlayerCountLabel, gameReconnect.TotalPlayerNum);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameStart");
        new LoadingScreenRemover().Remove();
    }

    private void HandleNewUser(NewUser newUser)
    {
        foreach (var currentPlayer in newUser.CurrentPlayers)
        {
            int userId = currentPlayer.PlayerId;
            string userName = currentPlayer.PlayerName;
            playerManager.SetCurrentUsersDictionary(userId, userName);
        }
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState.ChangingState != null)
        {
            List<HitBulletState> HitBulletStateList = new List<HitBulletState>();
            List<GetItemState> GetItemStateList = new List<GetItemState>();
            List<PlayerDeadState> PlayerDeadStateList = new List<PlayerDeadState>();

            if (gameState.ChangingState.HitBulletState != null)
            {
                foreach (var HitBulletState in gameState.ChangingState.HitBulletState)
                {
                    HitBulletStateList.Add(new HitBulletState(HitBulletState.ObjectType, HitBulletState.BulletId, HitBulletState.ObjectId, HitBulletState.X, HitBulletState.Y, HitBulletState.BulletType));
                }
            }

            if (gameState.ChangingState.GetItemState != null)
            {

                foreach (var GetItemState in gameState.ChangingState.GetItemState)
                {
                    GetItemStateList.Add(new GetItemState(GetItemState.ItemId, GetItemState.PlayerId, GetItemState.ItemType));
                }
            }

            if (gameState.ChangingState.PlayerDeadState != null)
            {

                foreach (var PlayerDeadState in gameState.ChangingState.PlayerDeadState)
                {
                    PlayerDeadStateList.Add(new PlayerDeadState(
                        PlayerDeadState.KillerId, PlayerDeadState.DeadId, PlayerDeadState.DyingStatus, PlayerDeadState.KillNum, PlayerDeadState.Placement
                    ));
                }
            }

            changingStateManager.UpdateState(HitBulletStateList, GetItemStateList, PlayerDeadStateList);
            gameStateManager.HandleGameEndState(PlayerDeadStateList);
        }

        if (gameState.BulletState != null)
        {
            List<Bullet> bulletStateList = new List<Bullet>();

            foreach (var bulletState in gameState.BulletState)
            {
                if (!cameraManager.IsInMinimapCameraView(new Vector3(bulletState.X, 0, bulletState.Y)))
                    continue;
                // Debug.Log($"BulletState: ({bulletState.X}, {bulletState.Y})");
                bulletStateList.Add(new Bullet(bulletState.BulletId, bulletState.BulletType, bulletState.X, bulletState.Y));
            }

            bulletManager.UpdateBullets(bulletStateList);
        }

        if (gameState.HealPackState != null)
        {
            // Debug.Log($"HealPackState: {gameState.HealPackState}");
            List<HealPack> healpackStateList = new List<HealPack>();

            foreach (var healpackState in gameState.HealPackState)
            {
                healpackStateList.Add(new HealPack(healpackState.ItemId, healpackState.X, healpackState.Y));
            }

            healPackManager.UpdateHealPackList(healpackStateList);
        }

        if (gameState.MagicItemState != null)
        {
            // Debug.Log($"MagicItemState: {gameState.MagicItemState}");
            List<Magic> magicitemStateList = new List<Magic>();

            foreach (var magicitemState in gameState.MagicItemState)
            {
                magicitemStateList.Add(new Magic(magicitemState.ItemId, magicitemState.MagicType, magicitemState.X, magicitemState.Y));
            }

            magicManager.UpdateMagicList(magicitemStateList);
        }

        if (gameState.TileState != null)
        {
            List<Tile> tileStateList = new List<Tile>();

            foreach (var tileState in gameState.TileState)
            {
                tileStateList.Add(new Tile(tileState.TileId, tileState.X, tileState.Y, tileState.TileState_, tileState.TileType));
            }

            tileManager.UpdateTiles(tileStateList);
        }

        if (gameState.LastSec >= 0)
        {
            // Debug.Log($"Time: {gameState.LastSec}");
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateTimerLabel, gameState.LastSec);
        }

        if (gameState.PlayerState != null && gameState.PlayerState.Count > 0)
        {
            List<Player> playerStateList = new List<Player>();

            foreach (var player in gameState.PlayerState)
            {
                Player newPlayer = new Player(
                    player.Id,
                    player.X, player.Y,
                    player.Dx, player.Dy,
                    player.Health, player.MagicType,
                    player.Angle, player.DashCoolTime,
                    player.IsMoved, player.IsDashing,
                    player.IsShooting, player.BulletGage,
                    cameraManager.IsInMinimapCameraView(new Vector3(player.X, 0, player.Y))
                );
                playerStateList.Add(newPlayer);
            }
            playerManager.UpdatePlayersFromServer(playerStateList);
            cameraManager.UpdateCameraFromServer(playerStateList);
        }

    }

    public bool IsOpen()
    {
        return websocket != null && websocket.State == WebSocketState.Open;
    }

    public void Send(byte[] data)
    {
        if (!IsOpen())
        {
            Debug.LogWarning("WebSocket is not open. Cannot send data.");
            // EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "ErrorCanvas");
            return;
        }

        try
        {
            websocket.Send(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send data: {ex.Message}");
        }
    }

    public async void SendHttpPing()
    {
        long latency = await SendHttpPingAsync();
    }

    private async Task<long> SendHttpPingAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        using (UnityWebRequest request = UnityWebRequest.Get(PingUrlString))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield(); // 대기
            }

            stopwatch.Stop();

            if (request.result == UnityWebRequest.Result.Success)
            {
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePingLabel, stopwatch.ElapsedMilliseconds);
                return stopwatch.ElapsedMilliseconds;
            }
            else
            {
                return -1;
            }
        }
    }

    private string GetHost()
    {
        if (IsDebug())
        {
            try
            {
                var url = Application.absoluteURL;
                Uri uri = new Uri(url);
                return $"{uri.Host}:8000";
            }
            catch (UriFormatException e)
            {
                Debug.LogError($"Invalid URL format: {e.Message}");
                return "localhost:8000";
            }
        }
        else
        {
            return "game-api.eternalsnowman.com:8080";
        }
    }

    private string GetSocketPathAndQuery()
    {
        if (IsDebug())
        {
            // EventBus<InputButtonEventType>.Publish(InputButtonEventType.Observer);
            // return "/observer?roomId=test";
            return "/room?roomId=room0&clientId=test";
        }
        else
        {
            var url = Application.absoluteURL;
            try
            {
                Uri uri = new Uri(url);
                NameValueCollection queryParameters = HttpUtility.ParseQueryString(uri.Query);

                string pathAndQuery = uri.PathAndQuery;

                if (pathAndQuery.StartsWith("/room", StringComparison.OrdinalIgnoreCase))
                {
                    string roomId = queryParameters["roomId"];
                    string clientId = queryParameters["clientId"];
                    string userName = queryParameters["username"];
                    return $"/room?roomId={roomId}&clientId={clientId}&username={userName}";
                }
                else if (pathAndQuery.StartsWith("/observer", StringComparison.OrdinalIgnoreCase))
                {
                    string roomId = queryParameters["roomId"];
                    EventBus<InputButtonEventType>.Publish(InputButtonEventType.Observer);
                    return $"/observer?roomId={roomId}";
                }

                throw new Exception("없는 경로");
            }
            catch (UriFormatException e)
            {
                Debug.LogError($"Invalid URL format: {e.Message}");
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "ErrorCanvas");
                return "";
            }
        }
    }
}
