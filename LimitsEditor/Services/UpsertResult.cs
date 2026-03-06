namespace LimitsEditor.Services;

public sealed class UpsertResult
{
    public bool SequenceCreated { get; init; }

    public bool TestAdded { get; init; }

    public bool TestOverwritten { get; init; }

    public bool RequiresOverwriteConfirmation { get; init; }

    public string Message { get; init; } = string.Empty;
}
