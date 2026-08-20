using LimitsEditor.Validation;

namespace LimitsEditor.Services;

public sealed class UpsertResult
{
    public OperationStatus Status { get; init; } = OperationStatus.Failed;

    public bool SequenceCreated { get; init; }

    public bool TestAdded { get; init; }

    public ValidationResult Validation { get; init; } = new();

    public string Message { get; init; } = string.Empty;
}
