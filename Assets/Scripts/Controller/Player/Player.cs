public class Player
{
    public string Id;
    public float X;
    public float Y;
    public int health;
    public int magicType;

    public Player(string playerId, float x, float y, int health, int magictype)
    {
        this.Id = playerId;
        this.X = x;
        this.Y = y;
        this.health = health;
        this.magicType = magictype;
    }
}