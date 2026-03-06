namespace LimitsEditor.Validation;

public sealed class FileValidationService : IFileValidationService
{
    public ValidationResult ValidateForLoad(string filePath)
    {
        var result = new ValidationResult();

        // TODO: Check file existence and access before load.

        return result;
    }

    public ValidationResult ValidateForSave(string filePath)
    {
        var result = new ValidationResult();

        // TODO: Check save path/write access before persisting edits.

        return result;
    }
}
