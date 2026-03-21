namespace LimitsEditor.Validation;

public interface IAddTestCreationValidator
{
    ValidationResult Validate(AddTestCreationRequest request);
}
