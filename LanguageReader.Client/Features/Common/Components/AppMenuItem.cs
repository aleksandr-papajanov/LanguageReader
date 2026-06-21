namespace LanguageReader.Client.Features.Common.Components;

/// <summary>
/// Represents one option in a shared flyout menu.
/// </summary>
public sealed record AppMenuItem(
    string Key,
    string Label,
    string? Icon = null,
    bool Disabled = false,
    string Variant = "ghost");

