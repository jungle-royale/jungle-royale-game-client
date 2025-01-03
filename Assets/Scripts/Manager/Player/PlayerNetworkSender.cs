using UnityEngine;
using NativeWebSocket;
using Message;
using Google.Protobuf;
using System;

public class PlayerNetworkSender : MonoBehaviour
{
    public GameNetworkManager gameNetworkManager;

    public void SendDoDash(bool dash)
    {
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
            gameNetworkManager.Send(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }

    public void SendChangeAngleMessage(float angle)
    {
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
            gameNetworkManager.Send(data);
            // Debug.Log($"Sent movement: angle={angle}, isMoved={isMoved}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }
    
    public void SendChangeDirMessage(float angle, bool isMoved)
    {
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
            gameNetworkManager.Send(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }

}