namespace Gs1.Tests;

public sealed class DigitalLinkTests
{
    private const string Domain = "example.com";

    private static Gs1Element[] Sample() =>
        Gs1.Parse("(01)09521234543213(10)A1B2C3(21)XYZ789").ToArray();

    [Fact]
    public void Build_ProducesPrimaryKeyAndQualifierPath()
    {
        var uri = Gs1DigitalLink.Build(Sample(), Domain);
        Assert.Equal("https://example.com/01/09521234543213/10/A1B2C3/21/XYZ789", uri);
    }

    [Fact]
    public void DigitalLink_RoundTrips()
    {
        var elements = Sample();
        var uri = Gs1DigitalLink.Build(elements, Domain);
        var parsed = Gs1DigitalLink.Parse(uri).ToArray();
        Assert.Equal(elements, parsed);
    }

    [Fact]
    public void Build_PlacesNonQualifierAisInQuery()
    {
        // AI 17 (expiration date) is not a path key-qualifier, so it belongs in the query string.
        var elements = Gs1.Parse("(01)09521234543213(17)261231").ToArray();
        var uri = Gs1DigitalLink.Build(elements, Domain);
        Assert.Equal("https://example.com/01/09521234543213?17=261231", uri);
    }

    [Fact]
    public void Parse_InvalidGtinCheckDigit_Throws()
    {
        Assert.Throws<Gs1ParseException>(() => Gs1DigitalLink.Parse("https://example.com/01/09521234543214"));
    }

    [Fact]
    public void Build_WithoutGtin_Throws()
    {
        var elements = Gs1.Parse("(10)A1B2C3").ToArray();
        Assert.Throws<ArgumentException>(() => Gs1DigitalLink.Build(elements, Domain));
    }

    [Fact]
    public void Parse_UnknownAiInPath_Throws()
    {
        // AI 88 is not defined; Digital Link parsing must reject it like element-string parsing does.
        Assert.Throws<Gs1ParseException>(
            () => Gs1DigitalLink.Parse("https://example.com/01/09521234543213/88/XYZ"));
    }

    [Fact]
    public void Parse_EmptyQueryValue_Throws()
    {
        // An empty variable-length value (?10=) must be rejected, not accepted as an empty batch.
        Assert.Throws<Gs1ParseException>(
            () => Gs1DigitalLink.Parse("https://example.com/01/09521234543213?10="));
    }

    [Fact]
    public void Parse_NumericQualifierWithLetters_Throws()
    {
        // GLN (AI 414, numeric) carried as a query AI with letters must be rejected.
        Assert.Throws<Gs1ParseException>(
            () => Gs1DigitalLink.Parse("https://example.com/01/09521234543213?414=ABCDEFGHIJKLM"));
    }
}
