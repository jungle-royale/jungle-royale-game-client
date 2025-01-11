using Message;

public class GetItemState
{
    public int itemId;
    public int playerId;

    public int itemType;

    public GetItemState(int itemId, int playerId, int itemType)
    {
        this.itemId = itemId;
        this.playerId = playerId;
        this.itemType = itemType;
    }

    public override string ToString()
    {
        return $"Player {playerId} was Get Item {itemType} {itemId}.";
    }
}