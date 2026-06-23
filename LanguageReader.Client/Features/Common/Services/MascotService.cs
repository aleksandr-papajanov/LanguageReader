namespace LanguageReader.Client.Features.Common.Services;

public sealed class MascotService
{
    private long actionVersion;

    public MascotState CurrentState { get; private set; } = MascotState.Idle;

    public MascotAction CurrentAction { get; private set; } = MascotAction.None;

    public MascotState? ActionFinalState { get; private set; }

    public long ActionVersion => actionVersion;

    public event Action? Changed;

    public void SetState(MascotState state)
    {
        CurrentState = state;
        CurrentAction = MascotAction.None;
        ActionFinalState = null;
        NotifyChanged();
    }

    public void PlayAction(MascotAction action, MascotState? finalState = null)
    {
        if (action == MascotAction.None)
        {
            SetState(finalState ?? CurrentState);
            return;
        }

        CurrentAction = action;
        ActionFinalState = finalState;
        actionVersion++;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
