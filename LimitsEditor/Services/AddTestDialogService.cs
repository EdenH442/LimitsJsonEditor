using System.Windows;
using LimitsEditor.Validation;
using LimitsEditor.ViewModels;
using LimitsEditor.Views;

namespace LimitsEditor.Services;

public sealed class AddTestDialogService : IAddTestDialogService
{
    private readonly IAddTestCreationValidator _addTestCreationValidator;

    public AddTestDialogService(IAddTestCreationValidator addTestCreationValidator)
    {
        _addTestCreationValidator = addTestCreationValidator;
    }

    public AddTestDialogResult ShowDialog(string sequenceName)
    {
        var viewModel = new AddTestDialogViewModel(sequenceName, _addTestCreationValidator);
        var dialog = new AddTestDialog
        {
            Owner = Application.Current?.MainWindow,
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
            return result == true
                ? AddTestDialogResult.Confirmed(viewModel.BuildSubmission())
                : AddTestDialogResult.Canceled();
        }
        finally
        {
            viewModel.CloseRequested -= HandleCloseRequested;
        }
    }
}
