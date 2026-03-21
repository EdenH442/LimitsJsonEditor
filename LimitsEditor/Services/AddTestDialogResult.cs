using System.Collections.Generic;

namespace LimitsEditor.Services;

public sealed record AddTestDialogResult(bool IsConfirmed, AddTestDialogSubmission? Submission)
{
    public static AddTestDialogResult Confirmed(AddTestDialogSubmission submission) => new(true, submission);

    public static AddTestDialogResult Canceled() => new(false, null);
}

public sealed record AddTestDialogSubmission(
    string StepName,
    string StepType,
    IReadOnlyList<AddTestLimitSubmission> Limits);

public sealed record AddTestLimitSubmission(
    string Name,
    string LimitType,
    string ComparisonType,
    string ThresholdType,
    string ExpectedRes,
    double? Low,
    double? High,
    string Unit);
