using System;
using Message;

public class DoDashState
{
    public int PlayerId;
    public bool Dash;

    public DoDashState(int playerId, bool dash)
    {
        this.PlayerId = playerId;
        this.Dash = dash;
    }

}