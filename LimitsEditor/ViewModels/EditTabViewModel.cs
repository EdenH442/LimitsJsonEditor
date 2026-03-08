using CommunityToolkit.Mvvm.ComponentModel;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class EditTabViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    private Limit? editableLimit;

    public bool HasEditableLimit => EditableLimit is not null;

    public void BeginEdit(Limit sourceLimit)
    {
        EditableLimit = CloneLimit(sourceLimit);
    }

    public void ClearEdit()
    {
        EditableLimit = null;
    }

    private static Limit CloneLimit(Limit source)
    {
        return new Limit
        {
            MultipleStepNameCheck = source.MultipleStepNameCheck,
            LimitType = source.LimitType,
            ComparisonType = source.ComparisonType,
            ThresholdType = source.ThresholdType,
            ExpectedRes = source.ExpectedRes,
            Low = source.Low,
            High = source.High,
            Unit = source.Unit
        };
    }
}
