namespace Gs1.Tests;

public sealed class DataTypeValidationTests
{
    private const char Fnc1 = '\u001D';

    [Theory]
    [InlineData("(17)ABCDEF")]             // date AI (numeric) with letters
    [InlineData("(410)ABCDEFGHIJKLM")]     // GLN AI (numeric, 13 chars) with letters
    [InlineData("(30)ABC")]                // variable count (numeric) with letters
    [InlineData("(3103)00ABCD")]           // measure AI (numeric) with letters
    public void Parse_NumericAiWithNonDigitValue_ThrowsGs1ParseException(string input)
    {
        Assert.Throws<Gs1ParseException>(() => Gs1.Parse(input));
    }

    [Fact]
    public void Parse_NumericAiRejectsGarbageAtParseTime_NotFromAGetter()
    {
        // The failure must surface from Parse, not be deferred to a property getter.
        Assert.False(Gs1.TryParse("(17)ABCDEF", out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("(30)123", "30", "123")]                          // valid numeric count
    [InlineData("(410)1234567890123", "410", "1234567890123")]    // valid numeric GLN (13 digits)
    [InlineData("(17)261231", "17", "261231")]                    // valid numeric date
    public void Parse_NumericAiWithDigits_StillParses(string input, string ai, string expected)
    {
        var result = Gs1.Parse(input);
        Assert.Equal(expected, result.Get(ai)!.Value.Value);
    }

    [Fact]
    public void Parse_AlphanumericAi_StillAcceptsLetters()
    {
        var result = Gs1.Parse("(10)A1B2C3(21)XYZ789");
        Assert.Equal("A1B2C3", result.BatchLot);
        Assert.Equal("XYZ789", result.SerialNumber);
    }

    [Fact]
    public void Parse_FixedLengthValueWithPrematureFnc1_Throws()
    {
        // AI 17 declares a 6-char value, but an FNC1 appears after 5 chars. The parser must throw rather
        // than slice "26123\u001D" and realign the next AI across the separator.
        var scan = $"1726123{Fnc1}10ABC";
        Assert.Throws<Gs1ParseException>(() => Gs1.Parse(scan));
    }
}
