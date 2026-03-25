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

        if (!string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = "Please select a JSON file (.json)."
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

        if (string.IsNullOrWhiteSpace(filePath))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = "Please provide a file path."
            });

            return result;
        }

        if (!string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = "Please save to a JSON file (.json)."
            });

            return result;
        }

        var directoryPath = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = "The target directory does not exist."
            });
            return result;
        }

        if (!File.Exists(filePath))
        {
            return result;
        }

        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.None);
        }
        catch (Exception ex)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(filePath),
                Message = $"The file cannot be opened for writing: {ex.Message}"
            });
        }

        return result;
    }

    public ValidationResult ValidateDocumentStructure(LimitaDocument document)
    {
        var result = new ValidationResult();

        for (var i = 0; i < document.Sequences.Count; i++)
        {
            var sequence = document.Sequences[i];
            if (string.IsNullOrWhiteSpace(sequence.SeqName))
            {
                result.AddIssue(new ValidationIssue
                {
                    Target = $"Sequences[{i}].SeqName",
                    Message = "Sequence name cannot be empty."
                });
            }

            for (var j = 0; j < sequence.StepList.Count; j++)
            {
                var step = sequence.StepList[j];
                if (string.IsNullOrWhiteSpace(step.StepName))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Target = $"Sequences[{i}].StepList[{j}].StepName",
                        Message = "Step name cannot be empty."
                    });
                }
            }
        }

        return result;
    }
}
