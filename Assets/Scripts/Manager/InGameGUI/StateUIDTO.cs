using System;
using UnityEngine;

public struct StateUIDTO
{
    public string nickName;
    public int placement;
    public int totalPlayer;
    public int killCount;
    // public int point;
}

public struct PlayerUIDTO
{
    public GameObject playerObj;
    public string userName;
    public int maxHealth;
    public int health;
}