namespace Gs1.Tests;

public sealed class CheckDigitTests
{
    [Fact]
    public void ComputeCheckDigit_ForKnownGtin14_Returns3()
    {
        // GTIN-14 data 0952123454321 has a known GS1 mod-10 check digit of 3.
        var check = Gs1Check.ComputeCheckDigit("0952123454321");
        Assert.Equal(3, check);
    }

    [Theory]
    [InlineData("09521234543213", true)]   // valid GTIN-14, check digit 3
    [InlineData("09521234543214", false)]  // last digit flipped -> invalid
    [InlineData("09521234543210", false)]
    [InlineData("4006381333931", true)]    // classic valid EAN-13, check digit 1
    [InlineData("4006381333930", false)]
    public void Gtin_ValidatesCheckDigit(string gtin, bool expected)
    {
        Assert.Equal(expected, Gs1Check.Gtin(gtin));
    }

    [Theory]
    [InlineData("006141411234567890", true)]   // valid SSCC-18, check digit 0
    [InlineData("006141411234567891", false)]  // last digit flipped -> invalid
    [InlineData("00614141123456789", false)]   // 17 digits -> wrong length
    public void Sscc_ValidatesCheckDigit(string sscc, bool expected)
    {
        Assert.Equal(expected, Gs1Check.Sscc(sscc));
    }

    [Fact]
    public void ComputeCheckDigit_RoundTrips_ForComputedSscc()
    {
        // Appending the computed check digit to the data must produce a value that validates.
        const string data = "00614141123456789";
        var check = Gs1Check.ComputeCheckDigit(data);
        Assert.Equal(0, check);
        Assert.True(Gs1Check.Sscc(data + check));
    }
}
