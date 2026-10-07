using System.Collections.Frozen;

namespace Gs1;

/// <summary>
/// The Application Identifier dictionary. It holds the common GS1 AIs with their titles and value
/// formats, and the prefix table that determines how many digits an AI occupies when reading a raw
/// element string. The set is deliberately a well-covered common subset, not the full GS1 register,
/// and is straightforward to extend by adding entries to <see cref="BuildDefinitions"/>.
/// </summary>
public static class ApplicationIdentifiers
{
    private const bool Fixed = true;
    private const bool Variable = false;

    // Number of characters a measure AI occupies (for example 3103), and the length of its base code
    // (for example 310) once the trailing implied-decimal digit is removed.
    private const int MeasureAiLength = 4;
    private const int MeasureBaseCodeLength = 3;

    // Value lengths for the common AIs, named to avoid unexplained numbers in the table below.
    private const int SsccValueLength = 18;
    private const int GtinValueLength = 14;
    private const int DateValueLength = 6;
    private const int VariantValueLength = 2;
    private const int MeasureValueLength = 6;
    private const int GlnValueLength = 13;
    private const int GsinValueLength = 17;
    private const int GsrnValueLength = 18;
    private const int PricePerUnitValueLength = 6;
    private const int ExpiryDateTimeValueLength = 10;

    private const int ShortTextMax = 20;
    private const int MediumTextMax = 30;
    private const int CountMax = 8;
    private const int PostalCodeMax = 20;
    private const int PostalCodeWithCountryMax = 12;
    private const int ProductionDateTimeMax = 12;
    private const int PaymentSlipMax = 25;

    // AI 90 carries information mutually agreed between trading partners (max 30); AIs 91 to 99 carry
    // company internal information (max 90).
    private const int MutuallyAgreedMax = 30;
    private const int CompanyInternalMax = 90;

    // AI numbers referenced elsewhere in the library, named to avoid magic strings.
    internal const string SsccAi = "00";
    internal const string GtinAi = "01";
    internal const string ContainedGtinAi = "02";
    internal const string BatchLotAi = "10";
    internal const string ProductionDateAi = "11";
    internal const string DueDateAi = "12";
    internal const string PackagingDateAi = "13";
    internal const string SellByDateAi = "15";
    internal const string BestBeforeDateAi = "16";
    internal const string ExpirationDateAi = "17";
    internal const string VariantAi = "20";
    internal const string SerialNumberAi = "21";
    internal const string ConsumerProductVariantAi = "22";

    private static readonly FrozenDictionary<string, ApplicationIdentifier> Definitions = BuildDefinitions();
    private static readonly FrozenDictionary<string, int> AiLengthByPrefix = BuildPrefixLengths();

