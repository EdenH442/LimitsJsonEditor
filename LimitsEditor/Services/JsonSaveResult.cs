using LimitsEditor.Validation;

namespace LimitsEditor.Services;

public sealed class JsonSaveResult
{
    public OperationStatus Status { get; init; } = OperationStatus.Failed;

    public ValidationResult Validation { get; init; } = new();

    public string Message { get; init; } = string.Empty;
}
