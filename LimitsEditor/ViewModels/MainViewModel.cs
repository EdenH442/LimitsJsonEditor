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
    private readonly IJsonFileService _jsonFileService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleButtonText))]
    private bool isDarkMode;

    [ObservableProperty]
    private string statusMessage = "Ready";

    public MainViewModel(
        MainEditorViewModel mainEditorViewModel,
        SharedFileContext sharedFileContext,
        IFileValidationService fileValidationService,
        IJsonFileService jsonFileService)
    {
        MainEditor = mainEditorViewModel;
        _sharedFileContext = sharedFileContext;
        _fileValidationService = fileValidationService;
        _jsonFileService = jsonFileService;

        MainEditor.DocumentEdited = MarkDocumentDirty;
        MainEditor.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainEditorViewModel.CurrentFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
                SaveFileCommand.NotifyCanExecuteChanged();
                DiscardUnsavedChangesCommand.NotifyCanExecuteChanged();
            }

            if (args.PropertyName == nameof(MainEditorViewModel.IsDocumentDirty))
            {
                SaveFileCommand.NotifyCanExecuteChanged();
                DiscardUnsavedChangesCommand.NotifyCanExecuteChanged();
            }
        };

        _paletteHelper = new PaletteHelper();
        InitializeThemeState();
    }

    public MainEditorViewModel MainEditor { get; }

    public string SelectedFilePath
    {
        get => MainEditor.CurrentFilePath;
        set => MainEditor.CurrentFilePath = value;
    }

    public string ThemeToggleButtonText => IsDarkMode ? "Light Mode" : "Dark Mode";

    private void InitializeThemeState()
    {
        var theme = _paletteHelper.GetTheme();
        IsDarkMode = theme.GetBaseTheme() == BaseTheme.Dark;
    }

    private void MarkDocumentDirty()
    {
        MainEditor.IsDocumentDirty = true;
        StatusMessage = "Updated selected details. Save file to persist changes.";
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
            StatusMessage = "File selected. Click Open to load the JSON content.";
        }
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var validation = _fileValidationService.ValidateFileForLoad(SelectedFilePath);

        if (!validation.IsValid)
        {
            var message = validation.Issues.FirstOrDefault()?.Message ?? "Unable to open selected file.";
            MessageBox.Show(message, "Open JSON File", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var loadResult = await ReloadSelectedFileFromDiskAsync();
        if (loadResult.Status != OperationStatus.Success || loadResult.Document is null)
        {
            StatusMessage = loadResult.Message;
            return;
        }

        StatusMessage = $"Opened JSON file with {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s).";
    }

    [RelayCommand(CanExecute = nameof(CanSaveFile))]
    private async Task SaveFileAsync()
    {
        var pathValidation = _fileValidationService.ValidateFileForSave(SelectedFilePath);
        if (!pathValidation.IsValid)
        {
            var message = pathValidation.Issues.FirstOrDefault()?.Message ?? "Unable to save file.";
            StatusMessage = message;
            MessageBox.Show(message, "Save JSON File", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var saveResult = await _jsonFileService.SaveAsync(SelectedFilePath, MainEditor.LoadedDocument);
        if (saveResult.Status == OperationStatus.Success)
        {
            MainEditor.IsDocumentDirty = false;
            StatusMessage = saveResult.Message;
            return;
        }

        StatusMessage = $"Save failed. {saveResult.Message}";
        MessageBox.Show(saveResult.Message, "Save JSON File", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private bool CanSaveFile()
    {
        return MainEditor.IsDocumentDirty &&
            !string.IsNullOrWhiteSpace(SelectedFilePath) &&
            MainEditor.LoadedDocument is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDiscardUnsavedChanges))]
    private async Task DiscardUnsavedChangesAsync()
    {
        var validation = _fileValidationService.ValidateFileForLoad(SelectedFilePath);
        if (!validation.IsValid)
        {
            var message = validation.Issues.FirstOrDefault()?.Message ?? "Unable to discard unsaved changes.";
            StatusMessage = message;
            MessageBox.Show(message, "Discard Unsaved Changes", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var loadResult = await ReloadSelectedFileFromDiskAsync();
        if (loadResult.Status == OperationStatus.Success && loadResult.Document is not null)
        {
            StatusMessage = "Discarded unsaved changes and restored the saved file from disk.";
            return;
        }

        StatusMessage = $"Discard failed. {loadResult.Message}";
        MessageBox.Show(loadResult.Message, "Discard Unsaved Changes", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private bool CanDiscardUnsavedChanges()
    {
        return MainEditor.IsDocumentDirty &&
            !string.IsNullOrWhiteSpace(SelectedFilePath) &&
            MainEditor.LoadedDocument is not null;
    }

    private async Task<JsonLoadResult> ReloadSelectedFileFromDiskAsync()
    {
        var loadResult = await _jsonFileService.LoadAsync(SelectedFilePath);
        if (loadResult.Status == OperationStatus.Success && loadResult.Document is not null)
        {
            _sharedFileContext.LoadedDocument = loadResult.Document;
            MainEditor.IsDocumentDirty = false;
        }

        return loadResult;
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
