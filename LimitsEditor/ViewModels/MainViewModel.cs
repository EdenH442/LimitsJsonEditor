using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;
using LimitsEditor.Validation;
using MaterialDesignThemes.Wpf;

namespace LimitsEditor.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IJsonFileService _jsonFileService;
    private readonly IBackupService _backupService;
    private readonly IJsonUpsertService _jsonUpsertService;
    private readonly IFileValidationService _fileValidationService;
    private readonly ITestItemValidator _testItemValidator;
    private readonly PaletteHelper _paletteHelper;

    [ObservableProperty]
    private string selectedFilePath = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string sequenceName = string.Empty;

    [ObservableProperty]
    private string testName = string.Empty;

    [ObservableProperty]
    private TestType testType = TestType.Single;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleButtonText))]
    private bool isDarkMode;

    public MainViewModel(
        IJsonFileService jsonFileService,
        IBackupService backupService,
        IJsonUpsertService jsonUpsertService,
        IFileValidationService fileValidationService,
        ITestItemValidator testItemValidator)
    {
        _jsonFileService = jsonFileService;
        _backupService = backupService;
        _jsonUpsertService = jsonUpsertService;
        _fileValidationService = fileValidationService;
        _testItemValidator = testItemValidator;
        _paletteHelper = new PaletteHelper();

        CurrentDocument = new LimitaDocument();
        Sequences = new ObservableCollection<Sequence>();
        TestValues = new ObservableCollection<TestValue>();
        AvailableTestTypes = new[] { TestType.Single, TestType.Multiple };

        InitializeThemeState();
    }

    public string ThemeToggleButtonText => IsDarkMode ? "Light Mode" : "Dark Mode";

    public LimitaDocument CurrentDocument { get; }

    public ObservableCollection<Sequence> Sequences { get; }

    public ObservableCollection<TestValue> TestValues { get; }

    public IReadOnlyList<TestType> AvailableTestTypes { get; }

    private void InitializeThemeState()
    {
        var theme = _paletteHelper.GetTheme();
        IsDarkMode = theme.GetBaseTheme() == BaseTheme.Dark;
    }

    [RelayCommand]
    private void BrowseFile()
    {
        StatusMessage = "Browse action placeholder (file dialog not implemented yet).";
    }

    [RelayCommand]
    private void LoadFile()
    {
        var _ = (_jsonFileService, _fileValidationService);
        StatusMessage = "Load action placeholder (file loading not implemented yet).";
    }

    [RelayCommand]
    private void ApplyChanges()
    {
        var _ = (_backupService, _jsonUpsertService, _testItemValidator);
        StatusMessage = "Apply action placeholder (upsert/save not implemented yet).";
    }

    [RelayCommand]
    private void AddTestValue()
    {
        TestValues.Add(new TestValue());
        StatusMessage = $"Added TestValue placeholder item ({TestValues.Count} total).";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var theme = _paletteHelper.GetTheme();
        var enableDarkMode = !IsDarkMode;
        theme.SetBaseTheme(enableDarkMode ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);

        IsDarkMode = enableDarkMode;
        StatusMessage = enableDarkMode ? "Dark mode enabled." : "Light mode enabled.";
    }
}
