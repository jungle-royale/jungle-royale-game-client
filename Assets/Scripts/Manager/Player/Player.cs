using System;
using Message;
using UnityEngine;

public class Player
{
    public int id;
    public float x;
    public float y;
    public int health;
    public int magicType;
    public float angle;
    public int dashCoolTime;
    public bool isMoved;
    public bool isDashing;
    public bool isShooting;
    public float dx;
    public float dy;
    public int bulletGage;

    public Player(
        int playerId,
        float x,
        float y,
        int health,
        int magictype,
        float angle,
        int dashCoolTime,
        bool isMoved,
        bool isDashing,
        bool isShooting,
        float dx,
        float dy,
        int bulletGage
    )
    {
        this.id = playerId;
        this.x = x;
        this.y = y;
        this.health = health;
        this.magicType = magictype;
        this.angle = angle;
        this.dashCoolTime = dashCoolTime;
        this.isMoved = isMoved;
        this.isDashing = isDashing;
        this.isShooting = isShooting;
        this.dx = dx;
        this.dy = dy;
        this.bulletGage = bulletGage;
    }

    public Vector3 NewPosition(float playerY)
    {
        return new Vector3(x, playerY, y);
    }

    // 대시

    // 총알 나갈 때 

    // 피격 - 죽엇을 때
    // hp가 이전보다 작아졌을 때
    // hp가 0이 아닐 때

    // 이동

    // 죽음
    // PlayerList에서 사라졌을 때 

    // 힐
    // HP가 올라갔을 때 

    // get item

}