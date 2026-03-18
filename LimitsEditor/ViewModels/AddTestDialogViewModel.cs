using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class AddTestDialogViewModel : ObservableObject
{
    private readonly EditableLimitViewModel _singleTestLimit = new();

    public AddTestDialogViewModel(string sequenceName)
    {
        SequenceName = sequenceName;
        availableStepTypes = new[] { "SINGLE", "MULTIPLE" };
        EditableLimit = _singleTestLimit;
    }

    public string SequenceName { get; }

    [ObservableProperty]
    private string editableStepName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMultipleTest))]
    [NotifyPropertyChangedFor(nameof(ShowSubTestsSection))]
    [NotifyPropertyChangedFor(nameof(IsSubTestItemSelected))]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
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

    public event EventHandler<bool?>? CloseRequested;

    partial void OnEditableStepTypeChanged(string value)
    {
        if (string.Equals(value, "MULTIPLE", StringComparison.OrdinalIgnoreCase))
        {
            if (SubTests.Count == 0)
            {
                AddSubTest();
                return;
            }

            SelectedSubTest ??= SubTests[0];
            EditableLimit = SelectedSubTest.EditableLimit;
            return;
        }

        SelectedSubTest = null;
        EditableLimit = _singleTestLimit;
    }

    partial void OnSelectedSubTestChanged(AddTestSubTestItemViewModel? value)
    {
        if (IsMultipleTest)
        {
            EditableLimit = value?.EditableLimit;
        }
    }

    [RelayCommand]
    private void AddSubTest()
    {
        var subTest = new AddTestSubTestItemViewModel(new EditableLimitViewModel());
        SubTests.Add(subTest);
        SelectedSubTest = subTest;
        EditableLimit = subTest.EditableLimit;
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

        SubTests.RemoveAt(index);

        if (SelectedSubTest == subTest)
        {
            SelectedSubTest = SubTests.Count == 0
                ? null
                : SubTests[Math.Min(index, SubTests.Count - 1)];
        }

        EditableLimit = SelectedSubTest?.EditableLimit;
        StatusMessage = "Removed sub-test.";
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

}
