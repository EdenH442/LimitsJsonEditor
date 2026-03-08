using LimitsEditor.Services;

namespace LimitsEditor.Validation;

public sealed class TestItemValidator : ITestItemValidator
{
    public ValidationResult Validate(UpsertTestRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.SequenceName))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(request.SequenceName),
                Message = "Sequence name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Step.StepName))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(request.Step.StepName),
                Message = "Step name is required."
            });
        }

        if (request.Step.LimitList.Count == 0)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(request.Step.LimitList),
                Message = "At least one limit entry is required."
            });
        }

        return result;
    }
}
