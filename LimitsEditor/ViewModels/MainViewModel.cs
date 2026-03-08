using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Services;
using LimitsEditor.Validation;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Windows;

namespace LimitsEditor.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly PaletteHelper _paletteHelper;
    private readonly SharedFileContext _sharedFileContext;
    private readonly IFileValidationService _fileValidationService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleButtonText))]
    private bool isDarkMode;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MainViewModel(
        AddTabViewModel addTabViewModel,
        FindTabViewModel findTabViewModel,
        SharedFileContext sharedFileContext,
        IFileValidationService fileValidationService)
    {
        AddTab = addTabViewModel;
        FindTab = findTabViewModel;
        _sharedFileContext = sharedFileContext;
        _fileValidationService = fileValidationService;

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        };

        _paletteHelper = new PaletteHelper();
        InitializeThemeState();
    }

    public AddTabViewModel AddTab { get; }

    public FindTabViewModel FindTab { get; }

    public string SelectedFilePath
    {
        get => _sharedFileContext.SelectedFilePath;
        set => _sharedFileContext.SelectedFilePath = value;
    }

    public string ThemeToggleButtonText => IsDarkMode ? "Light Mode" : "Dark Mode";

    private void InitializeThemeState()
    {
        var theme = _paletteHelper.GetTheme();
        IsDarkMode = theme.GetBaseTheme() == BaseTheme.Dark;
    }

    [RelayCommand]
    private void BrowseFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = false,
            FileName = string.IsNullOrWhiteSpace(SelectedFilePath) ? string.Empty : SelectedFilePath
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
            OpenFile();
        }
    }

    [RelayCommand]
    private void OpenFile()
    {
        var validation = _fileValidationService.ValidateFileForLoad(SelectedFilePath);

        if (!validation.IsValid)
        {
            var message = validation.Issues.FirstOrDefault()?.Message ?? "Unable to open selected file.";
            MessageBox.Show(message, "Open JSON File", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        StatusMessage = "Opened Json File Successfully";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var theme = _paletteHelper.GetTheme();
        var enableDarkMode = !IsDarkMode;
        theme.SetBaseTheme(enableDarkMode ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);

        IsDarkMode = enableDarkMode;
    }
}
