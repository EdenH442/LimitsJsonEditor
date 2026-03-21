using System.Windows;
using LimitsEditor.ViewModels;
using LimitsEditor.Views;

namespace LimitsEditor.Services;

public sealed class ConfirmationDialogService : IConfirmationDialogService
{
    public bool ShowConfirmation(string message, string title)
    {
        var viewModel = new ConfirmationDialogViewModel(title, message);
        var dialog = new ConfirmationDialog
        {
            Owner = Application.Current.MainWindow,
            DataContext = viewModel
        };

        void HandleCloseRequested(object? sender, bool? dialogResult)
        {
            dialog.DialogResult = dialogResult;
            dialog.Close();
        }

        viewModel.CloseRequested += HandleCloseRequested;
        try
        {
            return dialog.ShowDialog() == true;
        }
        finally
        {
            viewModel.CloseRequested -= HandleCloseRequested;
        }
    }
}
