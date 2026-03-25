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
    private readonly IAddTestDialogService _addTestDialogService;
    private readonly IConfirmationDialogService _confirmationDialogService;
    private Step? _targetTest;
    private Limit? _targetLimit;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FindSequenceCommand))]
    private string searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedSequence))]
    [NotifyPropertyChangedFor(nameof(CanDeleteSequence))]
    [NotifyCanExecuteChangedFor(nameof(AddTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSequenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    private SequenceItemViewModel? selectedSequence;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedTest))]
    [NotifyPropertyChangedFor(nameof(NotHasSelectedTest))]
    [NotifyPropertyChangedFor(nameof(IsMultipleTestSelected))]
    [NotifyPropertyChangedFor(nameof(IsSingleTestSelected))]
    [NotifyPropertyChangedFor(nameof(CanDeleteTest))]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    [NotifyPropertyChangedFor(nameof(IsSubTestItemSelected))]
    [NotifyPropertyChangedFor(nameof(SelectedRootTestItem))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditCommand))]
    private TestNavigationItemViewModel? selectedTestItem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditCommand))]
    private string editableStepName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditCommand))]
    private string editableStepType = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyPropertyChangedFor(nameof(HasPendingChanges))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditCommand))]
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
        EditorFilteringSelectionService filteringSelectionService,
        IAddTestDialogService addTestDialogService,
        IConfirmationDialogService confirmationDialogService)
    {
        _sharedFileContext = sharedFileContext;
        _filteringSelectionService = filteringSelectionService;
        _addTestDialogService = addTestDialogService;
        _confirmationDialogService = confirmationDialogService;

        FilteredSequences = new ObservableCollection<SequenceItemViewModel>();
        TestNavigationItems = new ObservableCollection<TestNavigationItemViewModel>();

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

    public ObservableCollection<TestNavigationItemViewModel> TestNavigationItems { get; }

    public string CurrentFilePath
    {
        get => _sharedFileContext.SelectedFilePath;
        set => _sharedFileContext.SelectedFilePath = value;
    }

    public LimitaDocument LoadedDocument => _sharedFileContext.LoadedDocument;

    public TestItemViewModel? SelectedTest => SelectedTestItem?.RootTest;

    public IReadOnlyList<string> AvailableStepTypes { get; } = new[] { "SINGLE", "MULTIPLE" };

    public bool IsStepTypeEditable => false;

    public string StepNameError => string.Empty;

    public string StepTypeError => string.Empty;

    public string CurrentSubTestNameError => string.Empty;

    public string CurrentLimitTypeError => string.Empty;

    public string CurrentComparisonTypeError => string.Empty;

    public string CurrentResultError => string.Empty;

    public string CurrentRangeError => string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDeleteTest))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    private TestNavigationItemViewModel? selectedRootTestItem;

    public bool HasSelectedSequence => SelectedSequence is not null;

    public bool HasSelectedTest => SelectedTestItem is not null;

    public bool NotHasSelectedTest => !HasSelectedTest;

    public bool IsMultipleTestSelected => string.Equals(SelectedTest?.Type, "MULTIPLE", StringComparison.OrdinalIgnoreCase);

    public bool IsSingleTestSelected => string.Equals(SelectedTest?.Type, "SINGLE", StringComparison.OrdinalIgnoreCase);


    public bool IsSubTestItemSelected => SelectedTestItem?.IsSubTest == true;

    public bool HasEditableLimit => EditableLimit is not null;

    public bool HasPendingChanges => HasRootChanges() || HasLimitChanges();

    public bool CanDeleteSequence => HasSelectedSequence;

    public bool CanDeleteTest => CanDeleteSelectedRootTest();

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
        RebuildTestNavigation(tests, null);
        StatusMessage = $"Loaded {tests.Count} test(s) from sequence '{value.Name}'.";
    }


    partial void OnSelectedRootTestItemChanged(TestNavigationItemViewModel? value)
    {
        if (value is null || ReferenceEquals(value, SelectedTestItem))
        {
            return;
        }

        SelectedTestItem = value;
    }

    partial void OnSelectedTestItemChanged(TestNavigationItemViewModel? value)
    {
        SelectedLimit = null;

        if (value is null)
        {
            SelectedRootTestItem = null;
            UpdateNavigationSelectionState();
            ClearEditState();
            StatusMessage = HasSelectedSequence ? "No test selected." : StatusMessage;
            return;
        }


        if (value.IsSubTest)
        {
            SelectedLimit = value.SubTestLimit;
        }
        else if (IsSingleTestSelected)
        {
            SelectedLimit = value.RootTest.Limits.FirstOrDefault();
        }

        SyncEditableFromSelection();
        UpdateNavigationSelectionState();
        SelectedRootTestItem = value.IsRoot ? value : null;

        StatusMessage = $"Selected {(value.IsRoot ? "test" : "sub-test")} '{value.DisplayName}'.";
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

    [RelayCommand(CanExecute = nameof(CanAddTest))]
    private void AddTest()
    {
        if (SelectedSequence is null)
        {
            StatusMessage = "Select a sequence before adding a test.";
            return;
        }

        var dialogResult = _addTestDialogService.ShowDialog(SelectedSequence.Name);
        if (!dialogResult.IsConfirmed || dialogResult.Submission is null)
        {
            StatusMessage = "Add test canceled.";
            return;
        }

        var createdStep = CreateStep(dialogResult.Submission);
        SelectedSequence.Model.StepList.Add(createdStep);

        var tests = _filteringSelectionService.BuildTestsForSequence(SelectedSequence);
        RebuildTestNavigation(tests, null);
        SelectRootTest(createdStep);

        IsDocumentDirty = true;
        DocumentEdited?.Invoke();
        StatusMessage = $"Added test '{createdStep.StepName}' to sequence '{SelectedSequence.Name}'.";
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTest))]
    private void DeleteTest()
    {
        if (SelectedSequence is null || SelectedRootTestItem is null)
        {
            StatusMessage = "Select a root test before deleting.";
            return;
        }

        var selectedSequence = SelectedSequence;
        var rootTestToDelete = SelectedRootTestItem.RootTest.Model;

        if (!selectedSequence.Model.StepList.Contains(rootTestToDelete))
        {
            StatusMessage = "The selected root test no longer belongs to the selected sequence.";
            return;
        }

        if (selectedSequence.Model.StepList.Count == 1)
        {
            var confirmationMessage =
                $"Delete '{rootTestToDelete.StepName}'?{Environment.NewLine}{Environment.NewLine}" +
                $"This is the last test in '{selectedSequence.Name}', so the sequence will also be deleted." +
                (IsMultipleRootTest(rootTestToDelete)
                    ? $"{Environment.NewLine}{Environment.NewLine}All sub-tests in this MULTIPLE test will be removed too."
                    : string.Empty);

            var isConfirmed = _confirmationDialogService.ShowConfirmation(
                confirmationMessage,
                "Delete Test");

            if (!isConfirmed)
            {
                StatusMessage = "Delete test canceled.";
                return;
            }

            ResetTestAndLimitSelection();
            SelectedSequence = null;
            LoadedDocument.Sequences.Remove(selectedSequence.Model);
            ApplySequenceFilter();

            IsDocumentDirty = true;
            DocumentEdited?.Invoke();
            StatusMessage = $"Deleted last test '{rootTestToDelete.StepName}' and removed sequence '{selectedSequence.Name}'.";
            return;
        }

        ResetTestAndLimitSelection();
        selectedSequence.Model.StepList.Remove(rootTestToDelete);
        var tests = _filteringSelectionService.BuildTestsForSequence(selectedSequence);
        RebuildTestNavigation(tests, null);

        IsDocumentDirty = true;
        DocumentEdited?.Invoke();
        StatusMessage = $"Deleted root test '{rootTestToDelete.StepName}' from sequence '{selectedSequence.Name}'.";
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (SelectedTestItem is null)
        {
            return;
        }

        if (_targetTest is not null)
        {
            _targetTest.StepName = EditableStepName;
            _targetTest.StepType = EditableStepType;
        }

        if (_targetLimit is not null && EditableLimit is not null)
        {
            EditableLimit.ApplyTo(_targetLimit);
        }

        RefreshSelectedLimitView();
        SyncEditableFromSelection();
        IsDocumentDirty = true;
        DocumentEdited?.Invoke();
        StatusMessage = "Applied in-memory edits to selected item.";
    }

    [RelayCommand(CanExecute = nameof(CanCancelEdit))]
    private void CancelEdit()
    {
        SyncEditableFromSelection();
        StatusMessage = "Reverted unsaved changes in details panel.";
    }

    private bool CanAddTest() => SelectedSequence is not null;

    private bool CanDeleteSelectedRootTest()
    {
        return SelectedSequence is not null
            && SelectedRootTestItem is not null
            && SelectedSequence.Model.StepList.Contains(SelectedRootTestItem.RootTest.Model);
    }

    private bool CanSaveChanges() => SelectedTestItem is not null && HasPendingChanges;

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

        if (IsMultipleTestSelected && IsSubTestItemSelected)
        {
            return SelectedLimit;
        }

        return null;
    }


    private void SelectRootTest(Step step)
    {
        SelectedTestItem = TestNavigationItems.FirstOrDefault(item => item.IsRoot && ReferenceEquals(item.RootTest.Model, step));
    }

    private static Step CreateStep(AddTestDialogSubmission submission)
    {
        return new Step
        {
            StepName = submission.StepName,
            StepType = submission.StepType,
            LimitList = submission.Limits.Select(CreateLimit).ToList()
        };
    }

    private static Limit CreateLimit(AddTestLimitSubmission submission)
    {
        return new Limit
        {
            MultipleStepNameCheck = submission.Name,
            LimitType = submission.LimitType,
            ComparisonType = submission.ComparisonType,
            ThresholdType = submission.ThresholdType,
            ExpectedRes = submission.ExpectedRes,
            Low = submission.Low,
            High = submission.High,
            Unit = submission.Unit
        };
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
        SelectedRootTestItem = null;
        SelectedTestItem = null;
        SelectedLimit = null;
        TestNavigationItems.Clear();
    }

    private void ResetAllSelection()
    {
        if (SelectedSequence is not null)
        {
            SelectedSequence = null;
        }
        else
        {
            ResetTestAndLimitSelection();
        }

        FilteredSequences.Clear();
    }


    private void SyncEditableFromSelection()
    {
        if (SelectedTestItem is null)
        {
            ClearEditState();
            return;
        }

        var step = SelectedTestItem.RootTest.Model;
        _targetTest = step;
        EditableStepName = step.StepName;
        EditableStepType = step.StepType;

        var targetLimit = ResolveLimitForEdit();
        if (targetLimit is null)
        {
            _targetLimit = null;
            EditableLimit = null;
            OnPropertyChanged(nameof(HasPendingChanges));
            return;
        }

        _targetLimit = targetLimit;
        EditableLimit = EditableLimitViewModel.FromModel(targetLimit);
        OnPropertyChanged(nameof(HasPendingChanges));
    }

    private void RebuildTestNavigation(IReadOnlyList<TestItemViewModel> rootTests, TestNavigationItemViewModel? preferredSelection)
    {
        var activeRoot = preferredSelection?.RootTest ?? SelectedTestItem?.RootTest;
        var selectedSubTest = preferredSelection?.IsSubTest == true
            ? preferredSelection.SubTestLimit
            : SelectedTestItem?.IsSubTest == true
                ? SelectedTestItem.SubTestLimit
                : null;

        var rootNavigationItems = new List<TestNavigationItemViewModel>();
        foreach (var rootTest in rootTests)
        {
            var rootItem = new TestNavigationItemViewModel(rootTest, null);
            var isActiveRoot = ReferenceEquals(activeRoot?.Model, rootTest.Model);
            rootItem.IsBranchExpanded = isActiveRoot && rootItem.IsMultipleRoot;

            foreach (var subTest in rootTest.Limits)
            {
                rootItem.SubTests.Add(new TestNavigationItemViewModel(rootTest, subTest));
            }

            rootNavigationItems.Add(rootItem);
        }

        ReplaceWith(TestNavigationItems, rootNavigationItems);

        if (preferredSelection is null && SelectedTestItem is null)
        {
            return;
        }

        TestNavigationItemViewModel? matchedSelection = null;
        if (selectedSubTest is not null)
        {
            matchedSelection = TestNavigationItems
                .SelectMany(root => root.SubTests)
                .FirstOrDefault(item => activeRoot is not null && item.Matches(activeRoot, selectedSubTest));
        }

        if (activeRoot is not null)
        {
            matchedSelection ??= TestNavigationItems.FirstOrDefault(item => item.Matches(activeRoot, null));
        }

        if (matchedSelection is not null)
        {
            SelectedTestItem = matchedSelection;
            UpdateNavigationSelectionState();
        }
    }

    private void UpdateNavigationSelectionState()
    {
        var selectedRootModel = SelectedTestItem?.RootTest.Model;

        foreach (var rootItem in TestNavigationItems)
        {
            var isSelectedRoot = SelectedTestItem?.IsRoot == true && ReferenceEquals(rootItem.RootTest.Model, SelectedTestItem.RootTest.Model);
            rootItem.IsSelected = isSelectedRoot;
            rootItem.IsBranchExpanded = ReferenceEquals(rootItem.RootTest.Model, selectedRootModel) && rootItem.IsMultipleRoot;

            foreach (var subTest in rootItem.SubTests)
            {
                var isSelectedSubTest = SelectedTestItem?.IsSubTest == true
                    && ReferenceEquals(subTest.RootTest.Model, SelectedTestItem.RootTest.Model)
                    && ReferenceEquals(subTest.SubTestLimit, SelectedTestItem.SubTestLimit);
                subTest.IsSelected = isSelectedSubTest;
            }
        }
    }

    private void ClearEditState()
    {
        _targetTest = null;
        EditableStepName = string.Empty;
        EditableStepType = string.Empty;
        _targetLimit = null;
        EditableLimit = null;
        OnPropertyChanged(nameof(HasPendingChanges));
    }

    private bool HasRootChanges()
    {
        if (_targetTest is null)
        {
            return false;
        }

        return !string.Equals(EditableStepName, _targetTest.StepName, StringComparison.Ordinal)
            || !string.Equals(EditableStepType, _targetTest.StepType, StringComparison.Ordinal);
    }

    private bool HasLimitChanges()
    {
        return _targetLimit is not null && EditableLimit is not null && EditableLimit.HasChangesComparedTo(_targetLimit);
    }

    private static void ReplaceWith<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static bool IsMultipleRootTest(Step step)
    {
        return string.Equals(step.StepType, "MULTIPLE", StringComparison.OrdinalIgnoreCase);
    }
}
