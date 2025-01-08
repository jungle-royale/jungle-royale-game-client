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


    private bool IsDebug()
    {
        return Debug.isDebugBuild || !Application.absoluteURL.Contains("eternalsnowman.com");
    }


    void Start()
    {
        Host = GetHost();
        PathAndQuery = GetSocketPathAndQuery();
        UrlString = "ws://" + Host + PathAndQuery;

        InitializeAndConnect();

        if (Debug.isDebugBuild)
        {
            InvokeRepeating(nameof(SendHttpPing), 1f, 10f);
        }
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
            new RedirectHandler().RedirectToFailure(1);
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
    }

    private void OnError(string errorMsg)
    {
        Debug.Log($"Error! {errorMsg}");
    }

    private void OnClose(WebSocketCloseCode OnClose)
    {
        Debug.Log("Connection closed!");
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
        tileManager.DeleteReadyTile();
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState.ChangingState != null)
        {
            List<HitBulletState> HitBulletStateList = new List<HitBulletState>();
            List<GetItemState> GetItemStateList = new List<GetItemState>();
            List<PlayerDeadState> PlayerDeadStateList = new List<PlayerDeadState>();

            if (gameState.ChangingState.HitBulletState != null && gameState.ChangingState.HitBulletState.Count > 0)
            {
                foreach (var HitBulletState in gameState.ChangingState.HitBulletState)
                {
                    HitBulletStateList.Add(new HitBulletState(HitBulletState.ObjectType, HitBulletState.BulletId, HitBulletState.ObjectId, HitBulletState.X, HitBulletState.Y, HitBulletState.BulletType));
                }
            }

            if (gameState.ChangingState.GetItemState != null && gameState.ChangingState.GetItemState.Count > 0)
            {

                foreach (var GetItemState in gameState.ChangingState.GetItemState)
                {
                    GetItemStateList.Add(new GetItemState(GetItemState.ItemId, GetItemState.PlayerId, GetItemState.ItemType));
                }
            }

            if (gameState.ChangingState.PlayerDeadState != null && gameState.ChangingState.PlayerDeadState.Count > 0)
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

        if (gameState.BulletState != null && gameState.BulletState.Count > 0)
        {
            List<Bullet> bulletStateList = new List<Bullet>();

            foreach (var bulletState in gameState.BulletState)
            {
                bulletStateList.Add(new Bullet(bulletState.BulletId, bulletState.X, bulletState.Y));
            }

            bulletManager.UpdateBullets(bulletStateList);
        }

        if (gameState.HealPackState != null)
        {
            List<HealPack> healpackStateList = new List<HealPack>();

            foreach (var healpackState in gameState.HealPackState)
            {
                healpackStateList.Add(new HealPack(healpackState.ItemId, healpackState.X, healpackState.Y));
            }

            healPackManager.UpdateHealPackList(healpackStateList);
        }

        if (gameState.MagicItemState != null && gameState.MagicItemState.Count > 0)
        {
            List<Magic> magicitemStateList = new List<Magic>();

            foreach (var magicitemState in gameState.MagicItemState)
            {
                magicitemStateList.Add(new Magic(magicitemState.ItemId, magicitemState.MagicType, magicitemState.X, magicitemState.Y));
            }

            magicManager.UpdateMagicList(magicitemStateList);
        }

        if (gameState.TileState != null && gameState.TileState.Count > 0)
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
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateTimerLabel, gameState.LastSec);
        }

        if (gameState.PlayerState != null && gameState.PlayerState.Count > 0)
        {
            List<Player> playerStateList = new List<Player>();

            foreach (var player in gameState.PlayerState)
            {
                Player newPlayer = new Player(
                    player.Id, player.X, player.Y, player.Health, player.MagicType, player.Angle, player.DashCoolTime, player.IsMoved, player.IsDashing, player.IsShooting
                );
                playerStateList.Add(newPlayer);
            }
            playerManager.UpdatePlayers(playerStateList);
            cameraManager.UpdateCamera(playerStateList);
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
            new RedirectHandler().RedirectToFailure(1);
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

    private void SendHttpPing()
    {
        StartCoroutine(SendHttpPingCoroutine());
    }

    private IEnumerator SendHttpPingCoroutine()
    {
        // 요청 시작 시간 기록
        requestStartTime = DateTime.Now;

        using (UnityWebRequest request = UnityWebRequest.Get("http://" + Host + "/ping"))
        {
            yield return request.SendWebRequest(); // 요청 보내기

            // 요청 완료 시간
            DateTime requestEndTime = DateTime.Now;
            long latency = (long)(requestEndTime - requestStartTime).TotalMilliseconds; // 레이턴시 계산

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"HTTP Ping success. RTT: {latency} ms");

                // EventBus를 통해 레이턴시 전달
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePingLabel, latency);
            }
            else
            {
                Debug.LogError($"HTTP Ping failed: {request.error}");
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
            return "/room?roomId=test&clientId=test";
        }
        else
        {
            var url = Application.absoluteURL;
            try
            {
                Uri uri = new Uri(url);
                NameValueCollection queryParameters = HttpUtility.ParseQueryString(uri.Query);
                string roomId = queryParameters["roomId"];
                string clientId = queryParameters["clientId"];
                return $"/room?roomId={roomId}&clientId={clientId}";
            }
            catch (UriFormatException e)
            {
                Debug.LogError($"Invalid URL format: {e.Message}");
                return "";
            }
        }
    }
}
