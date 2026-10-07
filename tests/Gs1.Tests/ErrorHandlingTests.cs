namespace Gs1.Tests;

public sealed class ErrorHandlingTests
{
    [Fact]
    public void Parse_BadGtinCheckDigit_ThrowsGs1ParseException()
    {
        Assert.Throws<Gs1ParseException>(() => Gs1.Parse("(01)09521234543214"));
    }

    [Fact]
    public void TryParse_BadGtinCheckDigit_ReturnsFalse()
    {
        var ok = Gs1.TryParse("(01)09521234543214", out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("(85)ABCDEF")]        // unknown AI (prefix 85 is not defined)
    [InlineData("8509521234543213")]  // unknown AI in scan form
    [InlineData("0109521234543")]     // fixed-length GTIN value truncated
    [InlineData("")]                  // empty input
    [InlineData("(01)")]              // fixed-length value missing
    [InlineData("(01")]              // malformed bracketed input, no ')'
    public void TryParse_Malformed_ReturnsFalseWithoutUnwrappedException(string input)
    {
        var ok = Gs1.TryParse(input, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_Null_ReturnsFalse()
    {
        var ok = Gs1.TryParse(null!, out var result);
        Assert.False(ok);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_TruncatedFixedLength_ThrowsGs1ParseException()
    {
        Assert.Throws<Gs1ParseException>(() => Gs1.Parse("0109521234543"));
    }

    [Fact]
    public void Parse_UnknownAi_ThrowsGs1ParseException()
    {
        Assert.Throws<Gs1ParseException>(() => Gs1.Parse("(85)ABCDEF"));
    }
}
