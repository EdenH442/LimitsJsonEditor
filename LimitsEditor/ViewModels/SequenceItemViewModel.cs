using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed class SequenceItemViewModel
{
    public SequenceItemViewModel(Sequence model)
    {
        Model = model;
    }

    public Sequence Model { get; }

    public string Name => Model.SeqName;

    public IReadOnlyList<Step> Steps => Model.StepList;
}
