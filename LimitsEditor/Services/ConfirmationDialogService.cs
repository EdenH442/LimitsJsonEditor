using System.Windows;

namespace LimitsEditor.Services;

public sealed class ConfirmationDialogService : IConfirmationDialogService
{
    public bool ShowConfirmation(string message, string title)
    {
        return MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning)
            == MessageBoxResult.Yes;
    }
}
