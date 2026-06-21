namespace LanguageReader.Client.Features.Common.Services;

public static class ApiQuery
{
    public static string Create(params (string Name, object? Value)[] parameters)
    {
        var items = parameters
            .Where(parameter => parameter.Value is not null)
            .Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Name)}={Uri.EscapeDataString(Format(parameter.Value!))}");

        var query = string.Join("&", items);
        return string.IsNullOrWhiteSpace(query) ? string.Empty : $"?{query}";
    }

    public static string Escape(string value)
    {
        return Uri.EscapeDataString(value);
    }

    private static string Format(object value)
    {
        return value switch
        {
            bool boolValue => boolValue.ToString().ToLowerInvariant(),
            DateTime dateTime => dateTime.ToString("O"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O"),
            Guid guid => guid.ToString("D"),
            _ => value.ToString() ?? string.Empty
        };
    }
}
