namespace LanguageReader.Client.Features.Common.Services;

public enum MascotState
{
    Idle,
    Reading,
    Learning,
    Sleeping
}

public enum MascotAction
{
    None,
    OpenBook,
    TurnPage
}
