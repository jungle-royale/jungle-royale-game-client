using Message;

public class Player
{
    public string id;
    public float x;
    public float y;
    public int health;
    public int magicType;
    public float angle;
    public int dashCoolTime;

    public Player(string playerId, float x, float y, int health, int magictype, float angle, int dashCoolTime)
    {
        this.id = playerId;
        this.x = x;
        this.y = y;
        this.health = health;
        this.magicType = magictype;
        this.angle = angle;
        this.dashCoolTime = dashCoolTime;
    }
}