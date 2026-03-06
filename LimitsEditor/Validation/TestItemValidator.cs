using LimitsEditor.Services;

namespace LimitsEditor.Validation;

public sealed class TestItemValidator : ITestItemValidator
{
    public ValidationResult Validate(UpsertTestRequest request)
    {
        var result = new ValidationResult();

        // TODO: Enforce business rules (single => exactly one TestValue, multiple => dynamic list).
        // TODO: Add property-level validation for required fields.

        return result;
    }
}
