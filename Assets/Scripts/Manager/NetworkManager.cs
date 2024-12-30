using System;
using UnityEngine;
using NativeWebSocket;
using System.Web;
    
public class NetworkManager : Singleton<NetworkManager>
{
    private WebSocket websocket;

    public static event Action OnOpen;
    public static event Action<string> OnError;
    public static event Action<string> OnClose;
    public static event Action<byte[]> OnMessage;

    void Start()
    {
        Initialize();
        RegisterHandler();
    }

    public void Initialize()
    {
        var roomId = "test";
        if (!Debug.isDebugBuild)
        {
            var url = Application.absoluteURL;
            Uri uri = new Uri(url);
            string query = uri.Query;
            var queryParams = HttpUtility.ParseQueryString(query);
            roomId = queryParams["roomId"];
        }
        var urlString = $"ws://localhost:8000/room?roomId={roomId}";
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
}