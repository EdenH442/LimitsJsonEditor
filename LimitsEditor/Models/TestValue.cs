namespace LimitsEditor.Models;

public sealed class TestValue
{
    public string ResultType { get; set; } = string.Empty;

    public string ExpectedResult { get; set; } = string.Empty;

    public string Comparison { get; set; } = string.Empty;

    public object? Min { get; set; }

    public object? Max { get; set; }
}
