using System;
using UnityEngine;
using NativeWebSocket;
using System.Web;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManager : Singleton<NetworkManager>
{
    private WebSocket websocket;

    public static event Action OnOpen;
    public static event Action<string> OnError;
    public static event Action<string> OnClose;
    public static event Action<byte[]> OnMessage;

    private String host;
    private String urlString;
    private DateTime requestStartTime;

    void Start()
    {
        Initialize();
        RegisterHandler();

        if (Debug.isDebugBuild)
        {
            InvokeRepeating(nameof(SendHttpPing), 1f, 1f);
        }
    }

    public void Initialize()
    {
        if (!Debug.isDebugBuild)
        {
            var url = Application.absoluteURL;

            try
            {
                Uri uri = new Uri(url);
                host = uri.Host; // 호스트 영역 추출
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
            urlString = url;
        }
        else
        {
            urlString = $"ws://localhost:8000/room?roomId=test";
            host = "localhost:8000";
        }
        Debug.Log($"Initializing WebSocket with URL: {urlString}");
        websocket = new WebSocket(urlString);
    }

    public async void Connect()
    {
        if (websocket == null)
        {
            Debug.LogError("WebSocket is not initialized.");
            return;
        }

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect WebSocket: {ex.Message}");
        }
    }

    private void RegisterHandler()
    {
        if (websocket == null)
        {
            Debug.LogError("WebSocket is not initialized.");
            return;
        }

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            OnOpen?.Invoke();
        };

        websocket.OnError += (e) =>
        {
            Debug.Log($"Error! {e}");
            OnError?.Invoke(e);
        };

        websocket.OnClose += (code) =>
        {
            Debug.Log("Connection closed!");
            OnClose?.Invoke(code.ToString());
        };

        websocket.OnMessage += (bytes) =>
        {
            OnMessage?.Invoke(bytes);
        };
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
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.PingUpdated, latency);
            }
            else
            {
                Debug.LogError($"HTTP Ping failed: {request.error}");
            }
        }
    }
}