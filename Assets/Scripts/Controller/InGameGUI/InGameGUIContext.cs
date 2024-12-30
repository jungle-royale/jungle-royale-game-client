public class InGameGUIContext
{
    public InGameGUIStateInterface CurrentState
    {
        get;set;
    }

    private readonly InGameGUIController _inGameGUIController;

    public InGameGUIContext(InGameGUIController inGameGUIController)
    {
        _inGameGUIController = inGameGUIController;
    }

    public void Transition()
    {
        CurrentState.Handle(_inGameGUIController);
    }

    public void Transition(InGameGUIStateInterface state)
    {
        CurrentState = state;
        CurrentState.Handle(_inGameGUIController);
    }
}