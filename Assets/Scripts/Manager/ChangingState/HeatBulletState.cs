using System;
using Message;
using UnityEngine;

public class HitBulletState
{
    public int ObjectType; // 0: Player, 4: environment object
    public int BulletId;
    public int ObjectId;
    public float X;
    public float Y;
    public int BulletType;

    public HitBulletState(int objectType, int bulletId, int ObjectId, float x, float y, int bulletType)
    {
        this.ObjectType = objectType;
        this.BulletId = bulletId;
        this.ObjectId = ObjectId;
        this.X = x;
        this.Y = y;
        this.BulletType = bulletType;
    }

    public override string ToString()
    {
        return $"Player {ObjectId} was killed by Bullet {BulletId}.";
    }

    public bool IsPlayer()
    {
        // Debug.Log($"ObjectType: {this.ObjectType}");
        return ObjectType == 0;
    }
}