namespace Gs1;

/// <summary>
/// GS1 standard mod-10 check-digit calculation and validation. The rightmost data digit is
/// weighted by three, the next by one, alternating towards the left; the check digit is the value
/// that makes the weighted sum a multiple of ten. The same algorithm validates GTIN and SSCC.
/// </summary>
public static class Gs1Check
{
    /// <summary>
    /// Computes the GS1 mod-10 check digit for a run of numeric data that does not yet include a
    /// check digit.
    /// </summary>
    /// <param name="digitsWithoutCheckDigit">The data digits, excluding the check digit.</param>
    /// <returns>The check digit in the range 0 to 9.</returns>
    /// <exception cref="ArgumentException">The input is empty or contains a non-digit character.</exception>
    public static int ComputeCheckDigit(ReadOnlySpan<char> digitsWithoutCheckDigit)
    {
        if (digitsWithoutCheckDigit.IsEmpty)
        {
            throw new ArgumentException("At least one digit is required.", nameof(digitsWithoutCheckDigit));
        }

        var sum = 0;
        for (var i = 0; i < digitsWithoutCheckDigit.Length; i++)
        {
            var c = digitsWithoutCheckDigit[digitsWithoutCheckDigit.Length - 1 - i];
            if (!IsDigit(c))
            {
                throw new ArgumentException("Input must contain digits only.", nameof(digitsWithoutCheckDigit));
            }

            var weight = (i % 2 == 0) ? Gs1Constants.CheckDigitWeightOuter : Gs1Constants.CheckDigitWeightInner;
            sum += (c - '0') * weight;
        }

        return (Gs1Constants.CheckDigitModulus - (sum % Gs1Constants.CheckDigitModulus)) % Gs1Constants.CheckDigitModulus;
    }

    /// <summary>
    /// Validates the trailing GS1 mod-10 check digit of a numeric string that already includes it.
    /// </summary>
    /// <param name="valueWithCheckDigit">The full numeric value, the last digit being the check digit.</param>
    /// <returns><see langword="true"/> when the value is numeric and its check digit is correct.</returns>
    public static bool IsValid(ReadOnlySpan<char> valueWithCheckDigit)
    {
        if (valueWithCheckDigit.Length < Gs1Constants.MinCheckableLength || !IsAllDigits(valueWithCheckDigit))
        {
            return false;
        }

        var data = valueWithCheckDigit[..^1];
        var expected = ComputeCheckDigit(data);
        var actual = valueWithCheckDigit[^1] - '0';
        return expected == actual;
    }

    /// <summary>
    /// Validates the mod-10 check digit of a GTIN. Accepts GTIN-8, GTIN-12, GTIN-13 and GTIN-14
    /// because the algorithm is identical across lengths.
    /// </summary>
    /// <param name="gtin">The GTIN digits including the trailing check digit.</param>
    /// <returns><see langword="true"/> when the GTIN check digit is correct.</returns>
    public static bool Gtin(string gtin)
    {
        ArgumentNullException.ThrowIfNull(gtin);
        return IsValid(gtin.AsSpan());
    }

    /// <summary>Validates the mod-10 check digit of an 18-digit SSCC.</summary>
    /// <param name="sscc">The SSCC digits including the trailing check digit.</param>
    /// <returns><see langword="true"/> when the SSCC is 18 digits and its check digit is correct.</returns>
    public static bool Sscc(string sscc)
    {
        ArgumentNullException.ThrowIfNull(sscc);
        return sscc.Length == Gs1Constants.SsccLength && IsValid(sscc.AsSpan());
    }

    private static bool IsAllDigits(ReadOnlySpan<char> value)
    {
        foreach (var c in value)
        {
            if (!IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';
}
