namespace LimitsEditor.Validation;

public sealed class AddTestCreationValidator : IAddTestCreationValidator
{
    public ValidationResult Validate(AddTestCreationRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.StepName))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = AddTestValidationTargets.StepName,
                Message = "Test name is required."
            });
        }

        var isSingle = string.Equals(request.StepType, "SINGLE", StringComparison.OrdinalIgnoreCase);
        var isMultiple = string.Equals(request.StepType, "MULTIPLE", StringComparison.OrdinalIgnoreCase);
        if (!isSingle && !isMultiple)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = AddTestValidationTargets.StepType,
                Message = "Test type must be SINGLE or MULTIPLE."
            });

            return result;
        }

        if (isSingle)
        {
            ValidateLimit(result, request.RootLimit, AddTestValidationTargets.RootLimitPrefix, requireName: false);
            return result;
        }

        if (request.SubTests.Count == 0)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = AddTestValidationTargets.SubTests,
                Message = "Add at least one sub-test for a MULTIPLE test."
            });

            return result;
        }

        for (var i = 0; i < request.SubTests.Count; i++)
        {
            ValidateLimit(result, request.SubTests[i], AddTestValidationTargets.SubTestPrefix(i), requireName: true);
        }

        return result;
    }

    private static void ValidateLimit(ValidationResult result, AddTestLimitDraft limit, string targetPrefix, bool requireName)
    {
        if (requireName && string.IsNullOrWhiteSpace(limit.Name))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = targetPrefix + AddTestValidationTargets.NameSuffix,
                Message = "Sub-test name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(limit.LimitType))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = targetPrefix + AddTestValidationTargets.LimitTypeSuffix,
                Message = "Limit type is required."
            });
        }

        if (string.IsNullOrWhiteSpace(limit.ComparisonType))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = targetPrefix + AddTestValidationTargets.ComparisonTypeSuffix,
                Message = "Comparison is required."
            });
        }

        if (string.IsNullOrWhiteSpace(limit.ExpectedRes) && !limit.Low.HasValue && !limit.High.HasValue)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = targetPrefix + AddTestValidationTargets.ResultSuffix,
                Message = "Provide an expected result, a low value, or a high value."
            });
        }

        if (limit.Low.HasValue && limit.High.HasValue && limit.Low > limit.High)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = targetPrefix + AddTestValidationTargets.RangeSuffix,
                Message = "Low cannot be greater than High."
            });
        }
    }
}

public static class AddTestValidationTargets
{
    public const string StepName = "StepName";
    public const string StepType = "StepType";
    public const string SubTests = "SubTests";
    public const string RootLimitPrefix = "RootLimit.";
    public const string NameSuffix = "Name";
    public const string LimitTypeSuffix = "LimitType";
    public const string ComparisonTypeSuffix = "ComparisonType";
    public const string ResultSuffix = "Result";
    public const string RangeSuffix = "Range";

    public static string SubTestPrefix(int index) => $"SubTests[{index}].";
}
