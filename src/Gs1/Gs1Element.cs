namespace Gs1;

/// <summary>
/// A single parsed GS1 element: an Application Identifier and its value, with the AI's title when known.
/// </summary>
/// <param name="Ai">The Application Identifier number, for example <c>01</c>.</param>
/// <param name="Value">The value that follows the AI, with any FNC1 terminator removed.</param>
/// <param name="Title">The human-readable title of the AI, or <see langword="null"/> when unknown.</param>
public readonly record struct Gs1Element(string Ai, string Value, string? Title);
