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

        LoadFileCommand = new RelayCommand(OnLoadFile);
        SaveFileCommand = new RelayCommand(OnSaveFile, CanSaveFile);
        UpsertTestCommand = new RelayCommand(OnUpsertTest);
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

    public LimitaDocument CurrentDocument { get; }

    public ObservableCollection<Sequence> Sequences { get; }

    public ICommand LoadFileCommand { get; }

    public ICommand SaveFileCommand { get; }

    public ICommand UpsertTestCommand { get; }

    private void OnLoadFile()
    {
        // TODO: Call file validation + JSON load service and hydrate Sequences collection.
        StatusMessage = "Load action not implemented yet.";
    }

    private bool CanSaveFile()
    {
        // TODO: Include validation state and required fields.
        return !string.IsNullOrWhiteSpace(SelectedFilePath);
    }

    private void OnSaveFile()
    {
        // TODO: Create backup then save document via service.
        StatusMessage = "Save action not implemented yet.";
    }

    private void OnUpsertTest()
    {
        // TODO: Validate editor state and call JSON upsert service.
        StatusMessage = "Upsert action not implemented yet.";
    }
}
