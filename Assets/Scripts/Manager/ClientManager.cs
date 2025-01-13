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
    public int ClientId { get; private set; }
    public string CurrentPlayerName
    {
        get
        {
            return "MyPlayer";
        }
    }
    public float X;
    public float Y;
    public int Hp;
    public MagicType MagicType;
    public float Angle;
    public int DashCoolTime;
    public int BulletGage;

    public int minPlayerNum;
    public int totalPlayerNum;

    // DeadState Datat
    public int placement;
    public int playerCount;
    public int killCount;

    public bool gameEnd = false;

    public void SetClientId(int clientId)
    {
        this.ClientId = clientId;
    }

    public void SetMinPlayerNumber(int minPlayerNum)
    {
        this.minPlayerNum = minPlayerNum;
    }

    public void SetTotalPlayerNumber(int totalPlayerNum)
    {
        // Debug.Log("총 플레이어 수 세팅!!!");
        this.totalPlayerNum = totalPlayerNum;
    }

    public bool CanDoDash()
    {
        return DashCoolTime == 0;
    }

    public void SetState(Wrapper wrapper)
    {
        switch (wrapper.MessageTypeCase)
        {
            case Wrapper.MessageTypeOneofCase.GameStart:
                if (wrapper.GameStart != null)
                {
                    totalPlayerNum = wrapper.GameStart.TotalPlayerNum;
                }
                break;

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
                            BulletGage = player.BulletGage;
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

                if (wrapper.GameState.ChangingState.PlayerDeadState != null)
                {
                    foreach (var player in wrapper.GameState.ChangingState.PlayerDeadState)
                    {
                        if (player.DeadId == ClientId)
                        {
                            placement = player.Placement;
                            killCount = player.KillNum;
                        }
                    }
                }
                break;

            default:
                break;
        }
    }

    public bool IsCreatedPlayer()
    {
        GameObject myPlayer = GameObject.Find("MyPlayer");

        return myPlayer != null; // 생성 됐으면 true, 아니면 false
    }
}