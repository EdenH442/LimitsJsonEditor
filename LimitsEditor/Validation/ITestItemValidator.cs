using LimitsEditor.Models;

namespace LimitsEditor.Validation;

public interface ITestItemValidator
{
    ValidationResult Validate(TestItem testItem);
}
