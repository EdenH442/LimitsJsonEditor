using CommunityToolkit.Mvvm.ComponentModel;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class EditableLimitViewModel : ObservableObject
{
    [ObservableProperty]
    private string multipleStepNameCheck = string.Empty;

    [ObservableProperty]
    private string limitType = string.Empty;

    [ObservableProperty]
    private string comparisonType = string.Empty;

    [ObservableProperty]
    private string thresholdType = string.Empty;

    [ObservableProperty]
    private string expectedRes = string.Empty;

    [ObservableProperty]
    private double? low;

    [ObservableProperty]
    private double? high;

    [ObservableProperty]
    private string unit = string.Empty;

    public static EditableLimitViewModel FromModel(Limit source)
    {
        return new EditableLimitViewModel
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

    public Limit ToModel()
    {
        return new Limit
        {
            MultipleStepNameCheck = MultipleStepNameCheck,
            LimitType = LimitType,
            ComparisonType = ComparisonType,
            ThresholdType = ThresholdType,
            ExpectedRes = ExpectedRes,
            Low = Low,
            High = High,
            Unit = Unit
        };
    }

    public bool HasChangesComparedTo(Limit target)
    {
        return MultipleStepNameCheck != target.MultipleStepNameCheck ||
               LimitType != target.LimitType ||
               ComparisonType != target.ComparisonType ||
               ThresholdType != target.ThresholdType ||
               ExpectedRes != target.ExpectedRes ||
               Low != target.Low ||
               High != target.High ||
               Unit != target.Unit;
    }
}
