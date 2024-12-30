public class MainCamera
{
    public string Id;
    public float X;
    public float Y;

    public MainCamera(string playerId, float x, float y)
    {
        this.Id = playerId;
        this.X = x;
        this.Y = y;
    }
}