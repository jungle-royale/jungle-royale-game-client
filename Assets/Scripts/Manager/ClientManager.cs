using System;
using UnityEngine;
using NativeWebSocket;
using System.Web;
using UnityEngine.Networking;
using System.Collections;
using Message;
using UnityEngine.Timeline;
using Unity.VisualScripting;
using System.Collections.Generic;

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

    // GameInit, GameReconnect
    public int minPlayerNum;
    public int totalPlayerNum;
    public int bulletMaxTick;
    public float bulletSpeed;

    // DeadState Datat
    public int placement;
    public int playerCount;
    public int killCount;

    public bool gameEnd = false;

    public int maxBulletGage;

    public Dictionary<int, string> currentUsersDictionary = new Dictionary<int, string>();

    public void SetClientId(int clientId)
    {
        this.ClientId = clientId;
    }

    public void SetCurrentUsersDictionary(int userId, string userName)
    {
        this.currentUsersDictionary[userId] = userName;
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

    public void SetBulletMaxTick(int bulletMaxTick)
    {
        this.bulletMaxTick = bulletMaxTick;
    }

    public void SetBulletSpeed(float bulletSpeed)
    {
        this.bulletSpeed = bulletSpeed;
    }


    public void SetMaxBulletGage(int bulletGage)
    {
        this.maxBulletGage = bulletGage;
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
                                    MagicType = MagicType.Stone;
                                    break;
                                case 2:
                                    MagicType = MagicType.Fire;
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