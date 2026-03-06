using LimitsEditor.Models;

namespace LimitsEditor.Validation;

public sealed class TestItemValidator : ITestItemValidator
{
    public ValidationResult Validate(TestItem testItem)
    {
        var result = new ValidationResult();

        // TODO: Enforce business rules (single => exactly one TestValue, multiple => dynamic list).
        // TODO: Add property-level validation for required fields.

        return result;
    }
}
