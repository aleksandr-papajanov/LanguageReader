namespace LanguageReader.Client.Features.Common.Components;

/// <summary>
/// Represents a shared select option.
/// </summary>
public sealed record AppSelectOption<TValue>(TValue Value, string Label);