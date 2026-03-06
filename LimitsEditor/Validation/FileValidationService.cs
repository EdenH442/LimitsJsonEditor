using LimitsEditor.Models;

namespace LimitsEditor.Validation;

public sealed class FileValidationService : IFileValidationService
{
    public ValidationResult ValidateFileForLoad(string filePath)
    {
        var result = new ValidationResult();

        // TODO: Check file existence and access before load.

        return result;
    }

    public ValidationResult ValidateFileForSave(string filePath)
    {
        var result = new ValidationResult();

        // TODO: Check save path/write access before persisting edits.

        return result;
    }

    public ValidationResult ValidateDocumentStructure(LimitaDocument document)
    {
        var result = new ValidationResult();

        // TODO: Check schema-specific JSON structure requirements.

        return result;
    }
}
