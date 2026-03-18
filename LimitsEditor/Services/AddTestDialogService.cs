using System.Windows;
using LimitsEditor.Models;
using LimitsEditor.ViewModels;
using LimitsEditor.Views;

namespace LimitsEditor.Services;

public sealed class AddTestDialogService : IAddTestDialogService
{
    public Step? ShowDialog(string sequenceName)
    {
        var viewModel = new AddTestDialogViewModel(sequenceName);
        var dialog = new AddTestDialog
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
            var result = dialog.ShowDialog();
            return result == true ? viewModel.BuildStep() : null;
        }
        finally
        {
            viewModel.CloseRequested -= HandleCloseRequested;
        }
    }
}
