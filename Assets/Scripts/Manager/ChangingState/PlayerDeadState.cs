using Message;

public class PlayerDeadState
{
    public string killerId;
    public string deadPlayerId;
    public int dyingStatus;

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
}