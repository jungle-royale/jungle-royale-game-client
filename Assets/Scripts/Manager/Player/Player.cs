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


    // 대시

    // 총알 나갈 때 

    // 피격 - 죽엇을 때
    // hp가 이전보다 작아졌을 때
    // hp가 0이 아닐 때

    // 이동

    // 죽음
    // PlayerList에서 사라졌을 때 

    // 힐
    // HP가 올라갔을 때 

    // get item

    // 
}