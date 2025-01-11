using Message;

public class PlayerDeadState
{
    public int killerId;
    public int deadPlayerId;
    public int dyingStatus;
    // 0: 안죽음
    // 1: snow
    // 2: stone
    // 3: fire
    // 4: fall

    public int killNum;
    public int placement; // rank

    public PlayerDeadState(int killerId, int deadPlayerId, int dyingStatus, int killNum, int placement)
    {
        this.killerId = killerId;
        this.deadPlayerId = deadPlayerId;
        this.dyingStatus = dyingStatus;
        this.killNum = killNum;
        this.placement = placement;
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

    public bool IsWinner()
    {
        return placement == 1;
    }

    public bool IsEndGame()
    {
        return placement == 1;
    }
}