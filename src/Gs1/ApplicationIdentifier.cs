namespace Gs1;

/// <summary>
/// Describes a single GS1 Application Identifier (AI): its number, human-readable title, and the
/// format of the value that follows it.
/// </summary>
/// <param name="Ai">The AI number, for example <c>01</c> or <c>3103</c>.</param>
/// <param name="Title">A short human-readable title, for example <c>GTIN</c> or <c>Batch/Lot</c>.</param>
/// <param name="ValueLength">
/// For a fixed-length AI, the exact number of value characters; for a variable-length AI, the
/// maximum number of value characters permitted.
/// </param>
/// <param name="IsFixedLength">
/// <see langword="true"/> when the value is always <paramref name="ValueLength"/> characters;
/// <see langword="false"/> when the value is variable-length and terminated by FNC1 or end of data.
/// </param>
/// <param name="IsNumeric">
/// <see langword="true"/> for the GS1 numeric data type (format "N"), whose value must contain digits
/// only; <see langword="false"/> for the GS1 alphanumeric data type (format "X").
/// </param>
public readonly record struct ApplicationIdentifier(string Ai, string Title, int ValueLength, bool IsFixedLength, bool IsNumeric);
