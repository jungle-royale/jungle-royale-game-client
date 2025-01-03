using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Collections;
using UnityEditor.PackageManager;
using Message;
using Google.Protobuf;

public class GameNetworkManager : Singleton<GameNetworkManager>
{
    private WebSocket websocket;

    private String host;
    private String urlString;
    private DateTime requestStartTime;

    private int serverPort = 8000;


    public GameStateManager gameStateManager;
    public PlayerManager playerManager;
    public TileManager tileManager;
    public BulletManager bulletManager;
    public MagicManager magicManager;
    public HealPackManager healPackManager;


    void Start()
    {
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
        Debug.Log($"💩 Init Network");

        if (!Debug.isDebugBuild)
        {

            // TODO: 서버 배포 후에는 Host를 DomainName으로 바꿔야 함

            var url = Application.absoluteURL;

            Debug.LogError($"💩 Try: {url}");

            try
            {
                Uri uri = new Uri(url);
                host = uri.Host; // 호스트 영역 추출

                // 포트를 8000으로 설정
                UriBuilder uriBuilder = new UriBuilder(uri)
                {
                    Port = serverPort // 포트를 8000으로 설정
                };

                // URL 변경
                url = uriBuilder.ToString();
            }
            catch (UriFormatException e)
            {
                Debug.LogError($"Invalid URL format: {e.Message}");
            }

            if (url.StartsWith("https://"))
            {
                url = url.Replace("https://", "wss://");
            }
            else if (url.StartsWith("http://"))
            {
                url = url.Replace("http://", "ws://");
            }

            urlString = url; // 실제 웹앱에서 넘어올 때에는 경로와 쿼리스트링이 같이 넘어온다.
        }
        else
        {
            urlString = $"ws://localhost:{serverPort}/room?roomId=test&clientId=test";
            host = $"localhost:{serverPort}";
        }
        Debug.Log($"Initializing WebSocket with URL: {urlString}");
        websocket = new WebSocket(urlString);

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
        Debug.Log($"Connection open! : {urlString}");
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

            Debug.Log("💩" + wrapper.ToString());

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
    }

    private void HandleGameCount(GameCount count)
    {
        // TODO: GameManager에게 전달
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameCountDown, 1.0f);
        // EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateGameCountDownLabel, count.Count);
    }

    private void HandleGameStart(GameStart gameStart)
    {
        gameStateManager.HandleGameStart(gameStart);
    }

    private void HandleGameState(GameState gameState)
    {
        gameStateManager.HandleGameState(gameState);

        if (gameState.PlayerState != null && gameState.PlayerState.Count > 0)
        {
            List<Player> playerStateList = new List<Player>();

            foreach (var player in gameState.PlayerState)
            {
                playerStateList.Add(new Player(player.Id, player.X, player.Y, player.Health, player.MagicType, player.Angle, player.DashCoolTime));
            }
            playerManager.UpdatePlayers(playerStateList);
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

        if (gameState.PlayerDeadState != null && gameState.PlayerDeadState.Count > 0)
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

        if (gameState.TileState != null && gameState.TileState.Count > 0)
        {
            List<Tile> tileStateList = new List<Tile>();

            foreach (var tileState in gameState.TileState)
            {
                tileStateList.Add(new Tile(tileState.TileId, tileState.X, tileState.Y));
            }

            tileManager.UpdateTiles(tileStateList);
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

        using (UnityWebRequest request = UnityWebRequest.Get("http://" + host + "/ping"))
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
}
