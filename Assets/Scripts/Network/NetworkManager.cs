using System;
using UnityEngine;
using NativeWebSocket;

public class NetworkManager : MonoBehaviour
{
    private WebSocket websocket;

    public void Initialize(string url)
    {
        Debug.Log($"Initializing WebSocket with URL: {url}");
        websocket = new WebSocket(url);
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

    public void RegisterHandler(
        Action onOpen,
        Action<string> onError,
        Action<string> onClose,
        Action<byte[]> onMessage
    )
    {
        if (websocket == null)
        {
            Debug.LogError("WebSocket is not initialized.");
            return;
        }

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            onOpen();
        };

        websocket.OnError += (e) =>
        {
            Debug.Log($"Error! {e}");
            onError(e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            onClose(e.ToString());
        };

        websocket.OnMessage += (bytes) =>
        {
            onMessage(bytes);
        };
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
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }
}