using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;

namespace LimitsEditor.ViewModels;

public sealed partial class FindTabViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;
    private readonly EditTabViewModel _editTabViewModel;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string sequenceSearchText = string.Empty;

    [ObservableProperty]
    private Sequence? selectedSequence;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMultipleTestSelected))]
    [NotifyPropertyChangedFor(nameof(IsSingleTestSelected))]
    [NotifyCanExecuteChangedFor(nameof(EditLimitCommand))]
    private Step? selectedTest;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditLimitCommand))]
    private Limit? selectedLimit;

    public FindTabViewModel(SharedFileContext sharedFileContext, EditTabViewModel editTabViewModel)
    {
        _sharedFileContext = sharedFileContext;
        _editTabViewModel = editTabViewModel;

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

    [RelayCommand(CanExecute = nameof(CanEditLimit))]
    private void EditLimit()
    {
        var limitToEdit = GetCurrentLimitToEdit();
        if (limitToEdit is null)
        {
            return;
        }

        var editableClone = EditableLimitViewModel.FromLimit(limitToEdit);
        _editTabViewModel.BeginEdit(editableClone, $"Editing limit in test '{SelectedTest?.StepName}'.");
        StatusMessage = "Opened selected limit in Edit tab.";
    }

    private bool CanEditLimit()
    {
        return GetCurrentLimitToEdit() is not null;
    }

    private Limit? GetCurrentLimitToEdit()
    {
        if (SelectedTest is null)
        {
            return null;
        }

        if (IsMultipleTestSelected)
        {
            return SelectedLimit;
        }

        if (IsSingleTestSelected)
        {
            return SelectedTest.LimitList.FirstOrDefault();
        }

        return null;
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

        foreach (var sequence in _sharedFileContext.LoadedDocument.Sequences)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Loaded {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s) from current file context.";
    }
}
