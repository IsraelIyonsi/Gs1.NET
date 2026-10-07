namespace Gs1;

/// <summary>
/// Thrown when a GS1 element string or GS1 Digital Link URI cannot be parsed, for example when an
/// Application Identifier is unknown, a fixed-length value is truncated, or a GTIN or SSCC carries
/// an invalid mod-10 check digit.
/// </summary>
public sealed class Gs1ParseException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="Gs1ParseException"/> class.</summary>
    /// <param name="message">A description of why parsing failed.</param>
    public Gs1ParseException(string message) : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Gs1ParseException"/> class.</summary>
    /// <param name="message">A description of why parsing failed.</param>
    /// <param name="innerException">The underlying exception that caused the failure.</param>
    public Gs1ParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
