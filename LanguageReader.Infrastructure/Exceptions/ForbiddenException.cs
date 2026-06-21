namespace LanguageReader.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when the current user is not allowed to access a resource.
/// </summary>
public class ForbiddenException(string message) : Exception(message);

