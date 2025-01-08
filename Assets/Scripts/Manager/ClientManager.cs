using System;
using UnityEngine;
using NativeWebSocket;
using System.Web;
using UnityEngine.Networking;
using System.Collections;
using Message;
using UnityEngine.Timeline;

public class ClientManager : Singleton<ClientManager>
{
    public string ClientId { get; private set; }
    public string CurrentPlayerName {
        get {
            return "MyPlayer";
        }
    }
    public float X;
    public float Y;
    public int Hp;
    public MagicType MagicType;
    public float Angle;
    public int DashCoolTime;

    public void SetClientId(string clientId)
    {
        this.ClientId = clientId;
    }

    public bool CanDoDash()
    {
        return DashCoolTime == 0;
    }

    public void SetState(Wrapper wrapper)
    {
        switch (wrapper.MessageTypeCase)
        {
            case Wrapper.MessageTypeOneofCase.GameState:
                if (wrapper.GameState.PlayerState != null)
                {
                    foreach (var player in wrapper.GameState.PlayerState)
                    {
                        if (player.Id == ClientId)
                        {
                            X = player.X;
                            Y = player.Y;
                            Hp = player.Health;
                            DashCoolTime = player.DashCoolTime;
                            Angle = player.Angle;
                            switch (player.MagicType)
                            {
                                case 1:
                                    MagicType = MagicType.Fire;
                                    break;
                                case 2:
                                    MagicType = MagicType.Stone;
                                    break;
                                default:
                                    MagicType = MagicType.None;
                                    break;
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

}