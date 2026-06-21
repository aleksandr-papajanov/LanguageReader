namespace LanguageReader.Client.Features.Common.Components;

/// <summary>
/// Represents a single tab item in the shared tab control.
/// </summary>
public sealed record AppTabItem(
    string Value,
    string Label,
    string? Icon = null,
    string? ImageSrc = null);

