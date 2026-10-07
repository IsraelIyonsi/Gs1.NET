# Gs1.Net

Correct, zero-dependency parsing and building of **GS1 Application Identifier (AI) element strings** for .NET.

The NuGet ecosystem is full of barcode *image* generators, but there is no focused, pure library that understands the *data* inside a GS1 barcode: the Application Identifiers, the FNC1 variable-length rule, the GTIN and SSCC check digits, and GS1 Digital Link. `Gs1.Net` fills that gap. It is pure string and arithmetic: deterministic, offline, AOT-compatible, with no runtime dependencies.

```
dotnet add package Gs1.Net
```

## Quick start

```csharp
using Gs1;

// Human-readable bracketed form
var e = Gs1.Parse("(01)09521234543213(17)261231(10)A1B2C3");

string? gtin  = e.Gtin;            // "09521234543213"
string? lot   = e.BatchLot;        // "A1B2C3"
DateOnly? exp = e.ExpirationDate;  // 2026-12-31

// GS1-128 / scan form (FNC1 = ASCII GS, 0x1D) parses to the same elements
var scan = Gs1.Parse("\u001D0109521234543213" + "17261231" + "10A1B2C3");

// Build element strings
string bracketed = Gs1.ToBracketed(e);   // (01)09521234543213(17)261231(10)A1B2C3
string gs128     = Gs1.ToScanString(e);  // FNC1 inserted after variable-length values
```

## The FNC1 variable-length rule

This is the single most important thing a GS1 parser must get right.

Each AI has a defined value format: either a **fixed** total length (for example GTIN is always 14 digits) or **variable** up to a maximum (for example a batch/lot is up to 20 characters).

- A **fixed-length** AI value is read by its defined length.
- A **variable-length** AI value is read until the next **FNC1** separator (`0x1D`, ASCII Group Separator) or the end of the data. In the bracketed form the value runs until the next `(`.

A correct parser never lets a variable-length value swallow the AI that follows it:

```csharp
// AI 21 (variable serial) | FNC1 | AI 10 (variable batch) | FNC1 | AI 01 (fixed GTIN)
var e = Gs1.Parse("21SER123\u001D10LOT456\u001D0109521234543213");

e.SerialNumber; // "SER123"  -- stops at FNC1, does not read "10LOT456..."
e.BatchLot;     // "LOT456"
e.Gtin;         // "09521234543213"
```

`ToScanString` is the inverse: it inserts an FNC1 after every variable-length value that is not the last element, and never after a fixed-length value.

## Check-digit guarantee

GTIN (AI 01, AI 02) and SSCC (AI 00) values are validated against the GS1 mod-10 check digit **on parse**. An invalid check digit raises `Gs1ParseException` (or makes `TryParse` return `false`), so a parsed element string is never silently wrong on its keys.

```csharp
Gs1Check.Gtin("09521234543213"); // true  (check digit 3)
Gs1Check.Gtin("09521234543214"); // false
Gs1Check.Sscc("006141411234567890"); // true  (check digit 0)
Gs1Check.ComputeCheckDigit("0952123454321"); // 3
```

The algorithm weights the rightmost data digit by 3, the next by 1, alternating towards the left, and the check digit makes the weighted sum a multiple of 10. It is identical for GTIN-8/12/13/14 and SSCC-18.

## GS1 Digital Link

Read and build the common primary-key / key-qualifier pattern:

```csharp
var e = Gs1.Parse("(01)09521234543213(10)A1B2C3(21)XYZ789");

string uri = Gs1DigitalLink.Build(e, "example.com");
// https://example.com/01/09521234543213/10/A1B2C3/21/XYZ789

var back = Gs1DigitalLink.Parse(uri); // same elements
```

The GTIN (01) is the primary key; the consumer product variant (22), batch/lot (10) and serial (21) are carried as path key-qualifiers in GS1 canonical order; any other AIs are carried in the query string.

## Dates

AIs 11, 12, 13, 15, 16 and 17 carry a `YYMMDD` date. The two-digit year is resolved against the current year using the GS1 century rule, and a day of `00` means the last day of the month.

```csharp
Gs1Date.Parse("261231");            // resolves century against the current year
Gs1Date.Parse("261231", 2026);      // 2026-12-31, deterministic reference year
Gs1Date.Parse("260200", 2026);      // 2026-02-28 (day 00 => last day of month)
```

## The AI dictionary

`ApplicationIdentifiers` covers the **common** set of AIs (identification keys, dates, measures 310n-369n with implied decimals, logistics, internal AIs 90-99, and more) with titles and value formats. It is deliberately **not** the full GS1 register. The dictionary is a single, documented table and is straightforward to extend by adding entries.

## Scope and caveats

- The AI dictionary is a curated common subset, not exhaustive. Unknown AIs are rejected rather than guessed.
- Variable-length maximum lengths follow the GS1 General Specifications for the covered AIs.
- Parsing validates structure and the GTIN/SSCC check digits; it does not validate every AI's internal value syntax (for example the component structure of a GDTI or GSIN beyond length).
- Digital Link support targets the common GTIN key-qualifier pattern, not every compressed or alternate-key variant.

## License

MIT. Copyright (c) 2026 Israel Iyonsi.
