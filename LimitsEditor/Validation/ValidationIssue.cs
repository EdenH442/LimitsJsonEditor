namespace LimitsEditor.Validation;

public sealed class ValidationIssue
{
    public string Message { get; init; } = string.Empty;

    public string Target { get; init; } = string.Empty;
}
