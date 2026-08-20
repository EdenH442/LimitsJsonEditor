using System;
using System.Collections.Generic;

namespace LimitsEditor.Models;

public static class LimitTypeSerialization
{
    public const string ComparisonSerialized = "COMPARISON";
    public const string NumericSerialized = "NUMERIC";
    public const string NoComparisonSerialized = "NO COMPARISON";
    public const string BooleanSerialized = "BOOLEAN";
    public const string StringSerialized = "STRING";

    public static IReadOnlyList<LimitType> All { get; } = new[]
    {
        LimitType.Comparison,
        LimitType.NoComparison,
        LimitType.Boolean,
        LimitType.String
    };

    public static string ToSerialized(LimitType value)
    {
        return value switch
        {
            LimitType.Comparison => ComparisonSerialized,
            LimitType.NoComparison => NoComparisonSerialized,
            LimitType.Boolean => BooleanSerialized,
            LimitType.String => StringSerialized,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported LimitType value.")
        };
    }

    public static LimitType FromSerialized(string value)
    {
        if (TryFromSerialized(value, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Unsupported LimitType '{value}'. Expected {ComparisonSerialized}/{NumericSerialized}, {NoComparisonSerialized}, {BooleanSerialized}, or {StringSerialized}.");
    }

    public static bool TryFromSerialized(string value, out LimitType parsed)
    {
        if (string.Equals(value, ComparisonSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = LimitType.Comparison;
            return true;
        }

        if (string.Equals(value, NumericSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = LimitType.Comparison;
            return true;
        }

        if (string.Equals(value, NoComparisonSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = LimitType.NoComparison;
            return true;
        }

        if (string.Equals(value, BooleanSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = LimitType.Boolean;
            return true;
        }

        if (string.Equals(value, StringSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = LimitType.String;
            return true;
        }

        parsed = default;
        return false;
    }
}
