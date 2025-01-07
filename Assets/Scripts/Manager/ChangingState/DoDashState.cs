using System;
using Message;

public class DoDashState
{
    public string PlayerId;
    public bool Dash;

    public DoDashState(string playerId, bool dash)
    {
        this.PlayerId = playerId;
        this.Dash = dash;
    }

}