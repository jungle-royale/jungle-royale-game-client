using UnityEngine.Rendering;

public class Bullet
{
    public int BulletId;
    public int BulletType;
    public float X;
    public float Y;

    public Bullet(int bulletId, int bulletType, float x, float y)
    {
        this.BulletId = bulletId;
        this.BulletType = bulletType;
        this.X = x;
        this.Y = y;
    }
}