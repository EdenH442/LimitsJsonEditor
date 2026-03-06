using LimitsEditor.Models;

namespace LimitsEditor.Services;

public sealed class UpsertTestRequest
{
    public string SequenceName { get; init; } = string.Empty;

    public TestItem TestItem { get; init; } = new();

    public bool OverwriteIfExists { get; init; }
}
