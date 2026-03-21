using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LimitsEditor.ViewModels;

public sealed partial class ConfirmationDialogViewModel : ObservableObject
{
    public ConfirmationDialogViewModel(string title, string message, string confirmText = "Delete", string cancelText = "Cancel")
    {
        Title = title;
        Message = message;
        ConfirmText = confirmText;
        CancelText = cancelText;
    }

    public string Title { get; }

    public string Message { get; }

    public string ConfirmText { get; }

    public string CancelText { get; }

    public event EventHandler<bool?>? CloseRequested;

    [RelayCommand]
    private void Confirm()
    {
        CloseRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }
}
