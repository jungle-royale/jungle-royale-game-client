using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using System.Collections;
using UnityEditor.PackageManager;


public class GameNetworkManager : MonoBehaviour
{

    private WebSocket websocket;


    private String host;
    private String urlString;
    private DateTime requestStartTime;

    private int serverPort = 8000;

    // public PlayerManager playerManager;
    // public BulletManager bulletManager;
    // public ItemManager itemManager;
    // public GameStateManager gameStateManager;

    void Start()
    {

        InitializeAndConnect();

        if (Debug.isDebugBuild)
        {
            InvokeRepeating(nameof(SendHttpPing), 1f, 10f);
        }

        // 웹소켓 연결
        // webSocketClient.OnMessageReceived += OnDataReceived;
        // webSocketClient.Connect("ws://your-server-address");
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
        // 서버 데이터 파싱
        // ServerData serverData = JsonUtility.FromJson<ServerData>(jsonData);

        // 게임 상태 처리
        // gameStateManager.ProcessGameState(serverData.gameState);

        // 각 매니저에 데이터 전달
        // playerManager.UpdatePlayers(serverData.players);
        // bulletManager.UpdateBullets(serverData.bullets);
        // itemManager.UpdateItems(serverData.items);
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
