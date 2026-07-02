namespace LanguageReader.Client.Features.Common.Services;

public sealed class AppErrorService
{
    private readonly List<AppErrorMessage> messages = [];

    public event Action? Changed;

    public IReadOnlyList<AppErrorMessage> Messages => messages;

    public void Show(Exception exception)
    {
        Show(exception.Message);
    }

    public void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Something went wrong. Please retry the last action.";
        }

        messages.Insert(0, new AppErrorMessage(Guid.NewGuid(), "Something went wrong", message.Trim()));
        Changed?.Invoke();
    }

    public void Dismiss(Guid id)
    {
        var removed = messages.RemoveAll(message => message.Id == id) > 0;
        if (!removed)
        {
            return;
        }

        Changed?.Invoke();
    }

    public void Clear()
    {
        if (messages.Count == 0)
        {
            return;
        }

        messages.Clear();
        Changed?.Invoke();
    }
}

public sealed record AppErrorMessage(
    Guid Id,
    string Title,
    string Message);
