using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;
using LimitsEditor.Validation;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Windows;

namespace LimitsEditor.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private const int FindTabIndex = 1;
    private const int EditTabIndex = 2;

    private readonly PaletteHelper _paletteHelper;
    private readonly SharedFileContext _sharedFileContext;
    private readonly IFileValidationService _fileValidationService;
    private readonly IJsonFileService _jsonFileService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleButtonText))]
    private bool isDarkMode;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private int selectedTabIndex;

    [ObservableProperty]
    private bool isEditTabEnabled;

    [ObservableProperty]
    private bool isDocumentDirty;

    public MainViewModel(
        AddTabViewModel addTabViewModel,
        FindTabViewModel findTabViewModel,
        EditTabViewModel editTabViewModel,
        SharedFileContext sharedFileContext,
        IFileValidationService fileValidationService,
        IJsonFileService jsonFileService)
    {
        AddTab = addTabViewModel;
        FindTab = findTabViewModel;
        EditTab = editTabViewModel;
        _sharedFileContext = sharedFileContext;
        _fileValidationService = fileValidationService;
        _jsonFileService = jsonFileService;

        FindTab.EditRequested = BeginLimitEdit;
        EditTab.SaveRequested = CompleteEditSave;
        EditTab.CancelRequested = CompleteEditCancel;

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

    public EditTabViewModel EditTab { get; }

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

    private void BeginLimitEdit(Limit limit)
    {
        EditTab.BeginEdit(limit);
        IsEditTabEnabled = true;
        SelectedTabIndex = EditTabIndex;
    }

    private void CompleteEditSave()
    {
        IsDocumentDirty = true;
        EditTab.ClearEdit();
        IsEditTabEnabled = false;
        SelectedTabIndex = FindTabIndex;
        StatusMessage = "Saved in-memory changes for selected limit.";
    }

    private void CompleteEditCancel()
    {
        EditTab.ClearEdit();
        IsEditTabEnabled = false;
        SelectedTabIndex = FindTabIndex;
        StatusMessage = "Canceled edit and discarded unsaved changes.";
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
            OpenFileCommand.Execute(null);
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

        var loadResult = await _jsonFileService.LoadAsync(SelectedFilePath);
        if (loadResult.Status != OperationStatus.Success || loadResult.Document is null)
        {
            StatusMessage = loadResult.Message;
            return;
        }

        _sharedFileContext.LoadedDocument = loadResult.Document;
        EditTab.ClearEdit();
        IsEditTabEnabled = false;
        IsDocumentDirty = false;
        if (SelectedTabIndex == EditTabIndex)
        {
            SelectedTabIndex = FindTabIndex;
        }

        StatusMessage = $"Opened JSON file with {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s).";
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
