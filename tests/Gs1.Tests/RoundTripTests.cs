namespace Gs1.Tests;

public sealed class RoundTripTests
{
    private const char Fnc1 = '\u001D';

    // GTIN (fixed), then batch (variable, followed by another element -> needs FNC1), then serial
    // (variable, last -> no trailing FNC1).
    private static Gs1Element[] Sample() =>
        Gs1.Parse("(01)09521234543213(10)A1B2C3(21)XYZ789").ToArray();

    [Fact]
    public void ToScanString_InsertsFnc1AfterNonTerminalVariableValue()
    {
        var scan = Gs1.ToScanString(Sample());

        var expected = $"0109521234543213" + $"10A1B2C3{Fnc1}" + "21XYZ789";
        Assert.Equal(expected, scan);
    }

    [Fact]
    public void ScanString_RoundTrips()
    {
        var elements = Sample();
        var roundTripped = Gs1.Parse(Gs1.ToScanString(elements)).ToArray();
        Assert.Equal(elements, roundTripped);
    }

    [Fact]
    public void Bracketed_RoundTrips()
    {
        var elements = Sample();
        var roundTripped = Gs1.Parse(Gs1.ToBracketed(elements)).ToArray();
        Assert.Equal(elements, roundTripped);
    }
}
