using LimitsEditor.Validation;

namespace LimitsEditor.Services;

public sealed class BackupResult
{
    public OperationStatus Status { get; init; } = OperationStatus.Failed;

    public string BackupFilePath { get; init; } = string.Empty;

    public ValidationResult Validation { get; init; } = new();

    public string Message { get; init; } = string.Empty;
}
