namespace LimitsEditor.Models;

public sealed class Sequence
{
    public string SequenceName { get; set; } = string.Empty;

    public List<TestItem> TestItems { get; set; } = new();
}
