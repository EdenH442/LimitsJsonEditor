namespace LimitsEditor.Validation;

public sealed class ValidationError
{
    public string Field { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
