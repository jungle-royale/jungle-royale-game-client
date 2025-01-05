using Message;

public class HitBulletState
{
    public string bulletId;
    public string PlayerId;

    public HitBulletState(string bulletId, string PlayerId)
    {
        this.bulletId = bulletId;
        this.PlayerId = PlayerId;
    }

    public override string ToString()
    {
        return $"Player {PlayerId} was killed by Bullet {bulletId}.";
    }
}