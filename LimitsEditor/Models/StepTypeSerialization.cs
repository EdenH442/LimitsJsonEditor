using System;
using System.Collections.Generic;

namespace LimitsEditor.Models;

public static class StepTypeSerialization
{
    public const string SingleSerialized = "SINGLE";
    public const string MultipleSerialized = "MULTIPLE";

    public static IReadOnlyList<StepType> All { get; } = new[] { StepType.Single, StepType.Multiple };

    public static string ToSerialized(StepType value)
    {
        return value switch
        {
            StepType.Single => SingleSerialized,
            StepType.Multiple => MultipleSerialized,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported StepType value.")
        };
    }

    public static StepType FromSerialized(string value)
    {
        if (TryFromSerialized(value, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Unsupported StepType '{value}'. Expected {SingleSerialized} or {MultipleSerialized}.");
    }

    public static bool TryFromSerialized(string value, out StepType parsed)
    {
        if (string.Equals(value, SingleSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = StepType.Single;
            return true;
        }

        if (string.Equals(value, MultipleSerialized, StringComparison.OrdinalIgnoreCase))
        {
            parsed = StepType.Multiple;
            return true;
        }

        parsed = default;
        return false;
    }
}
