namespace LanguageReader.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a requested resource cannot be found.
/// </summary>
public class NotFoundException(string message) : Exception(message);

