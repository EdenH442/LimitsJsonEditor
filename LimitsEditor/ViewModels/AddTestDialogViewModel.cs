using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class AddTestDialogViewModel : ObservableObject
{
    private readonly EditableLimitViewModel _singleTestLimit = new();
    private AddTestSubTestItemViewModel? _lastSelectedMultipleSubTest;

    public AddTestDialogViewModel(string sequenceName)
    {
        SequenceName = sequenceName;
        availableStepTypes = new[] { "SINGLE", "MULTIPLE" };
        EditableLimit = _singleTestLimit;
        SubTests.CollectionChanged += OnSubTestsCollectionChanged;
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

    public event EventHandler<bool?>? CloseRequested;

    partial void OnEditableStepTypeChanged(string value)
    {
        if (string.Equals(value, "MULTIPLE", StringComparison.OrdinalIgnoreCase))
        {
            RestoreMultipleSelection();
            StatusMessage = HasSubTests
                ? "Select a sub-test to edit, or add another sub-test."
                : "Add a sub-test to begin building this MULTIPLE test.";
            return;
        }

        if (SelectedSubTest is not null)
        {
            _lastSelectedMultipleSubTest = SelectedSubTest;
        }

        SelectedSubTest = null;
        EditableLimit = _singleTestLimit;
        StatusMessage = "Editing root fields for a SINGLE test.";
    }

    partial void OnSelectedSubTestChanged(AddTestSubTestItemViewModel? value)
    {
        if (value is not null)
        {
            _lastSelectedMultipleSubTest = value;
        }

        if (IsMultipleTest)
        {
            EditableLimit = value?.EditableLimit;
            StatusMessage = value is null
                ? "Select a sub-test to edit, or add a new one."
                : $"Editing sub-test '{value.DisplayName}'.";
        }
    }

    [RelayCommand]
    private void AddSubTest()
    {
        var subTest = new AddTestSubTestItemViewModel(new EditableLimitViewModel());
        SubTests.Add(subTest);
        SelectSubTest(subTest);
        StatusMessage = $"Added sub-test ({SubTests.Count} total).";
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
    }

    private bool CanDeleteSubTest(AddTestSubTestItemViewModel? subTest) => subTest is not null;

    [RelayCommand]
    private void Confirm()
    {
        var validationMessage = Validate();
        if (validationMessage is not null)
        {
            StatusMessage = validationMessage;
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

    private string? Validate()
    {
        if (string.IsNullOrWhiteSpace(EditableStepName))
        {
            return "Test name is required.";
        }

        if (!string.Equals(EditableStepType, "SINGLE", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(EditableStepType, "MULTIPLE", StringComparison.OrdinalIgnoreCase))
        {
            return "Test type must be SINGLE or MULTIPLE.";
        }

        if (IsMultipleTest && SubTests.Count == 0)
        {
            return "Add at least one sub-test for a MULTIPLE test.";
        }

        return null;
    }

    private void OnSubTestsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
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
        EditableLimit = IsMultipleTest
            ? SelectedSubTest?.EditableLimit
            : _singleTestLimit;
    }
}
