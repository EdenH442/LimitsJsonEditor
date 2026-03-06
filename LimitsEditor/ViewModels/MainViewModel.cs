using System.Collections.ObjectModel;
using System.Windows.Input;
using LimitsEditor.Commands;
using LimitsEditor.Models;
using LimitsEditor.Services;
using LimitsEditor.Validation;
using MaterialDesignThemes.Wpf;

namespace LimitsEditor.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly IJsonFileService _jsonFileService;
    private readonly IBackupService _backupService;
    private readonly IJsonUpsertService _jsonUpsertService;
    private readonly IFileValidationService _fileValidationService;
    private readonly ITestItemValidator _testItemValidator;
    private readonly PaletteHelper _paletteHelper;

    private string _selectedFilePath = string.Empty;
    private string _statusMessage = "Ready";
    private string _sequenceName = string.Empty;
    private string _testName = string.Empty;
    private TestType _testType = TestType.Single;
    private bool _isDarkMode;

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

        BrowseFileCommand = new RelayCommand(OnBrowseFile);
        LoadFileCommand = new RelayCommand(OnLoadFile);
        ApplyChangesCommand = new RelayCommand(OnApplyChanges);
        AddTestValueCommand = new RelayCommand(OnAddTestValue);
        ToggleThemeCommand = new RelayCommand(OnToggleTheme);

        InitializeThemeState();
    }

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set => SetProperty(ref _selectedFilePath, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string SequenceName
    {
        get => _sequenceName;
        set => SetProperty(ref _sequenceName, value);
    }

    public string TestName
    {
        get => _testName;
        set => SetProperty(ref _testName, value);
    }

    public TestType TestType
    {
        get => _testType;
        set => SetProperty(ref _testType, value);
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        private set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                OnPropertyChanged(nameof(ThemeToggleButtonText));
            }
        }
    }

    public string ThemeToggleButtonText => IsDarkMode ? "Light Mode" : "Dark Mode";

    public LimitaDocument CurrentDocument { get; }

    public ObservableCollection<Sequence> Sequences { get; }

    public ObservableCollection<TestValue> TestValues { get; }

    public IReadOnlyList<TestType> AvailableTestTypes { get; }

    public ICommand BrowseFileCommand { get; }

    public ICommand LoadFileCommand { get; }

    public ICommand ApplyChangesCommand { get; }

    public ICommand AddTestValueCommand { get; }

    public ICommand ToggleThemeCommand { get; }

    private void InitializeThemeState()
    {
        var theme = _paletteHelper.GetTheme();
        IsDarkMode = theme.GetBaseTheme() == BaseTheme.Dark;
    }

    private void OnBrowseFile()
    {
        StatusMessage = "Browse action placeholder (file dialog not implemented yet).";
    }

    private void OnLoadFile()
    {
        // Placeholder only. No file I/O in this task.
        StatusMessage = "Load action placeholder (file loading not implemented yet).";
    }

    private void OnApplyChanges()
    {
        // Placeholder only. No JSON upsert/save logic in this task.
        StatusMessage = "Apply action placeholder (upsert/save not implemented yet).";
    }

    private void OnAddTestValue()
    {
        TestValues.Add(new TestValue());
        StatusMessage = $"Added TestValue placeholder item ({TestValues.Count} total).";
    }

    private void OnToggleTheme()
    {
        var theme = _paletteHelper.GetTheme();
        var enableDarkMode = !IsDarkMode;
        theme.SetBaseTheme(enableDarkMode ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);

        IsDarkMode = enableDarkMode;
        StatusMessage = enableDarkMode ? "Dark mode enabled." : "Light mode enabled.";
    }
}
