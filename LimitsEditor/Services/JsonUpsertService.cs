using LimitsEditor.Models;

namespace LimitsEditor.Services;

public sealed class JsonUpsertService : IJsonUpsertService
{
    public UpsertResult Upsert(LimitaDocument document, UpsertTestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SequenceName))
        {
            return new UpsertResult
            {
                Status = OperationStatus.ValidationFailed,
                Message = "Sequence name is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Step.StepName))
        {
            return new UpsertResult
            {
                Status = OperationStatus.ValidationFailed,
                Message = "Step name is required."
            };
        }

        var sequence = document.Sequences.FirstOrDefault(s =>
            string.Equals(s.SeqName, request.SequenceName, StringComparison.OrdinalIgnoreCase));

        var sequenceCreated = false;
        if (sequence is null)
        {
            sequence = new Sequence { SeqName = request.SequenceName };
            document.Sequences.Add(sequence);
            sequenceCreated = true;
        }

        var existingStep = sequence.StepList.FirstOrDefault(t =>
            string.Equals(t.StepName, request.Step.StepName, StringComparison.OrdinalIgnoreCase));

        if (existingStep is not null && !request.OverwriteIfExists)
        {
            return new UpsertResult
            {
                Status = OperationStatus.Conflict,
                SequenceCreated = sequenceCreated,
                RequiresOverwriteConfirmation = true,
                Message = "A step with this name already exists in the selected sequence."
            };
        }

        var clonedStep = Clone(request.Step);

        if (existingStep is null)
        {
            sequence.StepList.Add(clonedStep);
            return new UpsertResult
            {
                Status = OperationStatus.Success,
                SequenceCreated = sequenceCreated,
                TestAdded = true,
                Message = sequenceCreated
                    ? "Created sequence and added step."
                    : "Added step to existing sequence."
            };
        }

        var existingIndex = sequence.StepList.IndexOf(existingStep);
        sequence.StepList[existingIndex] = clonedStep;

        return new UpsertResult
        {
            Status = OperationStatus.Success,
            SequenceCreated = sequenceCreated,
            TestOverwritten = true,
            Message = "Existing step overwritten."
        };
    }

    private static Step Clone(Step item)
    {
        return new Step
        {
            StepName = item.StepName,
            StepType = item.StepType,
            LimitList = item.LimitList
                .Select(v => new Limit
                {
                    MultipleStepNameCheck = v.MultipleStepNameCheck,
                    LimitType = v.LimitType,
                    ComparisonType = v.ComparisonType,
                    ThresholdType = v.ThresholdType,
                    ExpectedRes = v.ExpectedRes,
                    Low = v.Low,
                    High = v.High,
                    Unit = v.Unit
                })
                .ToList()
        };
    }
}
