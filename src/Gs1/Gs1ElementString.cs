using System.Collections;

namespace Gs1;

/// <summary>
/// An ordered, immutable collection of parsed <see cref="Gs1Element"/> values, with lookups by
/// Application Identifier and typed conveniences for the most common keys.
/// </summary>
public sealed class Gs1ElementString : IReadOnlyList<Gs1Element>
{
    private readonly IReadOnlyList<Gs1Element> _elements;

    /// <summary>Initializes a new instance of the <see cref="Gs1ElementString"/> class.</summary>
    /// <param name="elements">The parsed elements, in the order they appeared.</param>
    public Gs1ElementString(IReadOnlyList<Gs1Element> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        _elements = elements;
    }

    /// <summary>Gets the number of elements.</summary>
    public int Count => _elements.Count;

    /// <summary>Gets the element at the given index.</summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The element at <paramref name="index"/>.</returns>
    public Gs1Element this[int index] => _elements[index];

    /// <summary>Gets the GTIN carried by AI 01, or <see langword="null"/> when absent.</summary>
    public string? Gtin => Get(ApplicationIdentifiers.GtinAi)?.Value;

    /// <summary>Gets the SSCC carried by AI 00, or <see langword="null"/> when absent.</summary>
    public string? Sscc => Get(ApplicationIdentifiers.SsccAi)?.Value;

    /// <summary>Gets the batch or lot number carried by AI 10, or <see langword="null"/> when absent.</summary>
    public string? BatchLot => Get(ApplicationIdentifiers.BatchLotAi)?.Value;

    /// <summary>Gets the serial number carried by AI 21, or <see langword="null"/> when absent.</summary>
    public string? SerialNumber => Get(ApplicationIdentifiers.SerialNumberAi)?.Value;

    /// <summary>
    /// Gets the production date carried by AI 11, or <see langword="null"/> when absent. The century is
    /// resolved against the current UTC year; use <see cref="GetDate(string, int)"/> for a deterministic year.
    /// </summary>
    public DateOnly? ProductionDate => GetDate(ApplicationIdentifiers.ProductionDateAi);

    /// <summary>
    /// Gets the due date carried by AI 12, or <see langword="null"/> when absent. The century is resolved
    /// against the current UTC year; use <see cref="GetDate(string, int)"/> for a deterministic year.
    /// </summary>
    public DateOnly? DueDate => GetDate(ApplicationIdentifiers.DueDateAi);

    /// <summary>
    /// Gets the packaging date carried by AI 13, or <see langword="null"/> when absent. The century is
    /// resolved against the current UTC year; use <see cref="GetDate(string, int)"/> for a deterministic year.
    /// </summary>
    public DateOnly? PackagingDate => GetDate(ApplicationIdentifiers.PackagingDateAi);

    /// <summary>
    /// Gets the sell-by date carried by AI 15, or <see langword="null"/> when absent. The century is
    /// resolved against the current UTC year; use <see cref="GetDate(string, int)"/> for a deterministic year.
    /// </summary>
    public DateOnly? SellByDate => GetDate(ApplicationIdentifiers.SellByDateAi);

    /// <summary>
    /// Gets the best-before date carried by AI 16, or <see langword="null"/> when absent. The century is
    /// resolved against the current UTC year; use <see cref="GetDate(string, int)"/> for a deterministic year.
    /// </summary>
    public DateOnly? BestBeforeDate => GetDate(ApplicationIdentifiers.BestBeforeDateAi);

    /// <summary>
    /// Gets the expiration date carried by AI 17, or <see langword="null"/> when absent. The century is
    /// resolved against the current UTC year; use <see cref="GetDate(string, int)"/> for a deterministic year.
    /// </summary>
    public DateOnly? ExpirationDate => GetDate(ApplicationIdentifiers.ExpirationDateAi);

    /// <summary>Finds the first element with the given Application Identifier.</summary>
    /// <param name="ai">The AI number to look up.</param>
    /// <returns>The matching element, or <see langword="null"/> when absent.</returns>
    public Gs1Element? Get(string ai)
    {
        ArgumentNullException.ThrowIfNull(ai);
        foreach (var element in _elements)
        {
            if (string.Equals(element.Ai, ai, StringComparison.Ordinal))
            {
                return element;
            }
        }

        return null;
    }

    /// <summary>
    /// Parses the value of a date Application Identifier (11, 12, 13, 15, 16, 17) into a
    /// <see cref="DateOnly"/>, resolving the century against the current UTC year. Because the two-digit
    /// year is resolved against the system clock, the result can shift at a century boundary; use the
    /// <see cref="GetDate(string, int)"/> overload for a deterministic reference year.
    /// </summary>
    /// <param name="ai">The date AI number.</param>
    /// <returns>The parsed date, or <see langword="null"/> when the AI is absent.</returns>
    public DateOnly? GetDate(string ai)
    {
        var element = Get(ai);
        return element is null ? null : Gs1Date.Parse(element.Value.Value);
    }

    /// <summary>
    /// Parses the value of a date Application Identifier (11, 12, 13, 15, 16, 17) into a
    /// <see cref="DateOnly"/>, resolving the two-digit year against an explicit reference year so the
    /// result is independent of the system clock.
    /// </summary>
    /// <param name="ai">The date AI number.</param>
    /// <param name="referenceYear">The four-digit year used as the century pivot.</param>
    /// <returns>The parsed date, or <see langword="null"/> when the AI is absent.</returns>
    public DateOnly? GetDate(string ai, int referenceYear)
    {
        var element = Get(ai);
        return element is null ? null : Gs1Date.Parse(element.Value.Value, referenceYear);
    }

    /// <summary>Returns an enumerator over the elements in order.</summary>
    /// <returns>An enumerator.</returns>
    public IEnumerator<Gs1Element> GetEnumerator() => _elements.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
