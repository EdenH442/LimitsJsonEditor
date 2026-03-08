using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;

namespace LimitsEditor.ViewModels;

public sealed partial class MainEditorViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;
    private Limit? _targetLimit;

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
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    private Step? selectedTest;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditLimitCommand))]
    private Limit? selectedLimit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditCommand))]
    private Limit? editableLimit;

    public MainEditorViewModel(SharedFileContext sharedFileContext)
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

    public bool HasEditableLimit => EditableLimit is not null;

    public Action? DocumentEdited { get; set; }

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
        ClearEditState();

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

    [RelayCommand]
    private void FindSequence()
    {
        var query = SequenceSearchText.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _sharedFileContext.LoadedDocument.Sequences
            : _sharedFileContext.LoadedDocument.Sequences.Where(s => s.SeqName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        ResetAllSelection();
        ReplaceWith(MatchingSequences, matches);

        StatusMessage = $"Found {MatchingSequences.Count} matching sequence(s).";
    }

    [RelayCommand]
    private void AddSequence()
    {
        StatusMessage = "Add Sequence workflow placeholder (not implemented yet).";
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSequence))]
    private void DeleteSequence()
    {
        StatusMessage = "Delete Sequence placeholder (not implemented yet).";
    }

    private bool CanDeleteSequence() => SelectedSequence is not null;

    [RelayCommand]
    private void AddTest()
    {
        StatusMessage = "Add Test workflow placeholder (not implemented yet).";
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTest))]
    private void DeleteTest()
    {
        StatusMessage = "Delete Test placeholder (not implemented yet).";
    }

    private bool CanDeleteTest() => SelectedTest is not null;

    [RelayCommand(CanExecute = nameof(CanEditLimit))]
    private void EditLimit()
    {
        var targetLimit = ResolveLimitForEdit();
        if (targetLimit is null)
        {
            return;
        }

        _targetLimit = targetLimit;
        EditableLimit = CloneLimit(targetLimit);
        SelectedLimit = targetLimit;
        StatusMessage = $"Editing limit for test '{SelectedTest?.StepName}'.";
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (_targetLimit is null || EditableLimit is null)
        {
            return;
        }

        CopyLimitValues(EditableLimit, _targetLimit);
        RefreshSelectedLimitView();
        ClearEditState();
        DocumentEdited?.Invoke();
        StatusMessage = "Applied in-memory edits to selected limit.";
    }

    [RelayCommand(CanExecute = nameof(HasEditableLimit))]
    private void CancelEdit()
    {
        ClearEditState();
        StatusMessage = "Canceled edit changes.";
    }

    private bool CanEditLimit() => ResolveLimitForEdit() is not null;

    private bool CanSaveChanges() => _targetLimit is not null && EditableLimit is not null;

    public void RefreshSelectedLimitView()
    {
        if (SelectedTest is null)
        {
            return;
        }

        var refreshedSelection = ResolveLimitForEdit();
        SelectedLimit = null;
        SelectedLimit = refreshedSelection;
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

    private void ReloadFromSharedDocument()
    {
        ResetAllSelection();
        ClearEditState();
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

    private void ResetAllSelection()
    {
        MatchingSequences.Clear();
        SelectedSequence = null;
        ResetTestAndLimitSelection();
    }

    private void ClearEditState()
    {
        _targetLimit = null;
        EditableLimit = null;
    }

    private static void ReplaceWith<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static void CopyLimitValues(Limit source, Limit destination)
    {
        destination.MultipleStepNameCheck = source.MultipleStepNameCheck;
        destination.LimitType = source.LimitType;
        destination.ComparisonType = source.ComparisonType;
        destination.ThresholdType = source.ThresholdType;
        destination.ExpectedRes = source.ExpectedRes;
        destination.Low = source.Low;
        destination.High = source.High;
        destination.Unit = source.Unit;
    }

    private static Limit CloneLimit(Limit source)
    {
        return new Limit
        {
            MultipleStepNameCheck = source.MultipleStepNameCheck,
            LimitType = source.LimitType,
            ComparisonType = source.ComparisonType,
            ThresholdType = source.ThresholdType,
            ExpectedRes = source.ExpectedRes,
            Low = source.Low,
            High = source.High,
            Unit = source.Unit
        };
    }
}
