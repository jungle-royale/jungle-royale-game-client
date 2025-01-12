using System;
using Message;
using UnityEngine;

public class Player
{
    public int id;
    public float x;
    public float y;
    public float dx;
    public float dy;
    public int health;
    public int magicType;
    public float angle;
    public int dashCoolTime;
    public bool isMoved;
    public bool isDashing;
    public bool isShooting;
    public int bulletGage;

    public Player(
        int playerId,
        float x,
        float y,
        float dx,
        float dy,
        int health,
        int magictype,
        float angle,
        int dashCoolTime,
        bool isMoved,
        bool isDashing,
        bool isShooting,
        int bulletGage
    )
    {
        this.id = playerId;
        this.x = x;
        this.y = y;
        this.dx = dx;
        this.dy = dy;
        this.health = health;
        this.magicType = magictype;
        this.angle = angle;
        this.dashCoolTime = dashCoolTime;
        this.isMoved = isMoved;
        this.isDashing = isDashing;
        this.isShooting = isShooting;
        this.bulletGage = bulletGage;
    }

    public Vector3 NewPosition(float playerY)
    {
        return new Vector3(x, playerY, y);
    }

}