using Message;

public class HitBulletState
{
    public int ObjectType; // 0: Player, 4: environment object
    public string BulletId;
    public string ObjectId;
    public float X;
    public float Y;

    public HitBulletState(int objectType, string bulletId, string ObjectId, float x, float y)
    {
        this.ObjectType = objectType;
        this.BulletId = bulletId;
        this.ObjectId = ObjectId;
        this.X = x;
        this.Y = y;
    }

    public override string ToString()
    {
        return $"Player {ObjectId} was killed by Bullet {BulletId}.";
    }

    public bool IsPlayer()
    {
        return ObjectType == 0;
    }
}