using System.Globalization;

namespace Gs1;

/// <summary>
/// Parses the YYMMDD date values carried by the GS1 date Application Identifiers
/// (11, 12, 13, 15, 16, 17). The two-digit year is resolved against a reference year using the
/// GS1 century rule, and a day component of <c>00</c> is interpreted as the last day of the month.
/// </summary>
public static class Gs1Date
{
    /// <summary>
    /// Parses a YYMMDD value, resolving the century against the current UTC year.
    /// </summary>
    /// <param name="yymmdd">A six-character date value.</param>
    /// <returns>The resolved date.</returns>
    /// <exception cref="Gs1ParseException">The value is not a valid GS1 date.</exception>
    public static DateOnly Parse(string yymmdd) => Parse(yymmdd, DateTime.UtcNow.Year);

    /// <summary>
    /// Parses a YYMMDD value, resolving the two-digit year against an explicit reference year so the
    /// result is deterministic and independent of the system clock.
    /// </summary>
    /// <param name="yymmdd">A six-character date value.</param>
    /// <param name="referenceYear">The four-digit year used as the century pivot.</param>
    /// <returns>The resolved date.</returns>
    /// <exception cref="Gs1ParseException">The value is not a valid GS1 date.</exception>
    public static DateOnly Parse(string yymmdd, int referenceYear)
    {
        ArgumentNullException.ThrowIfNull(yymmdd);
        if (yymmdd.Length != Gs1Constants.DateLength || !int.TryParse(yymmdd, NumberStyles.None, CultureInfo.InvariantCulture, out _))
        {
            throw new Gs1ParseException($"Date value '{yymmdd}' is not a six-digit YYMMDD value.");
        }

        var yy = ToInt(yymmdd, 0, 2);
        var month = ToInt(yymmdd, 2, 2);
        var day = ToInt(yymmdd, 4, 2);

        var year = ResolveYear(yy, referenceYear);

        if (month is < 1 or > 12)
        {
            throw new Gs1ParseException($"Date value '{yymmdd}' has an invalid month.");
        }

        var daysInMonth = DateTime.DaysInMonth(year, month);
        if (day == Gs1Constants.LastDayOfMonthMarker)
        {
            day = daysInMonth;
        }
        else if (day > daysInMonth)
        {
            throw new Gs1ParseException($"Date value '{yymmdd}' has an invalid day.");
        }

        return new DateOnly(year, month, day);
    }

    private static int ResolveYear(int yy, int referenceYear)
    {
        var centuryBase = referenceYear - (referenceYear % Gs1Constants.YearsPerCentury);
        var difference = yy - (referenceYear % Gs1Constants.YearsPerCentury);

        if (difference >= Gs1Constants.PreviousCenturyThreshold)
        {
            return centuryBase - Gs1Constants.YearsPerCentury + yy;
        }

        if (difference <= Gs1Constants.NextCenturyThreshold)
        {
            return centuryBase + Gs1Constants.YearsPerCentury + yy;
        }

        return centuryBase + yy;
    }

    private static int ToInt(string value, int start, int length)
        => int.Parse(value.AsSpan(start, length), NumberStyles.None, CultureInfo.InvariantCulture);
}
