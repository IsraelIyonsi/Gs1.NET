namespace Gs1.Tests;

public sealed class ParseTests
{
    private const char Fnc1 = '\u001D';
    private const string BracketedSample = "(01)09521234543213(17)261231(10)A1B2C3";

    // The same three elements as BracketedSample in GS1-128 scan form: fixed 01 (14) and fixed 17 (6)
    // are read by length, and the trailing variable 10 is read to the end of the string.
    private const string ScanSample = "010952123454321317" + "261231" + "10A1B2C3";

    [Fact]
    public void Parse_Bracketed_RecoversThreeElements()
    {
        var result = Gs1.Parse(BracketedSample);

        Assert.Equal(3, result.Count);
        Assert.Equal("09521234543213", result.Gtin);
        Assert.Equal("A1B2C3", result.BatchLot);
        Assert.Equal("261231", result.Get("17")!.Value.Value);
        Assert.Equal("GTIN", result[0].Title);
    }

    [Fact]
    public void Parse_Bracketed_ParsesExpirationDate()
    {
        var result = Gs1.Parse(BracketedSample);

        // Resolve the century against a fixed reference year so the assertion never becomes a time bomb.
        var expiry = Gs1Date.Parse(result.Get("17")!.Value.Value, referenceYear: 2026);
        Assert.Equal(new DateOnly(2026, 12, 31), expiry);
    }

    [Fact]
    public void Parse_ScanForm_MatchesBracketedForm()
    {
        var fromBracketed = Gs1.Parse(BracketedSample).ToArray();
        var fromScan = Gs1.Parse(ScanSample).ToArray();

        Assert.Equal(fromBracketed, fromScan);
    }

    [Fact]
    public void Parse_ScanForm_StopsVariableValueAtFnc1_NotSwallowingNextAi()
    {
        // AI 21 (variable serial) then FNC1 then AI 10 (variable batch) then FNC1 then fixed AI 01.
        // This is the critical correctness point: the serial value must stop at FNC1.
        var scan = $"21SER123{Fnc1}10LOT456{Fnc1}0109521234543213";

        var result = Gs1.Parse(scan);

        Assert.Equal(3, result.Count);
        Assert.Equal("SER123", result.SerialNumber);   // must NOT be "SER123\u001D10LOT456..."
        Assert.Equal("LOT456", result.BatchLot);
        Assert.Equal("09521234543213", result.Gtin);
    }

    [Fact]
    public void Parse_ScanForm_AllowsLeadingFnc1()
    {
        var result = Gs1.Parse($"{Fnc1}0109521234543213{Fnc1}10A1B2C3");

        Assert.Equal("09521234543213", result.Gtin);
        Assert.Equal("A1B2C3", result.BatchLot);
    }

    [Fact]
    public void Parse_Measure_ReadsFixedSixDigitValueAndTitle()
    {
        var result = Gs1.Parse("(3103)000123");

        Assert.Equal("000123", result[0].Value);
        Assert.Equal("Net weight (kg)", result[0].Title);
    }

    [Fact]
    public void GetDate_Day00_ResolvesToLastDayOfMonth()
    {
        // AI 17 value 260200 => February 2026, day 00 => last day of month (28, 2026 is not a leap year).
        Assert.Equal(new DateOnly(2026, 2, 28), Gs1Date.Parse("260200", referenceYear: 2026));
    }
}
