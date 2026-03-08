using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace LimitsEditor.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly PaletteHelper _paletteHelper;
    private readonly SharedFileContext _sharedFileContext;
    private readonly IJsonFileService _jsonFileService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleButtonText))]
    private bool isDarkMode;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MainViewModel(
        AddTabViewModel addTabViewModel,
        FindTabViewModel findTabViewModel,
        SharedFileContext sharedFileContext,
        IJsonFileService jsonFileService)
    {
        AddTab = addTabViewModel;
        FindTab = findTabViewModel;
        _sharedFileContext = sharedFileContext;
        _jsonFileService = jsonFileService;

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
            OnPropertyChanged(nameof(SelectedFilePath));
            StatusMessage = "Selected JSON file path.";
        }
    }

    [RelayCommand]
    private async Task LoadFileAsync()
    {
        var result = await _jsonFileService.LoadAsync(SelectedFilePath);

        if (result.Status == OperationStatus.NotFound)
        {
            _sharedFileContext.LoadedDocument = new LimitaDocument();
            StatusMessage = "File not found. A new document will be created on save.";
            return;
        }

        if (result.Status != OperationStatus.Success || result.Document is null)
        {
            StatusMessage = result.Message;
            return;
        }

        _sharedFileContext.LoadedDocument = result.Document;
        StatusMessage = $"Loaded {result.Document.Sequences.Count} sequence(s).";
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
