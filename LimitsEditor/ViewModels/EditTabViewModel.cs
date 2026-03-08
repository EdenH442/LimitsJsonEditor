using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class EditTabViewModel : ObservableObject
{
    private Limit? _targetLimit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditableLimit))]
    [NotifyCanExecuteChangedFor(nameof(SaveChangesCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private Limit? editableLimit;

    public bool HasEditableLimit => EditableLimit is not null;

    public Action? SaveRequested { get; set; }

    public Action? CancelRequested { get; set; }

    public void BeginEdit(Limit sourceLimit)
    {
        _targetLimit = sourceLimit;
        EditableLimit = CloneLimit(sourceLimit);
    }

    public void ClearEdit()
    {
        _targetLimit = null;
        EditableLimit = null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        if (_targetLimit is null || EditableLimit is null)
        {
            return;
        }

        CopyLimitValues(EditableLimit, _targetLimit);
        SaveRequested?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(HasEditableLimit))]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }

    private bool CanSaveChanges()
    {
        return _targetLimit is not null && EditableLimit is not null;
    }

    private static void CopyLimitValues(Limit source, Limit destination)
    {
        destination.MultipleStepNameCheck = source.MultipleStepNameCheck;
        destination.LimitType = source.LimitType;
        destination.ComparisonType = source.ComparisonType;
        destination.ThresholdType = source.ThresholdType;
        destination.ExpectedRes = source.ExpectedRes;
        destination.Low = source.Low;
        destination.High = source.High;
        destination.Unit = source.Unit;
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
