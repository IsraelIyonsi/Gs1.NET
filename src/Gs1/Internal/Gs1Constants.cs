namespace Gs1;

/// <summary>
/// Named constants shared across the GS1 element-string parser, builder, check-digit
/// and date logic. Collected here so no magic numbers or characters appear in the code.
/// </summary>
internal static class Gs1Constants
{
    /// <summary>
    /// FNC1 function character, ASCII Group Separator (GS), code point 0x1D. In a GS1-128 or
    /// scan data stream it terminates a variable-length Application Identifier value.
    /// </summary>
    internal const char FunctionCode1 = '\u001D';

    /// <summary>Opening bracket that precedes an Application Identifier in the human-readable form.</summary>
    internal const char BracketOpen = '(';

    /// <summary>Closing bracket that follows an Application Identifier in the human-readable form.</summary>
    internal const char BracketClose = ')';

    /// <summary>Modulus used by the GS1 mod-10 check-digit algorithm.</summary>
    internal const int CheckDigitModulus = 10;

    /// <summary>Minimum length of a value that can carry a check digit: one data digit plus the check digit.</summary>
    internal const int MinCheckableLength = 2;

    /// <summary>
    /// Weight applied to the rightmost data digit (and every alternate digit moving left) in the
    /// GS1 mod-10 check-digit algorithm.
    /// </summary>
    internal const int CheckDigitWeightOuter = 3;

    /// <summary>Weight applied to the alternating digits between the outer-weighted digits.</summary>
    internal const int CheckDigitWeightInner = 1;

    /// <summary>Total digit length of a GTIN carried by AI 01 or AI 02 (GTIN-14, check digit included).</summary>
    internal const int GtinLength = 14;

    /// <summary>Total digit length of an SSCC carried by AI 00 (SSCC-18, check digit included).</summary>
    internal const int SsccLength = 18;

    /// <summary>Number of leading digits of an element string that determine the Application Identifier length.</summary>
    internal const int AiPrefixLength = 2;

    /// <summary>Length in characters of a YYMMDD date value.</summary>
    internal const int DateLength = 6;

    /// <summary>Number of years in a century, used when resolving a two-digit GS1 year.</summary>
    internal const int YearsPerCentury = 100;

    /// <summary>
    /// When the two-digit year minus the current century year is at least this value the date is
    /// interpreted as belonging to the previous century (GS1 General Specifications, century rule).
    /// </summary>
    internal const int PreviousCenturyThreshold = 51;

    /// <summary>
    /// When the two-digit year minus the current century year is at most this value the date is
    /// interpreted as belonging to the next century (GS1 General Specifications, century rule).
    /// </summary>
    internal const int NextCenturyThreshold = -50;

    /// <summary>GS1 day value signalling "the last day of the month" rather than a calendar day.</summary>
    internal const int LastDayOfMonthMarker = 0;
}
