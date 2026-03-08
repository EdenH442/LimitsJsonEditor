using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;
using LimitsEditor.Validation;

namespace LimitsEditor.ViewModels;

public sealed partial class AddTabViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;
    private readonly IJsonFileService _jsonFileService;
    private readonly IJsonUpsertService _jsonUpsertService;
    private readonly ITestItemValidator _testItemValidator;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string sequenceName = string.Empty;

    [ObservableProperty]
    private string stepName = string.Empty;

    [ObservableProperty]
    private string stepType = "SINGLE";

    [ObservableProperty]
    private bool overwriteExisting;

    public AddTabViewModel(
        SharedFileContext sharedFileContext,
        IJsonFileService jsonFileService,
        IJsonUpsertService jsonUpsertService,
        ITestItemValidator testItemValidator)
    {
        _sharedFileContext = sharedFileContext;
        _jsonFileService = jsonFileService;
        _jsonUpsertService = jsonUpsertService;
        _testItemValidator = testItemValidator;

        Limits = new ObservableCollection<Limit> { new() };
        AvailableStepTypes = new[] { "SINGLE", "MULTIPLE" };

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        };
    }

    public ObservableCollection<Limit> Limits { get; }

    public IReadOnlyList<string> AvailableStepTypes { get; }

    public string SelectedFilePath => _sharedFileContext.SelectedFilePath;

    [RelayCommand]
    private void AddLimit()
    {
        Limits.Add(new Limit());
        StatusMessage = $"Added limit ({Limits.Count} total).";
    }

    [RelayCommand]
    private void RemoveLimit(Limit? value)
    {
        if (value is null)
        {
            return;
        }

        Limits.Remove(value);
        if (Limits.Count == 0)
        {
            Limits.Add(new Limit());
        }

        StatusMessage = "Removed limit.";
    }

    [RelayCommand]
    private async Task ApplyChangesAsync()
    {
        if (string.IsNullOrWhiteSpace(_sharedFileContext.SelectedFilePath))
        {
            StatusMessage = "Select a file path and load it in the header first.";
            return;
        }

        var request = new UpsertTestRequest
        {
            SequenceName = SequenceName,
            Step = new Step
            {
                StepName = StepName,
                StepType = StepType,
                LimitList = Limits.ToList()
            },
            OverwriteIfExists = OverwriteExisting
        };

        var validation = _testItemValidator.Validate(request);
        if (!validation.IsValid)
        {
            StatusMessage = validation.Issues.FirstOrDefault()?.Message ?? "Validation failed.";
            return;
        }

        var document = _sharedFileContext.LoadedDocument;

        var upsertResult = _jsonUpsertService.Upsert(document, request);
        if (upsertResult.RequiresOverwriteConfirmation)
        {
            var overwrite = MessageBox.Show(
                upsertResult.Message + " Overwrite?",
                "Confirm overwrite",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (overwrite != MessageBoxResult.Yes)
            {
                StatusMessage = "Overwrite canceled by user.";
                return;
            }

            request = new UpsertTestRequest
            {
                SequenceName = request.SequenceName,
                Step = request.Step,
                OverwriteIfExists = true
            };

            upsertResult = _jsonUpsertService.Upsert(document, request);
        }

        if (upsertResult.Status != OperationStatus.Success)
        {
            StatusMessage = upsertResult.Message;
            return;
        }

        var saveResult = await _jsonFileService.SaveAsync(_sharedFileContext.SelectedFilePath, document);
        if (saveResult.Status == OperationStatus.Success)
        {
            _sharedFileContext.LoadedDocument = document;
        }

        StatusMessage = saveResult.Message;
    }
}
