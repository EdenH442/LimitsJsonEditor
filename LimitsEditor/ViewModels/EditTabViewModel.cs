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
    private EditableLimitViewModel? editableLimit;

    public bool HasEditableLimit => EditableLimit is not null;

    public Action? SaveRequested { get; set; }

    public Action? CancelRequested { get; set; }

    public void BeginEdit(Limit sourceLimit)
    {
        _targetLimit = sourceLimit;
        EditableLimit = EditableLimitViewModel.FromModel(sourceLimit);
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

        EditableLimit.ApplyTo(_targetLimit);
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
}
