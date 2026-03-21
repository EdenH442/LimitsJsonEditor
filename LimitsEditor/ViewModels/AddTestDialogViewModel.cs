using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Validation;

namespace LimitsEditor.ViewModels;

public sealed partial class AddTestDialogViewModel : ObservableObject
{
    private readonly EditableLimitViewModel _singleTestLimit = new();
    private readonly IAddTestCreationValidator _addTestCreationValidator;
    private AddTestSubTestItemViewModel? _lastSelectedMultipleSubTest;
    private ValidationResult _currentValidation = new();

    public AddTestDialogViewModel(string sequenceName, IAddTestCreationValidator addTestCreationValidator)
    {
        _addTestCreationValidator = addTestCreationValidator;
        SequenceName = sequenceName;
        availableStepTypes = new[] { "SINGLE", "MULTIPLE" };
        EditableLimit = _singleTestLimit;
        _singleTestLimit.PropertyChanged += OnRootLimitPropertyChanged;
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
    private string editableStepType = "SINGLE";

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

    public bool IsMultipleTest => string.Equals(EditableStepType, "MULTIPLE", StringComparison.OrdinalIgnoreCase);

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
        if (string.Equals(value, "MULTIPLE", StringComparison.OrdinalIgnoreCase))
        {
            RestoreMultipleSelection();
            StatusMessage = HasSubTests
                ? "Select a sub-test to edit, or add another sub-test."
                : "Add a sub-test to begin building this MULTIPLE test.";
        }
        else
        {
            if (SelectedSubTest is not null)
            {
                _lastSelectedMultipleSubTest = SelectedSubTest;
            }

            SelectedSubTest = null;
            EditableLimit = _singleTestLimit;
            StatusMessage = "Editing root fields for a SINGLE test.";
        }

        Revalidate();
    }

    partial void OnSelectedSubTestChanged(AddTestSubTestItemViewModel? oldValue, AddTestSubTestItemViewModel? newValue)
    {
        if (newValue is not null)
        {
            _lastSelectedMultipleSubTest = newValue;
        }

        if (IsMultipleTest)
        {
            EditableLimit = newValue?.EditableLimit;
            StatusMessage = newValue is null
                ? "Select a sub-test to edit, or add a new one."
                : $"Editing sub-test '{newValue.DisplayName}'.";
        }

        Revalidate();
    }

    [RelayCommand]
    private void AddSubTest()
    {
        var subTest = new AddTestSubTestItemViewModel(new EditableLimitViewModel());
        SubTests.Add(subTest);
        SelectSubTest(subTest);
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

        var index = SubTests.IndexOf(subTest);
        if (index < 0)
        {
            return;
        }

        var wasSelected = ReferenceEquals(SelectedSubTest, subTest);
        var nextSelection = wasSelected
            ? GetAdjacentSubTest(index)
            : SelectedSubTest;

        if (ReferenceEquals(_lastSelectedMultipleSubTest, subTest))
        {
            _lastSelectedMultipleSubTest = nextSelection;
        }

        SubTests.RemoveAt(index);

        if (wasSelected)
        {
            SelectSubTest(nextSelection);
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

    public Step BuildStep()
    {
        return new Step
        {
            StepName = EditableStepName.Trim(),
            StepType = IsMultipleTest ? "MULTIPLE" : "SINGLE",
            LimitList = IsMultipleTest
                ? SubTests.Select(item => item.EditableLimit.ToModel()).ToList()
                : new List<Limit> { _singleTestLimit.ToModel() }
        };
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

        if (_lastSelectedMultipleSubTest is not null && !SubTests.Contains(_lastSelectedMultipleSubTest))
        {
            _lastSelectedMultipleSubTest = null;
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
        var targetSelection = _lastSelectedMultipleSubTest is not null && SubTests.Contains(_lastSelectedMultipleSubTest)
            ? _lastSelectedMultipleSubTest
            : SubTests.FirstOrDefault();

        SelectSubTest(targetSelection);
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

    private void SyncEditableLimit()
    {
        if (!IsMultipleTest)
        {
            EditableLimit = _singleTestLimit;
            return;
        }

        EditableLimit = SelectedSubTest?.EditableLimit;
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
            RootLimit = ToDraft(_singleTestLimit),
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
            StepNameError = string.Empty;
            StepTypeError = string.Empty;
            SubTestsError = string.Empty;
            CurrentSubTestNameError = string.Empty;
            CurrentLimitTypeError = string.Empty;
            CurrentComparisonTypeError = string.Empty;
            CurrentResultError = string.Empty;
            CurrentRangeError = string.Empty;
            ValidationSummary = string.Empty;
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

        var currentTargetPrefix = IsMultipleTest && SelectedSubTest is not null
            ? AddTestValidationTargets.SubTestPrefix(SubTests.IndexOf(SelectedSubTest))
            : AddTestValidationTargets.RootLimitPrefix;

        CurrentSubTestNameError = IsMultipleTest && SelectedSubTest is not null
            ? GetIssueMessage(currentTargetPrefix + AddTestValidationTargets.NameSuffix)
            : string.Empty;
        CurrentLimitTypeError = GetIssueMessage(currentTargetPrefix + AddTestValidationTargets.LimitTypeSuffix);
        CurrentComparisonTypeError = GetIssueMessage(currentTargetPrefix + AddTestValidationTargets.ComparisonTypeSuffix);
        CurrentResultError = GetIssueMessage(currentTargetPrefix + AddTestValidationTargets.ResultSuffix);
        CurrentRangeError = GetIssueMessage(currentTargetPrefix + AddTestValidationTargets.RangeSuffix);

        ValidationSummary = validation.IsValid
            ? string.Empty
            : string.Join(Environment.NewLine, validation.Issues.Select(issue => $"• {issue.Message}").Distinct());
    }

    private string GetIssueMessage(string target)
    {
        return _currentValidation.Issues.FirstOrDefault(issue => string.Equals(issue.Target, target, StringComparison.Ordinal))?.Message ?? string.Empty;
    }
}
