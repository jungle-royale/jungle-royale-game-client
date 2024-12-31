using System;
using UnityEngine;

public class Item
{
    public string ItemId { get; private set; }
    public float X { get; private set; }
    public float Y { get; private set; }

    // 공통 필드는 부모 클래스에서 관리
    public Item(string itemId, float x, float y)
    {
        this.ItemId = itemId;
        this.X = x;
        this.Y = y;
    }

    // 위치 반환 메서드
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

public class HealPack : Item
{
    public HealPack(string itemId, float x, float y)
        : base(itemId, x, y) // 부모 클래스 생성자 호출
    {
    }

}

public class MagicItem : Item
{
    public int MagicType { get; private set; }

    public MagicItem(string itemId, int magicType, float x, float y)
        : base(itemId, x, y) // 부모 클래스 생성자 호출
    {
        this.MagicType = magicType;
    }
}