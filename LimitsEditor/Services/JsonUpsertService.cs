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

        if (string.IsNullOrWhiteSpace(request.TestItem.TestName))
        {
            return new UpsertResult
            {
                Status = OperationStatus.ValidationFailed,
                Message = "Test name is required."
            };
        }

        var sequence = document.Sequences.FirstOrDefault(s =>
            string.Equals(s.SequenceName, request.SequenceName, StringComparison.OrdinalIgnoreCase));

        var sequenceCreated = false;
        if (sequence is null)
        {
            sequence = new Sequence { SequenceName = request.SequenceName };
            document.Sequences.Add(sequence);
            sequenceCreated = true;
        }

        var existingTest = sequence.TestItems.FirstOrDefault(t =>
            string.Equals(t.TestName, request.TestItem.TestName, StringComparison.OrdinalIgnoreCase));

        if (existingTest is not null && !request.OverwriteIfExists)
        {
            return new UpsertResult
            {
                Status = OperationStatus.Conflict,
                SequenceCreated = sequenceCreated,
                RequiresOverwriteConfirmation = true,
                Message = "A test with this name already exists in the selected sequence."
            };
        }

        var clonedItem = Clone(request.TestItem);

        if (existingTest is null)
        {
            sequence.TestItems.Add(clonedItem);
            return new UpsertResult
            {
                Status = OperationStatus.Success,
                SequenceCreated = sequenceCreated,
                TestAdded = true,
                Message = sequenceCreated
                    ? "Created sequence and added test."
                    : "Added test to existing sequence."
            };
        }

        var existingIndex = sequence.TestItems.IndexOf(existingTest);
        sequence.TestItems[existingIndex] = clonedItem;

        return new UpsertResult
        {
            Status = OperationStatus.Success,
            SequenceCreated = sequenceCreated,
            TestOverwritten = true,
            Message = "Existing test overwritten."
        };
    }

    private static TestItem Clone(TestItem item)
    {
        return new TestItem
        {
            TestName = item.TestName,
            TestType = item.TestType,
            TestValues = item.TestValues
                .Select(v => new TestValue
                {
                    ResultType = v.ResultType,
                    ExpectedResult = v.ExpectedResult,
                    Comparison = v.Comparison,
                    Min = v.Min,
                    Max = v.Max
                })
                .ToList()
        };
    }
}
