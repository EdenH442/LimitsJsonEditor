using LimitsEditor.Models;

namespace LimitsEditor.Validation;

public interface IFileValidationService
{
    ValidationResult ValidateFileForLoad(string filePath);

    ValidationResult ValidateFileForSave(string filePath);

    ValidationResult ValidateDocumentStructure(LimitaDocument document);
}
