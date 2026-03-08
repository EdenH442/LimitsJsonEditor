using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;
using Microsoft.Win32;

namespace LimitsEditor.ViewModels;

public sealed partial class FindTabViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;
    private readonly IJsonFileService _jsonFileService;

    private LimitaDocument _loadedDocument = new();

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

    public FindTabViewModel(SharedFileContext sharedFileContext, IJsonFileService jsonFileService)
    {
        _sharedFileContext = sharedFileContext;
        _jsonFileService = jsonFileService;

        MatchingSequences = new ObservableCollection<Sequence>();
        TestsInSelectedSequence = new ObservableCollection<TestItem>();

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        };
    }

    public ObservableCollection<Sequence> MatchingSequences { get; }

    public ObservableCollection<TestItem> TestsInSelectedSequence { get; }

    public string SelectedFilePath
    {
        get => _sharedFileContext.SelectedFilePath;
        set => _sharedFileContext.SelectedFilePath = value;
    }

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
    private void BrowseFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
            FileName = string.IsNullOrWhiteSpace(SelectedFilePath) ? string.Empty : SelectedFilePath
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
            OnPropertyChanged(nameof(SelectedFilePath));
            StatusMessage = "Selected JSON file path.";
        }
    }

    [RelayCommand]
    private async Task LoadFileAsync()
    {
        var result = await _jsonFileService.LoadAsync(SelectedFilePath);

        if (result.Status != OperationStatus.Success || result.Document is null)
        {
            StatusMessage = result.Message;
            return;
        }

        _loadedDocument = result.Document;
        MatchingSequences.Clear();
        TestsInSelectedSequence.Clear();
        SelectedSequence = null;
        SelectedTest = null;
        SelectedTestDetails = "Select a test to view details.";

        foreach (var sequence in _loadedDocument.Sequences)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Loaded {_loadedDocument.Sequences.Count} sequence(s).";
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
            ? _loadedDocument.Sequences
            : _loadedDocument.Sequences.Where(s => s.SequenceName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var sequence in matches)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Found {MatchingSequences.Count} matching sequence(s).";
    }
}
