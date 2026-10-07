using System.Text;

namespace Gs1;

/// <summary>
/// Reads and builds GS1 Digital Link URIs for the common primary-key / key-qualifier pattern, for
/// example <c>https://example.com/01/09521234543213/10/A1B2C3/21/XYZ789</c>. The GTIN (AI 01) is the
/// primary key; the consumer product variant (22), batch or lot (10) and serial number (21) are
/// carried as path key-qualifiers in GS1 canonical order; any other AIs are carried in the query string.
/// </summary>
public static class Gs1DigitalLink
{
    private const string Scheme = "https://";
    private const char PathSeparator = '/';
    private const char QueryStart = '?';
    private const char QuerySeparator = '&';
    private const char QueryAssign = '=';

    // Key-qualifiers that belong on the path, in the GS1 canonical order after the GTIN primary key.
    private static readonly string[] PathQualifierOrder =
    {
        ApplicationIdentifiers.ConsumerProductVariantAi,
        ApplicationIdentifiers.BatchLotAi,
        ApplicationIdentifiers.SerialNumberAi,
    };

    /// <summary>
    /// Builds a GS1 Digital Link URI from the given elements. The elements must include a GTIN (AI 01).
    /// </summary>
    /// <param name="elements">The elements to encode.</param>
    /// <param name="domain">The host, optionally with a path prefix, for example <c>id.gs1.org</c>.</param>
    /// <returns>The Digital Link URI.</returns>
    /// <exception cref="ArgumentException">No GTIN (AI 01) is present, or the GTIN check digit is invalid.</exception>
    public static string Build(IEnumerable<Gs1Element> elements, string domain)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentException.ThrowIfNullOrEmpty(domain);

        var list = elements as IReadOnlyList<Gs1Element> ?? new List<Gs1Element>(elements);
        var gtin = Find(list, ApplicationIdentifiers.GtinAi)
            ?? throw new ArgumentException("A GTIN (AI 01) is required to build a GS1 Digital Link.", nameof(elements));

        if (!Gs1Check.Gtin(gtin))
        {
            throw new ArgumentException($"GTIN '{gtin}' has an invalid check digit.", nameof(elements));
        }

        var builder = new StringBuilder();
        builder.Append(Scheme).Append(domain.TrimEnd(PathSeparator));
        builder.Append(PathSeparator).Append(ApplicationIdentifiers.GtinAi).Append(PathSeparator).Append(Uri.EscapeDataString(gtin));

        foreach (var qualifier in PathQualifierOrder)
        {
            var value = Find(list, qualifier);
            if (value is not null)
            {
                builder.Append(PathSeparator).Append(qualifier).Append(PathSeparator).Append(Uri.EscapeDataString(value));
            }
        }

        var query = new StringBuilder();
        foreach (var element in list)
        {
            if (string.Equals(element.Ai, ApplicationIdentifiers.GtinAi, StringComparison.Ordinal) || Array.IndexOf(PathQualifierOrder, element.Ai) >= 0)
            {
                continue;
            }

            query.Append(query.Length == 0 ? QueryStart : QuerySeparator)
                .Append(Uri.EscapeDataString(element.Ai))
                .Append(QueryAssign)
                .Append(Uri.EscapeDataString(element.Value));
        }

        builder.Append(query);
        return builder.ToString();
    }

    /// <summary>Parses a GS1 Digital Link URI into its elements.</summary>
    /// <param name="uri">The Digital Link URI.</param>
    /// <returns>The ordered elements, path key-qualifiers first, then query AIs.</returns>
    /// <exception cref="Gs1ParseException">The URI is not a valid Digital Link, or a GTIN or SSCC check digit is invalid.</exception>
    public static Gs1ElementString Parse(string uri)
    {
        ArgumentException.ThrowIfNullOrEmpty(uri);
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
        {
            throw new Gs1ParseException($"'{uri}' is not an absolute URI.");
        }

        var elements = new List<Gs1Element>();
        var segments = SplitPath(parsed.AbsolutePath);
        var primaryIndex = segments.IndexOf(ApplicationIdentifiers.GtinAi);
        if (primaryIndex < 0)
        {
            throw new Gs1ParseException("No GTIN (AI 01) key found in the Digital Link path.");
        }

        for (var i = primaryIndex; i + 1 < segments.Count; i += 2)
        {
            var ai = segments[i];
            var value = Uri.UnescapeDataString(segments[i + 1]);
            elements.Add(BuildElement(ai, value));
        }

        foreach (var (ai, value) in SplitQuery(parsed.Query))
        {
            elements.Add(BuildElement(ai, value));
        }

        if (elements.Count == 0)
        {
            throw new Gs1ParseException("The Digital Link contained no elements.");
        }

        return new Gs1ElementString(elements);
    }

    // Route through the shared element-string validator so a Digital Link is held to the same rules as a
    // scan or bracketed element string: known AI, length, numeric data type, and GTIN/SSCC check digit.
    private static Gs1Element BuildElement(string ai, string value) => ElementStringParser.BuildElement(ai, value);

    private static string? Find(IReadOnlyList<Gs1Element> elements, string ai)
    {
        foreach (var element in elements)
        {
            if (string.Equals(element.Ai, ai, StringComparison.Ordinal))
            {
                return element.Value;
            }
        }

        return null;
    }

    private static List<string> SplitPath(string path)
    {
        var segments = new List<string>();
        foreach (var segment in path.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            segments.Add(segment);
        }

        return segments;
    }

    private static IEnumerable<(string Ai, string Value)> SplitQuery(string query)
    {
        var trimmed = query.TrimStart(QueryStart);
        if (trimmed.Length == 0)
        {
            yield break;
        }

        foreach (var pair in trimmed.Split(QuerySeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var assign = pair.IndexOf(QueryAssign);
            if (assign <= 0)
            {
                continue;
            }

            var ai = Uri.UnescapeDataString(pair[..assign]);
            var value = Uri.UnescapeDataString(pair[(assign + 1)..]);
            yield return (ai, value);
        }
    }
}
