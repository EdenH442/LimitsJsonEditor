using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class TestNavigationItemViewModel : ObservableObject
{
    public TestNavigationItemViewModel(TestItemViewModel rootTest, Limit? subTestLimit)
    {
        RootTest = rootTest;
        SubTestLimit = subTestLimit;
        SubTests = new ObservableCollection<TestNavigationItemViewModel>();
    }

    public TestItemViewModel RootTest { get; }

    public Limit? SubTestLimit { get; }

    public bool IsRoot => SubTestLimit is null;

    public bool IsSubTest => !IsRoot;

    public ObservableCollection<TestNavigationItemViewModel> SubTests { get; }

    [ObservableProperty]
    private bool isBranchExpanded;

    [ObservableProperty]
    private bool isSelected;

    public bool IsMultipleRoot => IsRoot && string.Equals(RootTest.Type, "MULTIPLE", StringComparison.OrdinalIgnoreCase);

    public bool HasSubTests => SubTests.Count > 0;

    public Thickness ItemMargin => new(IsRoot ? 0 : 20, 0, 0, 4);

    public string DisplayName => IsRoot
        ? RootTest.Name
        : string.IsNullOrWhiteSpace(SubTestLimit?.MultipleStepNameCheck)
            ? "(unnamed sub-test)"
            : SubTestLimit.MultipleStepNameCheck;

    public bool Matches(TestItemViewModel rootTest, Limit? subTestLimit)
    {
        return ReferenceEquals(RootTest.Model, rootTest.Model) && ReferenceEquals(SubTestLimit, subTestLimit);
    }
}
