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

        if (document.Sequences is null)
        {
            result.AddIssue(new ValidationIssue
            {
                Target = nameof(document.Sequences),
                Message = "The JSON document must contain a sequence array, not null."
            });

            return result;
        }

        for (var i = 0; i < document.Sequences.Count; i++)
        {
            var sequence = document.Sequences[i];
            if (sequence is null)
            {
                result.AddIssue(new ValidationIssue
                {
                    Target = $"Sequences[{i}]",
                    Message = $"Sequence {i + 1} cannot be null."
                });
                continue;
            }

            if (string.IsNullOrWhiteSpace(sequence.SequenceName))
            {
                result.AddIssue(new ValidationIssue
                {
                    Target = $"Sequences[{i}].SequenceName",
                    Message = "Sequence name cannot be empty."
                });
            }

            if (sequence.StepList is null)
            {
                result.AddIssue(new ValidationIssue
                {
                    Target = $"Sequences[{i}].StepList",
                    Message = $"The step list in sequence {i + 1} cannot be null."
                });
                continue;
            }

            for (var j = 0; j < sequence.StepList.Count; j++)
            {
                var step = sequence.StepList[j];
                if (step is null)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Target = $"Sequences[{i}].StepList[{j}]",
                        Message = $"Step {j + 1} in sequence {i + 1} cannot be null."
                    });
                    continue;
                }

                if (string.IsNullOrWhiteSpace(step.StepName))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Target = $"Sequences[{i}].StepList[{j}].StepName",
                        Message = "Step name cannot be empty."
                    });
                }

                if (!StepTypeSerialization.TryFromSerialized(step.StepType, out _))
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Target = $"Sequences[{i}].StepList[{j}].StepType",
                        Message = $"Step type for '{step.StepName ?? $"step {j + 1}"}' must be SINGLE or MULTIPLE."
                    });
                }

                if (step.LimitList is null)
                {
                    result.AddIssue(new ValidationIssue
                    {
                        Target = $"Sequences[{i}].StepList[{j}].LimitList",
                        Message = $"The limit list for '{step.StepName ?? $"step {j + 1}"}' cannot be null."
                    });
                    continue;
                }

                for (var k = 0; k < step.LimitList.Count; k++)
                {
                    var limit = step.LimitList[k];
                    if (limit is null)
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Target = $"Sequences[{i}].StepList[{j}].LimitList[{k}]",
                            Message = $"Limit {k + 1} in '{step.StepName ?? $"step {j + 1}"}' cannot be null."
                        });
                        continue;
                    }

                    if (!LimitTypeSerialization.TryFromSerialized(limit.LimitType, out _))
                    {
                        result.AddIssue(new ValidationIssue
                        {
                            Target = $"Sequences[{i}].StepList[{j}].LimitList[{k}].LimitType",
                            Message = $"Limit type for limit {k + 1} in '{step.StepName ?? $"step {j + 1}"}' is missing or unsupported."
                        });
                    }
                }
            }
        }

        return result;
    }
}
