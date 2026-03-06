using System.Windows;
using LimitsEditor.Services;
using LimitsEditor.Validation;
using LimitsEditor.ViewModels;

namespace LimitsEditor.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

        // TODO: Move to app-level dependency injection composition root.
        DataContext = new MainViewModel(
            new JsonFileService(),
            new BackupService(),
            new JsonUpsertService(),
            new FileValidationService(),
            new TestItemValidator());
    }
}
