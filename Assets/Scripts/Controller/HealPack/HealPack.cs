using System;
using UnityEngine;

public class HealPack
{

    public string ItemId { get; private set; }
    public float X { get; private set; }
    public float Y { get; private set; }


    public HealPack(string itemId, float x, float y)
    {
        this.ItemId = itemId;
        this.X = x;
        this.Y = y;
    }

    public virtual Vector3 Position()
    {
        return new Vector3(this.X, 0, this.Y);
    }

    // Scale 메서드: 기본값 2f를 가지는 매개변수화된 함수
    public virtual Vector3 Scale(float scale = 2f)
    {
        return new Vector3(scale, 0, scale);
    }

}

