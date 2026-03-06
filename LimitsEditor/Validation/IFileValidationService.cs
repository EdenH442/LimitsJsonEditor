namespace LimitsEditor.Validation;

public interface IFileValidationService
{
    ValidationResult ValidateForLoad(string filePath);

    ValidationResult ValidateForSave(string filePath);
}
