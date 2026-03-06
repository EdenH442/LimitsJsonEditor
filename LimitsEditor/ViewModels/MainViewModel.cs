using System.Collections.ObjectModel;
using System.Windows.Input;
using LimitsEditor.Commands;
using LimitsEditor.Models;
using LimitsEditor.Services;
using LimitsEditor.Validation;

namespace LimitsEditor.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly IJsonFileService _jsonFileService;
    private readonly IBackupService _backupService;
    private readonly IJsonUpsertService _jsonUpsertService;
    private readonly IFileValidationService _fileValidationService;
    private readonly ITestItemValidator _testItemValidator;

    private string _selectedFilePath = string.Empty;
    private string _statusMessage = "Ready";
    private string _sequenceName = string.Empty;
    private string _testName = string.Empty;
    private TestType _testType = TestType.Single;

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

        CurrentDocument = new LimitaDocument();
        Sequences = new ObservableCollection<Sequence>();
        TestValues = new ObservableCollection<TestValue>();
        AvailableTestTypes = new[] { TestType.Single, TestType.Multiple };

        BrowseFileCommand = new RelayCommand(OnBrowseFile);
        LoadFileCommand = new RelayCommand(OnLoadFile);
        ApplyChangesCommand = new RelayCommand(OnApplyChanges);
        AddTestValueCommand = new RelayCommand(OnAddTestValue);
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

    public LimitaDocument CurrentDocument { get; }

    public ObservableCollection<Sequence> Sequences { get; }

    public ObservableCollection<TestValue> TestValues { get; }

    public IReadOnlyList<TestType> AvailableTestTypes { get; }

    public ICommand BrowseFileCommand { get; }

    public ICommand LoadFileCommand { get; }

    public ICommand ApplyChangesCommand { get; }

    public ICommand AddTestValueCommand { get; }

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
}
