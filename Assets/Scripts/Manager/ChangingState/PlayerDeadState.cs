using Message;

public class PlayerDeadState
{
    public string killerId;
    public string deadPlayerId;
    public int dyingStatus;
    // 1: snow
    // 2: stone
    // 3: fire
    // 4: fall

    public PlayerDeadState(string killerId, string deadPlayerId, int dyingStatus)
    {
        this.killerId = killerId;
        this.deadPlayerId = deadPlayerId;
        this.dyingStatus = dyingStatus;
    }

    public override string ToString()
    {
        return $"Player {deadPlayerId} was killed by {killerId} due to {dyingStatus}.";
    }

    public bool IsSnow()
    {
        return dyingStatus == 1;
    }

    public bool IsStone()
    {
        return dyingStatus == 2;
    }

    public bool IsFire()
    {
        return dyingStatus == 3;
    }

    public bool IsFall()
    {
        return dyingStatus == 4;
    }
}