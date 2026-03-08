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
    [NotifyPropertyChangedFor(nameof(IsMultipleTestSelected))]
    [NotifyPropertyChangedFor(nameof(IsSingleTestSelected))]
    private Step? selectedTest;

    [ObservableProperty]
    private Limit? selectedLimit;

    [ObservableProperty]
    private string selectedLimitSummary = "Select a limit to view details.";

    public FindTabViewModel(SharedFileContext sharedFileContext)
    {
        _sharedFileContext = sharedFileContext;

        MatchingSequences = new ObservableCollection<Sequence>();
        TestsInSelectedSequence = new ObservableCollection<Step>();
        LimitsInSelectedTest = new ObservableCollection<Limit>();

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.LoadedDocument))
            {
                ReloadFromSharedDocument();
            }
        };

        ReloadFromSharedDocument();
    }

    public ObservableCollection<Sequence> MatchingSequences { get; }

    public ObservableCollection<Step> TestsInSelectedSequence { get; }

    public ObservableCollection<Limit> LimitsInSelectedTest { get; }

    public bool IsMultipleTestSelected => string.Equals(SelectedTest?.StepType, "MULTIPLE", StringComparison.OrdinalIgnoreCase);

    public bool IsSingleTestSelected => string.Equals(SelectedTest?.StepType, "SINGLE", StringComparison.OrdinalIgnoreCase);

    partial void OnSelectedSequenceChanged(Sequence? value)
    {
        TestsInSelectedSequence.Clear();
        LimitsInSelectedTest.Clear();
        SelectedTest = null;
        SelectedLimit = null;

        if (value is null)
        {
            return;
        }

        foreach (var test in value.StepList)
        {
            TestsInSelectedSequence.Add(test);
        }

        StatusMessage = $"Loaded {TestsInSelectedSequence.Count} test(s) from sequence '{value.SeqName}'.";
    }

    partial void OnSelectedTestChanged(Step? value)
    {
        LimitsInSelectedTest.Clear();
        SelectedLimit = null;

        if (value is null)
        {
            return;
        }

        foreach (var limit in value.LimitList)
        {
            LimitsInSelectedTest.Add(limit);
        }

        if (IsSingleTestSelected)
        {
            SelectedLimit = value.LimitList.FirstOrDefault();
        }

        StatusMessage = $"Loaded {LimitsInSelectedTest.Count} limit(s) from test '{value.StepName}'.";
    }

    partial void OnSelectedLimitChanged(Limit? value)
    {
        SelectedLimitSummary = value is null
            ? "Select a limit to view details."
            : BuildLimitSummary(value, IsMultipleTestSelected);
    }

    [RelayCommand]
    private void EditLimit(Limit? limit)
    {
        if (limit is null)
        {
            return;
        }

        SelectedLimit = limit;
        StatusMessage = $"Prepared edit state for limit in test '{SelectedTest?.StepName}'.";
    }

    [RelayCommand]
    private void FindSequence()
    {
        MatchingSequences.Clear();
        TestsInSelectedSequence.Clear();
        LimitsInSelectedTest.Clear();
        SelectedSequence = null;
        SelectedTest = null;
        SelectedLimit = null;

        var query = SequenceSearchText.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _sharedFileContext.LoadedDocument.Sequences
            : _sharedFileContext.LoadedDocument.Sequences.Where(s => s.SeqName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

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
        LimitsInSelectedTest.Clear();
        SelectedSequence = null;
        SelectedTest = null;
        SelectedLimit = null;
        SelectedLimitSummary = "Select a limit to view details.";

        foreach (var sequence in _sharedFileContext.LoadedDocument.Sequences)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Loaded {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s) from current file context.";
    }

    private static string BuildLimitSummary(Limit limit, bool includeMultipleStepName)
    {
        var baseSummary = $"LimitType={limit.LimitType}, Comparison={limit.ComparisonType}, ThresholdType={ToDisplay(limit.ThresholdType)}, Expected={ToDisplay(limit.ExpectedRes)}, Low={ToDisplay(limit.Low)}, High={ToDisplay(limit.High)}, Unit={ToDisplay(limit.Unit)}";
        return includeMultipleStepName
            ? $"MultipleStepNameCheck={ToDisplay(limit.MultipleStepNameCheck)}, {baseSummary}"
            : baseSummary;
    }

    private static string ToDisplay(double? value)
    {
        return value?.ToString() ?? "-";
    }

    private static string ToDisplay(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
