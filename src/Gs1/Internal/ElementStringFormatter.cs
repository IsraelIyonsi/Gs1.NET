using System.Text;

namespace Gs1;

/// <summary>
/// Renders elements back into the two supported textual forms. The bracketed form writes each AI in
/// parentheses; the scan form concatenates AI and value and inserts FNC1 (0x1D) after a
/// variable-length value whenever another element follows it.
/// </summary>
internal static class ElementStringFormatter
{
    internal static string ToBracketed(IEnumerable<Gs1Element> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        var builder = new StringBuilder();
        foreach (var element in elements)
        {
            builder.Append(Gs1Constants.BracketOpen)
                .Append(element.Ai)
                .Append(Gs1Constants.BracketClose)
                .Append(element.Value);
        }

        return builder.ToString();
    }

    internal static string ToScanString(IEnumerable<Gs1Element> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        var list = elements as IReadOnlyList<Gs1Element> ?? new List<Gs1Element>(elements);
        var builder = new StringBuilder();

        for (var i = 0; i < list.Count; i++)
        {
            var element = list[i];
            builder.Append(element.Ai).Append(element.Value);

            var isLast = i == list.Count - 1;
            if (!isLast && IsVariableLength(element.Ai))
            {
                builder.Append(Gs1Constants.FunctionCode1);
            }
        }

        return builder.ToString();
    }

    private static bool IsVariableLength(string ai)
    {
        // Unknown AIs are treated as variable-length so a separator is emitted and the value boundary
        // is preserved on a subsequent parse.
        return !ApplicationIdentifiers.TryGet(ai, out var definition) || !definition.IsFixedLength;
    }
}
