using Message;

public class GetItemState
{
    public string itemId;
    public string playerId;

    public int itemType;

    public GetItemState(string itemId, string playerId, int itemType)
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