namespace LanguageReader.Client.Features.Common.Services;

public sealed class AppErrorService
{
    public event Action? Changed;

    public AppErrorMessage? Current { get; private set; }

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

        Current = new AppErrorMessage("Something went wrong", message.Trim());
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (Current is null)
        {
            return;
        }

        Current = null;
        Changed?.Invoke();
    }
}

public sealed record AppErrorMessage(
    string Title,
    string Message);
