public class PlayerInit
{
    public string ClientId { get; }

    public PlayerInit(string playerId)
    {
        this.ClientId = playerId;
    }
}