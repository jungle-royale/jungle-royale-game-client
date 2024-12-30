public class MainCameraInit 
{
    public string ClientId { get; }

    public MainCameraInit(string playerId)
    {
        this.ClientId = playerId;
    }
}