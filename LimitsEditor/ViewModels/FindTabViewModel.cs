using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;

namespace LimitsEditor.ViewModels;

public sealed partial class FindTabViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string sequenceSearchText = string.Empty;

    [ObservableProperty]
    private Sequence? selectedSequence;

    [ObservableProperty]
    private TestItem? selectedTest;

    [ObservableProperty]
    private string selectedTestDetails = "Select a test to view details.";

    public FindTabViewModel(SharedFileContext sharedFileContext)
    {
        _sharedFileContext = sharedFileContext;

        MatchingSequences = new ObservableCollection<Sequence>();
        TestsInSelectedSequence = new ObservableCollection<TestItem>();

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.LoadedDocument))
            {
                ReloadFromSharedDocument();
            }
            else if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        };

        ReloadFromSharedDocument();
    }

    public ObservableCollection<Sequence> MatchingSequences { get; }

    public ObservableCollection<TestItem> TestsInSelectedSequence { get; }

    public string SelectedFilePath => _sharedFileContext.SelectedFilePath;

    partial void OnSelectedSequenceChanged(Sequence? value)
    {
        TestsInSelectedSequence.Clear();
        SelectedTest = null;

        if (value is null)
        {
            return;
        }

        foreach (var test in value.TestItems)
        {
            TestsInSelectedSequence.Add(test);
        }

        StatusMessage = $"Loaded {TestsInSelectedSequence.Count} test(s) from sequence '{value.SequenceName}'.";
    }

    partial void OnSelectedTestChanged(TestItem? value)
    {
        if (value is null)
        {
            SelectedTestDetails = "Select a test to view details.";
            return;
        }

        var lines = new List<string>
        {
            $"Test Name: {value.TestName}",
            $"Test Type: {value.TestType}",
            "Values:"
        };

        if (value.TestValues.Count == 0)
        {
            lines.Add("  (none)");
        }
        else
        {
            for (var i = 0; i < value.TestValues.Count; i++)
            {
                var entry = value.TestValues[i];
                lines.Add($"  [{i + 1}] ResultType={entry.ResultType}, ExpectedResult={entry.ExpectedResult}, Comparison={entry.Comparison}, Min={entry.Min}, Max={entry.Max}");
            }
        }

        SelectedTestDetails = string.Join(Environment.NewLine, lines);
    }

    [RelayCommand]
    private void FindSequence()
    {
        MatchingSequences.Clear();
        TestsInSelectedSequence.Clear();
        SelectedSequence = null;
        SelectedTest = null;

        var query = SequenceSearchText.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _sharedFileContext.LoadedDocument.Sequences
            : _sharedFileContext.LoadedDocument.Sequences.Where(s => s.SequenceName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var sequence in matches)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Found {MatchingSequences.Count} matching sequence(s).";
    }

    private void ReloadFromSharedDocument()
    {
        MatchingSequences.Clear();
        TestsInSelectedSequence.Clear();
        SelectedSequence = null;
        SelectedTest = null;
        SelectedTestDetails = "Select a test to view details.";

        foreach (var sequence in _sharedFileContext.LoadedDocument.Sequences)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Loaded {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s) from current file context.";
    }
}
