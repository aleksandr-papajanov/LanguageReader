namespace LanguageReader.Infrastructure.Exceptions;

/// <summary>
/// Exception for infrastructure failures.
/// </summary>
public class InfrastructureException(string message, Exception? innerException = null) : Exception(message, innerException);

