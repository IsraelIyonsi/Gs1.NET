namespace Gs1;

/// <summary>
/// Parses both supported input forms into an ordered list of elements: the human-readable bracketed
/// form where AIs appear in parentheses, and the GS1-128 / scan form where FNC1 (0x1D) terminates a
/// variable-length value. Fixed-length AIs are read by their defined length; variable-length AIs are
/// read up to the next FNC1 or the end of the string. GTIN (AI 01, 02) and SSCC (AI 00) check digits
/// are validated as they are read.
/// </summary>
internal static class ElementStringParser
{
    internal static Gs1ElementString Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length == 0)
        {
            throw new Gs1ParseException("Input is empty.");
        }

        return IsBracketedForm(input) ? ParseBracketed(input) : ParseScan(input);
    }

    private static bool IsBracketedForm(string input)
    {
        foreach (var c in input)
        {
            if (c == Gs1Constants.FunctionCode1)
            {
                continue;
            }

            return c == Gs1Constants.BracketOpen;
        }

        return false;
    }

    private static Gs1ElementString ParseScan(string input)
    {
        var elements = new List<Gs1Element>();
        var pos = 0;

        while (pos < input.Length)
        {
            if (input[pos] == Gs1Constants.FunctionCode1)
            {
                pos++;
                continue;
            }

            if (!ApplicationIdentifiers.TryGetAiLength(input, pos, out var aiLength) || pos + aiLength > input.Length)
            {
                throw new Gs1ParseException($"Unrecognised Application Identifier prefix at position {pos}.");
            }

            var ai = input.Substring(pos, aiLength);
            pos += aiLength;

            if (!ApplicationIdentifiers.TryGet(ai, out var definition))
            {
                throw new Gs1ParseException($"Unknown Application Identifier '{ai}'.");
            }

            string value;
            if (definition.IsFixedLength)
            {
                if (pos + definition.ValueLength > input.Length)
                {
                    throw new Gs1ParseException($"Value for fixed-length AI '{ai}' is truncated.");
                }

                value = input.Substring(pos, definition.ValueLength);
                if (value.IndexOf(Gs1Constants.FunctionCode1) >= 0)
                {
                    // A separator inside a fixed-length field means the field was shorter than declared.
                    // Do not realign across it; that would silently corrupt every following element.
                    throw new Gs1ParseException($"Value for fixed-length AI '{ai}' contains a premature FNC1 separator.");
                }

                pos += definition.ValueLength;
            }
            else
            {
                var terminator = input.IndexOf(Gs1Constants.FunctionCode1, pos);
                if (terminator < 0)
                {
                    value = input[pos..];
                    pos = input.Length;
                }
                else
                {
                    value = input[pos..terminator];
                    pos = terminator;
                }
            }

            elements.Add(BuildElement(ai, value, definition));
        }

        if (elements.Count == 0)
        {
            throw new Gs1ParseException("Input contained no elements.");
        }

        return new Gs1ElementString(elements);
    }

    private static Gs1ElementString ParseBracketed(string input)
    {
        var elements = new List<Gs1Element>();
        var pos = 0;

        while (pos < input.Length)
        {
            if (input[pos] != Gs1Constants.BracketOpen)
            {
                throw new Gs1ParseException($"Expected '(' at position {pos}.");
            }

            var close = input.IndexOf(Gs1Constants.BracketClose, pos + 1);
            if (close < 0)
            {
                throw new Gs1ParseException("Missing ')' after an Application Identifier.");
            }

            var ai = input[(pos + 1)..close];
            if (ai.Length == 0 || !IsAllDigits(ai))
            {
                throw new Gs1ParseException($"Invalid Application Identifier '{ai}'.");
            }

            pos = close + 1;
            var nextOpen = input.IndexOf(Gs1Constants.BracketOpen, pos);

            string value;
            if (nextOpen < 0)
            {
                value = input[pos..];
                pos = input.Length;
            }
            else
            {
                value = input[pos..nextOpen];
                pos = nextOpen;
            }

            if (!ApplicationIdentifiers.TryGet(ai, out var definition))
            {
                throw new Gs1ParseException($"Unknown Application Identifier '{ai}'.");
            }

            elements.Add(BuildElement(ai, value, definition));
        }

        if (elements.Count == 0)
        {
            throw new Gs1ParseException("Input contained no elements.");
        }

        return new Gs1ElementString(elements);
    }

    /// <summary>
    /// Resolves the definition for <paramref name="ai"/> and validates <paramref name="value"/> against
    /// it (length, numeric data type, and GTIN/SSCC check digit) before constructing the element. Shared
    /// by element-string parsing and GS1 Digital Link parsing so both apply identical rules.
    /// </summary>
    internal static Gs1Element BuildElement(string ai, string value)
    {
        if (!ApplicationIdentifiers.TryGet(ai, out var definition))
        {
            throw new Gs1ParseException($"Unknown Application Identifier '{ai}'.");
        }

        return BuildElement(ai, value, definition);
    }

    private static Gs1Element BuildElement(string ai, string value, ApplicationIdentifier definition)
    {
        if (definition.IsFixedLength)
        {
            if (value.Length != definition.ValueLength)
            {
                throw new Gs1ParseException($"Value for fixed-length AI '{ai}' must be {definition.ValueLength} characters.");
            }
        }
        else
        {
            if (value.Length == 0)
            {
                throw new Gs1ParseException($"Value for variable-length AI '{ai}' is missing.");
            }

            if (value.Length > definition.ValueLength)
            {
                throw new Gs1ParseException($"Value for variable-length AI '{ai}' exceeds {definition.ValueLength} characters.");
            }
        }

        if (definition.IsNumeric && !IsAllDigits(value))
        {
            throw new Gs1ParseException($"Value '{value}' for numeric AI '{ai}' must contain digits only.");
        }

        ValidateCheckDigit(ai, value);
        return new Gs1Element(ai, value, definition.Title);
    }

    private static void ValidateCheckDigit(string ai, string value)
    {
        switch (ai)
        {
            case ApplicationIdentifiers.GtinAi:
            case ApplicationIdentifiers.ContainedGtinAi:
                if (value.Length != Gs1Constants.GtinLength || !Gs1Check.Gtin(value))
                {
                    throw new Gs1ParseException($"GTIN '{value}' has an invalid check digit.");
                }

                break;
            case ApplicationIdentifiers.SsccAi:
                if (!Gs1Check.Sscc(value))
                {
                    throw new Gs1ParseException($"SSCC '{value}' has an invalid check digit.");
                }

                break;
        }
    }

    private static bool IsAllDigits(string value)
    {
        foreach (var c in value)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }
}
