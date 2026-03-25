using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed class EditorFilteringSelectionService
{
    public IReadOnlyList<SequenceItemViewModel> BuildFilteredSequences(LimitaDocument document, string query, Action onEdited)
    {
        var trimmedQuery = query.Trim();
        var matches = string.IsNullOrWhiteSpace(trimmedQuery)
            ? document.Sequences
            : document.Sequences
                .Where(sequence => sequence.SeqName.Contains(trimmedQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

        return matches
            .Select(sequence => 
            {
                var vm =new SequenceItemViewModel(sequence);
                vm.Edited = onEdited;
                return vm;
                })
            .ToList();
    }

    public IReadOnlyList<TestItemViewModel> BuildTestsForSequence(SequenceItemViewModel? sequence)
    {
        if (sequence is null)
        {
            return Array.Empty<TestItemViewModel>();
        }

        return sequence.Steps
            .Select(step => new TestItemViewModel(step))
            .ToList();
    }

    public IReadOnlyList<Limit> BuildLimitsForTest(TestItemViewModel? test)
    {
        if (test is null)
        {
            return Array.Empty<Limit>();
        }

        return test.Limits.ToList();
    }
}
