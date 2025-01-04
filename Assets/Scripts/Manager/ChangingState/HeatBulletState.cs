using Message;

public class HeatBulletState
{
    public string bulletId;
    public string PlayerId;

    public HeatBulletState(string bulletId, string PlayerId)
    {
        this.bulletId = bulletId;
        this.PlayerId = PlayerId;
    }

    public override string ToString()
    {
        return $"Player {PlayerId} was killed by Bullet {bulletId}.";
    }
}