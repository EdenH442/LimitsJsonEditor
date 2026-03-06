using LimitsEditor.Models;
using LimitsEditor.Validation;

namespace LimitsEditor.Services;

public sealed class JsonLoadResult
{
    public OperationStatus Status { get; init; } = OperationStatus.Failed;

    public LimitaDocument? Document { get; init; }

    public ValidationResult Validation { get; init; } = new();

    public string Message { get; init; } = string.Empty;
}
