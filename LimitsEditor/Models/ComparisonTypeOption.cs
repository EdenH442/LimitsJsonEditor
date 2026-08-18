namespace LimitsEditor.Models;

public sealed record ComparisonTypeOption(string Code, string DisplayText)
{
    public static IReadOnlyList<ComparisonTypeOption> All { get; } = new[]
    {
        new ComparisonTypeOption("EQ", "EQ (==)"),
        new ComparisonTypeOption("EQT", "EQT (==+/-)"),
        new ComparisonTypeOption("NE", "NE (!=)"),
        new ComparisonTypeOption("GT", "GT (>)"),
        new ComparisonTypeOption("LT", "LT (<)"),
        new ComparisonTypeOption("GE", "GE (>=)"),
        new ComparisonTypeOption("LE", "LE (<=)"),
        new ComparisonTypeOption("GTLT", "GTLT (><)"),
        new ComparisonTypeOption("GELE", "GELE (>=<=)"),
        new ComparisonTypeOption("GELT", "GELT (>=<)"),
        new ComparisonTypeOption("GTLE", "GTLE (><=)"),
        new ComparisonTypeOption("LTGT", "LTGT (<>)"),
        new ComparisonTypeOption("LEGE", "LEGE (<=>=)"),
        new ComparisonTypeOption("LEGT", "LEGT (<=>)"),
        new ComparisonTypeOption("LTGE", "LTGE (<>=)"),
        new ComparisonTypeOption("LOG", "LOG (No Comparison)")
    };

    public static bool IsValidCode(string? value) =>
        All.Any(option => string.Equals(option.Code, value, StringComparison.Ordinal));
}
