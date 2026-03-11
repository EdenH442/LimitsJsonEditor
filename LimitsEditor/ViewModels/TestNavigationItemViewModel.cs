using System.Windows;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed class TestNavigationItemViewModel
{
    public TestNavigationItemViewModel(TestItemViewModel rootTest, Limit? subTestLimit)
    {
        RootTest = rootTest;
        SubTestLimit = subTestLimit;
    }

    public TestItemViewModel RootTest { get; }

    public Limit? SubTestLimit { get; }

    public bool IsRoot => SubTestLimit is null;

    public bool IsSubTest => !IsRoot;


    public Thickness ItemMargin => new(IsRoot ? 0 : 16, 0, 0, 4);

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
