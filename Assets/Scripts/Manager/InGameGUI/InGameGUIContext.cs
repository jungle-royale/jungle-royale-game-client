public class InGameGUIContext
{
    public InGameGUIStateInterface CurrentState
    {
        get;set;
    }

    private readonly InGameGUIManager _inGameGUIManager;

    public InGameGUIContext(InGameGUIManager inGameGUIController)
    {
        _inGameGUIManager = inGameGUIController;
    }

    public void Transition()
    {
        CurrentState.Handle(_inGameGUIManager);
    }

    public void Transition(InGameGUIStateInterface state)
    {
        CurrentState = state;
        CurrentState.Handle(_inGameGUIManager);
    }
}