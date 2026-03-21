using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Validation;
using AddTestDialogSubmission = LimitsEditor.Services.AddTestDialogSubmission;
using AddTestLimitSubmission = LimitsEditor.Services.AddTestLimitSubmission;

namespace LimitsEditor.ViewModels;

public sealed partial class AddTestDialogViewModel : ObservableObject
{
    private const string SingleStepType = "SINGLE";
    private const string MultipleStepType = "MULTIPLE";

    private readonly EditableLimitViewModel _rootLimitDraft = new();
    private readonly IAddTestCreationValidator _addTestCreationValidator;
    private AddTestSubTestItemViewModel? _lastSelectedSubTestDraft;
    private ValidationResult _currentValidation = new();

    public AddTestDialogViewModel(string sequenceName, IAddTestCreationValidator addTestCreationValidator)
    {
        _addTestCreationValidator = addTestCreationValidator;
        SequenceName = sequenceName;
        availableStepTypes = new[] { SingleStepType, MultipleStepType };
        EditableLimit = _rootLimitDraft;
        _rootLimitDraft.PropertyChanged += OnRootLimitPropertyChanged;
        SubTests.CollectionChanged += OnSubTestsCollectionChanged;
        Revalidate();
    }

    public string SequenceName { get; }

    [ObservableProperty]
    private string editableStepName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMultipleTest))]
    [NotifyPropertyChangedFor(nameof(ShowSubTestsSection))]
    [NotifyPropertyChangedFor(nameof(IsSubTestItemSelected))]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyPropertyChangedFor(nameof(HasSubTests))]
    private string editableStepType = SingleStepType;

    [ObservableProperty]
    private string statusMessage = "Create a test and confirm to append it to the selected sequence.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasValidationSummary))]
    private string validationSummary = string.Empty;

    [ObservableProperty]
    private bool hasAttemptedConfirm;

    [ObservableProperty]
    private string stepNameError = string.Empty;

    [ObservableProperty]
    private string stepTypeError = string.Empty;

    [ObservableProperty]
    private string subTestsError = string.Empty;

    [ObservableProperty]
    private string currentSubTestNameError = string.Empty;

    [ObservableProperty]
    private string currentLimitTypeError = string.Empty;

    [ObservableProperty]
    private string currentComparisonTypeError = string.Empty;

    [ObservableProperty]
    private string currentResultError = string.Empty;

    [ObservableProperty]
    private string currentRangeError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyPropertyChangedFor(nameof(IsSubTestItemSelected))]
    private AddTestSubTestItemViewModel? selectedSubTest;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyPropertyChangedFor(nameof(IsSubTestItemSelected))]
    private EditableLimitViewModel? editableLimit;

    [ObservableProperty]
    private IReadOnlyList<string> availableStepTypes;

    public ObservableCollection<AddTestSubTestItemViewModel> SubTests { get; } = new();

    public bool IsMultipleTest => string.Equals(EditableStepType, MultipleStepType, StringComparison.OrdinalIgnoreCase);

    public bool ShowSubTestsSection => IsMultipleTest;

    public bool IsSubTestItemSelected => IsMultipleTest && SelectedSubTest is not null;

    public bool HasEditableLimit => EditableLimit is not null;

    public bool HasSubTests => SubTests.Count > 0;

    public bool HasValidationSummary => !string.IsNullOrWhiteSpace(ValidationSummary);

    public bool IsCreationValid => _currentValidation.IsValid;

    public event EventHandler<bool?>? CloseRequested;

    partial void OnEditableStepNameChanged(string value)
    {
        Revalidate();
    }

    partial void OnEditableStepTypeChanged(string value)
    {
        HasAttemptedConfirm = false;

        if (string.Equals(value, MultipleStepType, StringComparison.OrdinalIgnoreCase))
        {
            RestoreMultipleSelection();
        }
        else
        {
            SwitchToSingleTestEditor();
        }

        UpdateStatusMessageForCurrentSelection();
        Revalidate();
    }

    partial void OnSelectedSubTestChanged(AddTestSubTestItemViewModel? oldValue, AddTestSubTestItemViewModel? newValue)
    {
        if (newValue is not null)
        {
            _lastSelectedSubTestDraft = newValue;
        }

        SyncEditableLimit();
        UpdateStatusMessageForCurrentSelection();
        Revalidate();
    }

    [RelayCommand]
    private void AddSubTest()
    {
        var subTestDraft = CreateSubTestDraft();
        SubTests.Add(subTestDraft);
        SelectSubTest(subTestDraft);
        StatusMessage = $"Added sub-test ({SubTests.Count} total).";
        Revalidate();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSubTest))]
    private void DeleteSubTest(AddTestSubTestItemViewModel? subTest)
    {
        if (subTest is null)
        {
            return;
        }

        var deletedIndex = SubTests.IndexOf(subTest);
        if (deletedIndex < 0)
        {
            return;
        }

        var wasSelected = ReferenceEquals(SelectedSubTest, subTest);
        var replacementSelection = wasSelected
            ? GetAdjacentSubTest(deletedIndex)
            : SelectedSubTest;

        if (ReferenceEquals(_lastSelectedSubTestDraft, subTest))
        {
            _lastSelectedSubTestDraft = replacementSelection;
        }

        SubTests.RemoveAt(deletedIndex);

        if (wasSelected)
        {
            SelectSubTest(replacementSelection);
        }
        else
        {
            SyncEditableLimit();
        }

        StatusMessage = SubTests.Count == 0
            ? "Removed the last sub-test. Add a sub-test to continue building this MULTIPLE test."
            : $"Removed sub-test ({SubTests.Count} remaining).";

        Revalidate();
    }

    private bool CanDeleteSubTest(AddTestSubTestItemViewModel? subTest) => subTest is not null;

    [RelayCommand]
    private void Confirm()
    {
        HasAttemptedConfirm = true;
        Revalidate();
        if (!IsCreationValid)
        {
            StatusMessage = "Fix the validation errors before confirming.";
            return;
        }

        CloseRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }

    public AddTestDialogSubmission BuildSubmission()
    {
        return new AddTestDialogSubmission(
            EditableStepName.Trim(),
            IsMultipleTest ? MultipleStepType : SingleStepType,
            IsMultipleTest
                ? SubTests.Select(item => ToSubmission(item.EditableLimit)).ToList()
                : new List<AddTestLimitSubmission> { ToSubmission(_rootLimitDraft) });
    }

    private AddTestSubTestItemViewModel CreateSubTestDraft()
    {
        return new AddTestSubTestItemViewModel(new EditableLimitViewModel());
    }

    private static AddTestLimitSubmission ToSubmission(EditableLimitViewModel source)
    {
        return new AddTestLimitSubmission(
            source.MultipleStepNameCheck,
            source.LimitType,
            source.ComparisonType,
            source.ThresholdType,
            source.ExpectedRes,
            source.Low,
            source.High,
            source.Unit);
    }

    private void OnSubTestsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (AddTestSubTestItemViewModel subTest in e.OldItems)
            {
                subTest.EditableLimit.PropertyChanged -= OnSubTestLimitPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (AddTestSubTestItemViewModel subTest in e.NewItems)
            {
                subTest.EditableLimit.PropertyChanged += OnSubTestLimitPropertyChanged;
            }
        }

        OnPropertyChanged(nameof(HasSubTests));

        if (SelectedSubTest is not null && !SubTests.Contains(SelectedSubTest))
        {
            SelectedSubTest = null;
        }

        if (_lastSelectedSubTestDraft is not null && !SubTests.Contains(_lastSelectedSubTestDraft))
        {
            _lastSelectedSubTestDraft = null;
        }

        if (IsMultipleTest)
        {
            SyncEditableLimit();
        }

        Revalidate();
    }

    private void OnRootLimitPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Revalidate();
    }

    private void OnSubTestLimitPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Revalidate();
    }

    private void RestoreMultipleSelection()
    {
        var preferredSelection = _lastSelectedSubTestDraft is not null && SubTests.Contains(_lastSelectedSubTestDraft)
            ? _lastSelectedSubTestDraft
            : SubTests.FirstOrDefault();

        SelectSubTest(preferredSelection);
    }

    private void SelectSubTest(AddTestSubTestItemViewModel? subTest)
    {
        if (ReferenceEquals(SelectedSubTest, subTest))
        {
            SyncEditableLimit();
            return;
        }

        SelectedSubTest = subTest;
    }

    private AddTestSubTestItemViewModel? GetAdjacentSubTest(int deletedIndex)
    {
        if (SubTests.Count <= 1)
        {
            return null;
        }

        var candidateIndex = deletedIndex < SubTests.Count - 1
            ? deletedIndex + 1
            : deletedIndex - 1;

        return candidateIndex >= 0 && candidateIndex < SubTests.Count
            ? SubTests[candidateIndex]
            : null;
    }

    private void SwitchToSingleTestEditor()
    {
        if (SelectedSubTest is not null)
        {
            _lastSelectedSubTestDraft = SelectedSubTest;
        }

        SelectedSubTest = null;
        EditableLimit = _rootLimitDraft;
    }

    private void SyncEditableLimit()
    {
        EditableLimit = IsMultipleTest
            ? SelectedSubTest?.EditableLimit
            : _rootLimitDraft;
    }

    private void UpdateStatusMessageForCurrentSelection()
    {
        if (!IsMultipleTest)
        {
            StatusMessage = "Editing root fields for a SINGLE test.";
            return;
        }

        if (!HasSubTests)
        {
            StatusMessage = "Add a sub-test to begin building this MULTIPLE test.";
            return;
        }

        StatusMessage = SelectedSubTest is null
            ? "Select a sub-test to edit, or add another sub-test."
            : $"Editing sub-test '{SelectedSubTest.DisplayName}'.";
    }

    private void Revalidate()
    {
        _currentValidation = _addTestCreationValidator.Validate(BuildValidationRequest());
        ApplyValidationState(_currentValidation);
    }

    private AddTestCreationRequest BuildValidationRequest()
    {
        return new AddTestCreationRequest
        {
            StepName = EditableStepName,
            StepType = EditableStepType,
            RootLimit = ToDraft(_rootLimitDraft),
            SubTests = SubTests.Select(item => ToDraft(item.EditableLimit)).ToList()
        };
    }

    private static AddTestLimitDraft ToDraft(EditableLimitViewModel source)
    {
        return new AddTestLimitDraft
        {
            Name = source.MultipleStepNameCheck,
            LimitType = source.LimitType,
            ComparisonType = source.ComparisonType,
            ExpectedRes = source.ExpectedRes,
            Low = source.Low,
            High = source.High
        };
    }

    private void ApplyValidationState(ValidationResult validation)
    {
        foreach (var subTest in SubTests)
        {
            subTest.ValidationMessage = string.Empty;
        }

        if (!HasAttemptedConfirm)
        {
            ClearValidationMessages();
            return;
        }

        StepNameError = GetIssueMessage(AddTestValidationTargets.StepName);
        StepTypeError = GetIssueMessage(AddTestValidationTargets.StepType);
        SubTestsError = GetIssueMessage(AddTestValidationTargets.SubTests);

        for (var index = 0; index < SubTests.Count; index++)
        {
            var subTestPrefix = AddTestValidationTargets.SubTestPrefix(index);
            var subTestIssues = validation.Issues
                .Where(issue => issue.Target.StartsWith(subTestPrefix, StringComparison.Ordinal))
                .Select(issue => issue.Message)
                .Distinct()
                .ToList();

            if (subTestIssues.Count > 0)
            {
                SubTests[index].ValidationMessage = string.Join(" ", subTestIssues);
            }
        }

        var activeLimitValidationTarget = GetActiveLimitValidationTarget();
        CurrentSubTestNameError = IsSubTestItemSelected
            ? GetIssueMessage(activeLimitValidationTarget + AddTestValidationTargets.NameSuffix)
            : string.Empty;
        CurrentLimitTypeError = GetIssueMessage(activeLimitValidationTarget + AddTestValidationTargets.LimitTypeSuffix);
        CurrentComparisonTypeError = GetIssueMessage(activeLimitValidationTarget + AddTestValidationTargets.ComparisonTypeSuffix);
        CurrentResultError = GetIssueMessage(activeLimitValidationTarget + AddTestValidationTargets.ResultSuffix);
        CurrentRangeError = GetIssueMessage(activeLimitValidationTarget + AddTestValidationTargets.RangeSuffix);

        ValidationSummary = validation.IsValid
            ? string.Empty
            : string.Join(Environment.NewLine, validation.Issues.Select(issue => $"• {issue.Message}").Distinct());
    }

    private void ClearValidationMessages()
    {
        StepNameError = string.Empty;
        StepTypeError = string.Empty;
        SubTestsError = string.Empty;
        CurrentSubTestNameError = string.Empty;
        CurrentLimitTypeError = string.Empty;
        CurrentComparisonTypeError = string.Empty;
        CurrentResultError = string.Empty;
        CurrentRangeError = string.Empty;
        ValidationSummary = string.Empty;
    }

    private string GetActiveLimitValidationTarget()
    {
        if (!IsMultipleTest || SelectedSubTest is null)
        {
            return AddTestValidationTargets.RootLimitPrefix;
        }

        var selectedSubTestIndex = SubTests.IndexOf(SelectedSubTest);
        return selectedSubTestIndex >= 0
            ? AddTestValidationTargets.SubTestPrefix(selectedSubTestIndex)
            : AddTestValidationTargets.RootLimitPrefix;
    }

    private string GetIssueMessage(string target)
    {
        return _currentValidation.Issues.FirstOrDefault(issue => string.Equals(issue.Target, target, StringComparison.Ordinal))?.Message ?? string.Empty;
    }
}
