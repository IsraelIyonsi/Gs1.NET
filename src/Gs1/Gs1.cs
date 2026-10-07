namespace Gs1;

/// <summary>
/// Entry point for parsing and building GS1 Application Identifier element strings. Supports the
/// human-readable bracketed form and the GS1-128 / scan form with FNC1 (0x1D) variable-length
/// termination. All operations are deterministic, offline, and culture-independent.
/// </summary>
public static class Gs1
{
    /// <summary>
    /// Parses a GS1 element string in either the bracketed form, for example
    /// <c>(01)09521234543213(17)261231(10)A1B2C3</c>, or the FNC1 scan form.
    /// </summary>
    /// <param name="input">The element string to parse.</param>
    /// <returns>The ordered elements.</returns>
    /// <exception cref="Gs1ParseException">The input is malformed, has an unknown AI, a truncated fixed-length value, or an invalid GTIN or SSCC check digit.</exception>
    public static Gs1ElementString Parse(string input) => ElementStringParser.Parse(input);

    /// <summary>Attempts to parse a GS1 element string without throwing.</summary>
    /// <param name="input">The element string to parse.</param>
    /// <param name="result">The parsed elements when successful; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when parsing succeeded.</returns>
    public static bool TryParse(string input, out Gs1ElementString? result)
    {
        if (input is null)
        {
            result = null;
            return false;
        }

        try
        {
            result = ElementStringParser.Parse(input);
            return true;
        }
        catch (Gs1ParseException)
        {
            result = null;
            return false;
        }
    }

    /// <summary>Renders elements into the human-readable bracketed form.</summary>
    /// <param name="elements">The elements to render.</param>
    /// <returns>A string such as <c>(01)09521234543213(10)A1B2C3</c>.</returns>
    public static string ToBracketed(IEnumerable<Gs1Element> elements) => ElementStringFormatter.ToBracketed(elements);

    /// <summary>
    /// Renders elements into the GS1-128 / scan form, inserting FNC1 (0x1D) after a variable-length
    /// value whenever another element follows it.
    /// </summary>
    /// <param name="elements">The elements to render.</param>
    /// <returns>The concatenated scan string with FNC1 separators where required.</returns>
    public static string ToScanString(IEnumerable<Gs1Element> elements) => ElementStringFormatter.ToScanString(elements);
}
