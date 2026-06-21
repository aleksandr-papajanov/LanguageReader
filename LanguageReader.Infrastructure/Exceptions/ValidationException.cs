namespace LanguageReader.Infrastructure.Exceptions;

/// <summary>
/// Exception for invalid user input.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Creates a validation exception with a message.
    /// </summary>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Creates a validation exception with field errors.
    /// </summary>
    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Field-level validation errors.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

