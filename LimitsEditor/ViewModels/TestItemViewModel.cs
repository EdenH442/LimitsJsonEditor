using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed class TestItemViewModel
{
    public TestItemViewModel(Step model)
    {
        Model = model;
    }

    public Step Model { get; }

    public string Name => Model.StepName;

    public string Type => Model.StepType;

    public IReadOnlyList<Limit> Limits => Model.LimitList;
}
