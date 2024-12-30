using System.Numerics;

public class HealPack
{
    public string ItemId;
    public float X;
    public float Y;

    const float ITEM_SCALE = 2f;

    public HealPack(string itmeId, float x, float y)
    {
        this.ItemId = itmeId;
        this.X = x;
        this.Y = y;
    }

    public Vector3 Position()
    {
        return new Vector3(this.X, 0, this.Y);
    }

    public Vector3 Scale()
    {
        return new Vector3(ITEM_SCALE, 0, ITEM_SCALE);
    }
}