using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;

namespace LimitsEditor.ViewModels;

public sealed partial class MainEditorViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;
    private readonly EditorFilteringSelectionService _filteringSelectionService;
    private Limit? _targetLimit;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FindSequenceCommand))]
    private string searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSequence))]
    [NotifyPropertyChangedFor(nameof(CanDeleteSequence))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSequenceCommand))]
    private SequenceItemViewModel? selectedSequence;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedTest))]
    [NotifyPropertyChangedFor(nameof(IsMultipleTestSelected))]
    [NotifyPropertyChangedFor(nameof(IsSingleTestSelected))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTest))]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    private TestItemViewModel? selectedTest;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    private Limit? selectedLimit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditCommand))]
    private EditableLimitViewModel? editableLimit;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    private bool isDocumentDirty;

    public MainEditorViewModel(
        SharedFileContext sharedFileContext,
        EditorFilteringSelectionService filteringSelectionService)
    {
        _sharedFileContext = sharedFileContext;
        _filteringSelectionService = filteringSelectionService;

        FilteredSequences = new ObservableCollection<SequenceItemViewModel>();
        SelectedSequenceTests = new ObservableCollection<TestItemViewModel>();
        LimitsInSelectedTest = new ObservableCollection<Limit>();

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(CurrentFilePath));
            }

            if (args.PropertyName == nameof(SharedFileContext.LoadedDocument))
            {
                OnPropertyChanged(nameof(LoadedDocument));
                IsDocumentDirty = false;
                ReloadFromSharedDocument();
            }
        };

        ReloadFromSharedDocument();
    }

    public ObservableCollection<SequenceItemViewModel> FilteredSequences { get; }

    public ObservableCollection<TestItemViewModel> SelectedSequenceTests { get; }

    public ObservableCollection<Limit> LimitsInSelectedTest { get; }

    public string CurrentFilePath
    {
        get => _sharedFileContext.SelectedFilePath;
        set => _sharedFileContext.SelectedFilePath = value;
    }

    public LimitaDocument LoadedDocument => _sharedFileContext.LoadedDocument;

    public bool HasSelectedSequence => SelectedSequence is not null;

    public bool HasSelectedTest => SelectedTest is not null;

    public bool IsMultipleTestSelected => string.Equals(SelectedTest?.Type, "MULTIPLE", StringComparison.OrdinalIgnoreCase);

    public bool IsSingleTestSelected => string.Equals(SelectedTest?.Type, "SINGLE", StringComparison.OrdinalIgnoreCase);

    public bool HasEditableLimit => EditableLimit is not null;

    public bool HasPendingChanges => _targetLimit is not null && EditableLimit is not null && EditableLimit.HasChangesComparedTo(_targetLimit);

    public bool CanDeleteSequence => HasSelectedSequence;

    public bool CanDeleteTest => HasSelectedTest;

    public Action? DocumentEdited { get; set; }

    partial void OnSearchTextChanged(string value)
    {
        ApplySequenceFilter();
    }

    partial void OnSelectedSequenceChanged(SequenceItemViewModel? value)
    {
        ResetTestAndLimitSelection();

        if (value is null)
        {
            StatusMessage = "No sequence selected.";
            return;
        }

        var tests = _filteringSelectionService.BuildTestsForSequence(value);
        ReplaceWith(SelectedSequenceTests, tests);
        StatusMessage = $"Loaded {SelectedSequenceTests.Count} test(s) from sequence '{value.Name}'.";
    }

    partial void OnSelectedTestChanged(TestItemViewModel? value)
    {
        LimitsInSelectedTest.Clear();
        SelectedLimit = null;

        if (value is null)
        {
            ClearEditState();
            StatusMessage = HasSelectedSequence ? "No test selected." : StatusMessage;
            return;
        }

        var limits = _filteringSelectionService.BuildLimitsForTest(value);
        ReplaceWith(LimitsInSelectedTest, limits);

        if (IsSingleTestSelected)
        {
            SelectedLimit = value.Limits.FirstOrDefault();
        }

        SyncEditableFromSelection();
        StatusMessage = $"Loaded {LimitsInSelectedTest.Count} limit(s) from test '{value.Name}'.";
    }


    partial void OnSelectedLimitChanged(Limit? value)
    {
        SyncEditableFromSelection();
    }


    partial void OnEditableLimitChanged(EditableLimitViewModel? oldValue, EditableLimitViewModel? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= OnEditableLimitPropertyChanged;
        }

        if (newValue is not null)
        {
            newValue.PropertyChanged += OnEditableLimitPropertyChanged;
        }

        SaveChangesCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasPendingChanges));
    }

    private void OnEditableLimitPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        SaveChangesCommand.NotifyCanExecuteChanged();
        CancelEditCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasPendingChanges));
    }

    [RelayCommand]
    private void FindSequence()
    {
        ApplySequenceFilter();
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

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (_targetLimit is null || EditableLimit is null)
        {
            return;
        }

        CopyLimitValues(EditableLimit, _targetLimit);
        RefreshSelectedLimitView();
        SyncEditableFromSelection();
        IsDocumentDirty = true;
        DocumentEdited?.Invoke();
        StatusMessage = "Applied in-memory edits to selected limit.";
    }

    [RelayCommand(CanExecute = nameof(CanCancelEdit))]
    private void CancelEdit()
    {
        SyncEditableFromSelection();
        StatusMessage = "Reverted unsaved changes in details panel.";
    }

    private bool CanSaveChanges() => _targetLimit is not null && EditableLimit is not null && HasPendingChanges;

    private bool CanCancelEdit() => HasPendingChanges;

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
            return SelectedTest.Limits.FirstOrDefault();
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
        ApplySequenceFilter();

        StatusMessage = $"Loaded {LoadedDocument.Sequences.Count} sequence(s) from current file context.";
    }

    private void ApplySequenceFilter()
    {
        var filteredSequences = _filteringSelectionService.BuildFilteredSequences(LoadedDocument, SearchText);

        ResetAllSelection();
        ReplaceWith(FilteredSequences, filteredSequences);

        StatusMessage = $"Showing {FilteredSequences.Count} sequence(s).";
    }

    private void ResetTestAndLimitSelection()
    {
        SelectedSequenceTests.Clear();
        LimitsInSelectedTest.Clear();
        SelectedTest = null;
        SelectedLimit = null;
    }

    private void ResetAllSelection()
    {
        FilteredSequences.Clear();
        SelectedSequence = null;
        ResetTestAndLimitSelection();
    }


    private void SyncEditableFromSelection()
    {
        var targetLimit = ResolveLimitForEdit();
        if (targetLimit is null)
        {
            ClearEditState();
            return;
        }

        _targetLimit = targetLimit;
        EditableLimit = EditableLimitViewModel.FromModel(targetLimit);
        OnPropertyChanged(nameof(HasPendingChanges));
    }

    private void ClearEditState()
    {
        _targetLimit = null;
        EditableLimit = null;
        OnPropertyChanged(nameof(HasPendingChanges));
    }

    private static void ReplaceWith<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static void CopyLimitValues(EditableLimitViewModel source, Limit destination)
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
}
