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
    [NotifyCanExecuteChangedFor(nameof(EditLimitCommand))]
    private Step? selectedTest;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditLimitCommand))]
    private Limit? selectedLimit;

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

    public Action<Limit>? EditRequested { get; set; }

    public bool IsMultipleTestSelected => string.Equals(SelectedTest?.StepType, "MULTIPLE", StringComparison.OrdinalIgnoreCase);

    public bool IsSingleTestSelected => string.Equals(SelectedTest?.StepType, "SINGLE", StringComparison.OrdinalIgnoreCase);

    partial void OnSelectedSequenceChanged(Sequence? value)
    {
        ResetTestAndLimitSelection();

        if (value is null)
        {
            return;
        }

        ReplaceWith(TestsInSelectedSequence, value.StepList);
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

        ReplaceWith(LimitsInSelectedTest, value.LimitList);

        if (IsSingleTestSelected)
        {
            SelectedLimit = value.LimitList.FirstOrDefault();
        }

        StatusMessage = $"Loaded {LimitsInSelectedTest.Count} limit(s) from test '{value.StepName}'.";
    }

    [RelayCommand(CanExecute = nameof(CanEditLimit))]
    private void EditLimit()
    {
        var targetLimit = ResolveLimitForEdit();
        if (targetLimit is null)
        {
            return;
        }

        SelectedLimit = targetLimit;
        EditRequested?.Invoke(targetLimit);
        StatusMessage = $"Prepared edit state for limit in test '{SelectedTest?.StepName}'.";
    }

    private bool CanEditLimit()
    {
        return ResolveLimitForEdit() is not null;
    }

    private Limit? ResolveLimitForEdit()
    {
        if (SelectedTest is null)
        {
            return null;
        }

        if (IsSingleTestSelected)
        {
            return SelectedTest.LimitList.FirstOrDefault();
        }

        if (IsMultipleTestSelected)
        {
            return SelectedLimit;
        }

        return null;
    }

    [RelayCommand]
    private void FindSequence()
    {
        var query = SequenceSearchText.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _sharedFileContext.LoadedDocument.Sequences
            : _sharedFileContext.LoadedDocument.Sequences.Where(s => s.SeqName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        ResetAllFindState();
        ReplaceWith(MatchingSequences, matches);

        StatusMessage = $"Found {MatchingSequences.Count} matching sequence(s).";
    }

    private void ReloadFromSharedDocument()
    {
        ResetAllFindState();
        ReplaceWith(MatchingSequences, _sharedFileContext.LoadedDocument.Sequences);

        StatusMessage = $"Loaded {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s) from current file context.";
    }

    private void ResetTestAndLimitSelection()
    {
        TestsInSelectedSequence.Clear();
        LimitsInSelectedTest.Clear();
        SelectedTest = null;
        SelectedLimit = null;
    }

    private void ResetAllFindState()
    {
        MatchingSequences.Clear();
        SelectedSequence = null;
        ResetTestAndLimitSelection();
    }

    private static void ReplaceWith<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}
