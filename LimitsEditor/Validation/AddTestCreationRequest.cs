namespace LimitsEditor.Validation;

public sealed class AddTestCreationRequest
{
    public string StepName { get; init; } = string.Empty;

    public string StepType { get; init; } = string.Empty;

    public AddTestLimitDraft RootLimit { get; init; } = new();

    public IReadOnlyList<AddTestLimitDraft> SubTests { get; init; } = Array.Empty<AddTestLimitDraft>();
}

public sealed class AddTestLimitDraft
{
    public string Name { get; init; } = string.Empty;

    public string LimitType { get; init; } = string.Empty;

    public string ComparisonType { get; init; } = string.Empty;

    public string ExpectedRes { get; init; } = string.Empty;

    public double? Low { get; init; }

    public double? High { get; init; }
}
