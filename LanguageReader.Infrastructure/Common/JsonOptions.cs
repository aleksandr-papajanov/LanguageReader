using System.Text.Json;

namespace LanguageReader.Infrastructure.Common;

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

