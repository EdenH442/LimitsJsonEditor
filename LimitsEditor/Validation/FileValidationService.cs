using LimitsEditor.Models;
using System.IO;

namespace LimitsEditor.Validation;

public sealed class FileValidationService : IFileValidationService
{
    public ValidationResult ValidateFileForLoad(string filePath)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = "Please provide a file path."
            });

            return result;
        }

        if (!File.Exists(filePath))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = "The selected file does not exist."
            });

            return result;
        }

        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception ex)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = $"The file cannot be opened for reading: {ex.Message}"
            });
        }

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
