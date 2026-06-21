namespace LanguageReader.Infrastructure.Exceptions;

/// <summary>
/// Exception for application workflow failures.
/// </summary>
public class ApplicationException(string message) : Exception(message);

