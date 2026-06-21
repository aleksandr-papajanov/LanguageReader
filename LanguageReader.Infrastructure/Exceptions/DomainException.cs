namespace LanguageReader.Infrastructure.Exceptions;

/// <summary>
/// Base exception for domain rule violations.
/// </summary>
public class DomainException(string message) : Exception(message);