    // First two digits of a measure AI (6-digit value, 4-digit AI, implied decimal from the 4th digit).
    private static readonly FrozenSet<string> MeasurePrefixes =
        new[] { "31", "32", "33", "34", "35", "36" }.ToFrozenSet(StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, string> MeasureTitles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["310"] = "Net weight (kg)",
        ["311"] = "Length, first dimension (m)",
        ["312"] = "Width, diameter, second dimension (m)",
        ["313"] = "Depth, thickness, height, third dimension (m)",
        ["314"] = "Area (m2)",
        ["315"] = "Net volume (l)",
        ["316"] = "Net volume (m3)",
        ["320"] = "Net weight (lb)",
        ["330"] = "Logistic weight (kg)",
        ["340"] = "Logistic weight (lb)",
        ["350"] = "Area (in2)",
        ["360"] = "Net volume (qt)",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to find the Application Identifier that begins at <paramref name="position"/> in a raw
    /// element string and returns how many characters the AI occupies.
    /// </summary>
    /// <param name="input">The raw element string.</param>
    /// <param name="position">The index at which the AI starts.</param>
    /// <param name="aiLength">The number of characters the AI occupies when found.</param>
    /// <returns><see langword="true"/> when the AI prefix is recognised.</returns>
    public static bool TryGetAiLength(string input, int position, out int aiLength)
    {
        ArgumentNullException.ThrowIfNull(input);
        aiLength = 0;
        if (position + Gs1Constants.AiPrefixLength > input.Length)
        {
            return false;
        }

        var prefix = input.Substring(position, Gs1Constants.AiPrefixLength);
        return AiLengthByPrefix.TryGetValue(prefix, out aiLength);
    }

    /// <summary>Attempts to resolve the definition for an Application Identifier number.</summary>
    /// <param name="ai">The AI number.</param>
    /// <param name="definition">The resolved definition when found.</param>
    /// <returns><see langword="true"/> when the AI is known.</returns>
    public static bool TryGet(string ai, out ApplicationIdentifier definition)
    {
        ArgumentNullException.ThrowIfNull(ai);
        if (Definitions.TryGetValue(ai, out definition))
        {
            return true;
        }

        if (TryGetMeasure(ai, out definition))
        {
            return true;
        }

        definition = default;
        return false;
    }

    /// <summary>Returns the human-readable title for an AI, or <see langword="null"/> when unknown.</summary>
    /// <param name="ai">The AI number.</param>
    /// <returns>The title, or <see langword="null"/>.</returns>
    public static string? TitleFor(string ai) => TryGet(ai, out var definition) ? definition.Title : null;

    private static bool TryGetMeasure(string ai, out ApplicationIdentifier definition)
    {
        definition = default;
        if (ai.Length != MeasureAiLength || !MeasurePrefixes.Contains(ai.AsSpan(0, Gs1Constants.AiPrefixLength).ToString()))
        {
            return false;
        }

        foreach (var c in ai)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        var baseCode = ai[..MeasureBaseCodeLength];
        var title = MeasureTitles.TryGetValue(baseCode, out var known) ? known : $"Measurement ({baseCode}n)";
        definition = Num(ai, title, MeasureValueLength, Fixed);
        return true;
    }

    // Factory for a GS1 numeric (format "N") Application Identifier: its value must be digits only.
    private static ApplicationIdentifier Num(string ai, string title, int valueLength, bool isFixed)
        => new(ai, title, valueLength, isFixed, IsNumeric: true);

    // Factory for a GS1 alphanumeric (format "X") Application Identifier.
    private static ApplicationIdentifier Aln(string ai, string title, int valueLength, bool isFixed)
        => new(ai, title, valueLength, isFixed, IsNumeric: false);

    private static FrozenDictionary<string, ApplicationIdentifier> BuildDefinitions()
    {
        var entries = new[]
        {
            Num(SsccAi, "SSCC", SsccValueLength, Fixed),
            Num(GtinAi, "GTIN", GtinValueLength, Fixed),
            Num(ContainedGtinAi, "GTIN of contained trade items", GtinValueLength, Fixed),
            Aln(BatchLotAi, "Batch/Lot", ShortTextMax, Variable),
            Num(ProductionDateAi, "Production date", DateValueLength, Fixed),
            Num(DueDateAi, "Due date", DateValueLength, Fixed),
            Num(PackagingDateAi, "Packaging date", DateValueLength, Fixed),
            Num(SellByDateAi, "Best before / sell by date", DateValueLength, Fixed),
            Num(BestBeforeDateAi, "Best before date", DateValueLength, Fixed),
            Num(ExpirationDateAi, "Expiration date", DateValueLength, Fixed),
            Num(VariantAi, "Internal product variant", VariantValueLength, Fixed),
            Aln(SerialNumberAi, "Serial number", ShortTextMax, Variable),
            Aln(ConsumerProductVariantAi, "Consumer product variant", ShortTextMax, Variable),
            Num("30", "Variable count", CountMax, Variable),
            Num("37", "Count of trade items", CountMax, Variable),
            Aln("240", "Additional product identification", MediumTextMax, Variable),
            Aln("241", "Customer part number", MediumTextMax, Variable),
            Aln("250", "Secondary serial number", MediumTextMax, Variable),
            Aln("251", "Reference to source entity", MediumTextMax, Variable),
            Aln("253", "Global Document Type Identifier (GDTI)", MediumTextMax, Variable),
            Aln("254", "GLN extension component", ShortTextMax, Variable),
            Aln("400", "Customer purchase order number", MediumTextMax, Variable),
            Aln("401", "Global Identification Number for Consignment (GINC)", MediumTextMax, Variable),
            Num("402", "Global Shipment Identification Number (GSIN)", GsinValueLength, Fixed),
            Aln("403", "Routing code", MediumTextMax, Variable),
            Num("410", "Ship to / deliver to GLN", GlnValueLength, Fixed),
            Num("411", "Bill to / invoice to GLN", GlnValueLength, Fixed),
            Num("412", "Purchased from GLN", GlnValueLength, Fixed),
            Num("413", "Ship for / deliver for GLN", GlnValueLength, Fixed),
            Num("414", "Identification of a physical location GLN", GlnValueLength, Fixed),
            Num("415", "GLN of the invoicing party", GlnValueLength, Fixed),
            Num("416", "GLN of the production or service location", GlnValueLength, Fixed),
            Num("417", "Party GLN", GlnValueLength, Fixed),
            Aln("420", "Ship to / deliver to postal code (single postal authority)", PostalCodeMax, Variable),
            Aln("421", "Ship to / deliver to postal code with ISO country code", PostalCodeWithCountryMax, Variable),
            Num("7003", "Expiration date and time", ExpiryDateTimeValueLength, Fixed),
            Num("8005", "Price per unit of measure", PricePerUnitValueLength, Fixed),
            Num("8008", "Date and time of production", ProductionDateTimeMax, Variable),
            Num("8018", "Global Service Relation Number (recipient)", GsrnValueLength, Fixed),
            Aln("8020", "Payment slip reference number", PaymentSlipMax, Variable),
            Aln("90", "Information mutually agreed between trading partners", MutuallyAgreedMax, Variable),
            Aln("91", "Company internal information", CompanyInternalMax, Variable),
            Aln("92", "Company internal information", CompanyInternalMax, Variable),
            Aln("93", "Company internal information", CompanyInternalMax, Variable),
            Aln("94", "Company internal information", CompanyInternalMax, Variable),
            Aln("95", "Company internal information", CompanyInternalMax, Variable),
            Aln("96", "Company internal information", CompanyInternalMax, Variable),
            Aln("97", "Company internal information", CompanyInternalMax, Variable),
            Aln("98", "Company internal information", CompanyInternalMax, Variable),
            Aln("99", "Company internal information", CompanyInternalMax, Variable),
        };

        return entries.ToFrozenDictionary(e => e.Ai, StringComparer.Ordinal);
    }

    private static FrozenDictionary<string, int> BuildPrefixLengths()
    {
        const int twoDigitAi = 2;
        const int threeDigitAi = 3;
        const int fourDigitAi = 4;

        var map = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["00"] = twoDigitAi,
            ["01"] = twoDigitAi,
            ["02"] = twoDigitAi,
            ["10"] = twoDigitAi,
            ["11"] = twoDigitAi,
            ["12"] = twoDigitAi,
            ["13"] = twoDigitAi,
            ["15"] = twoDigitAi,
            ["16"] = twoDigitAi,
            ["17"] = twoDigitAi,
            ["20"] = twoDigitAi,
            ["21"] = twoDigitAi,
            ["22"] = twoDigitAi,
            ["30"] = twoDigitAi,
            ["37"] = twoDigitAi,
            ["24"] = threeDigitAi,
            ["25"] = threeDigitAi,
            ["40"] = threeDigitAi,
            ["41"] = threeDigitAi,
            ["42"] = threeDigitAi,
            ["31"] = fourDigitAi,
            ["32"] = fourDigitAi,
            ["33"] = fourDigitAi,
            ["34"] = fourDigitAi,
            ["35"] = fourDigitAi,
            ["36"] = fourDigitAi,
            ["70"] = fourDigitAi,
            ["80"] = fourDigitAi,
            ["90"] = twoDigitAi,
            ["91"] = twoDigitAi,
            ["92"] = twoDigitAi,
            ["93"] = twoDigitAi,
            ["94"] = twoDigitAi,
            ["95"] = twoDigitAi,
            ["96"] = twoDigitAi,
            ["97"] = twoDigitAi,
            ["98"] = twoDigitAi,
            ["99"] = twoDigitAi,
        };

        return map.ToFrozenDictionary(StringComparer.Ordinal);
    }
}
