using LimitsEditor.Services;

namespace LimitsEditor.Validation;

public interface ITestItemValidator
{
    ValidationResult Validate(UpsertTestRequest request);
}
