using System.Collections.ObjectModel;
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
    private StepType stepType = StepType.Single;

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

        Limits = new ObservableCollection<Limit> { CreateLimit() };
        AvailableStepTypes = StepTypeSerialization.All;
        AvailableLimitTypes = LimitTypeSerialization.All;

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        };
    }

    public ObservableCollection<Limit> Limits { get; }

    public IReadOnlyList<StepType> AvailableStepTypes { get; }

    public IReadOnlyList<LimitType> AvailableLimitTypes { get; }

    public string SelectedFilePath => _sharedFileContext.SelectedFilePath;

    [RelayCommand]
    private void AddLimit()
    {
        Limits.Add(CreateLimit());
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
            Limits.Add(CreateLimit());
        }

        StatusMessage = "Removed limit.";
    }

    private static Limit CreateLimit()
    {
        return new Limit
        {
            LimitType = LimitTypeSerialization.ComparisonSerialized
        };
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
                StepType = StepTypeSerialization.ToSerialized(StepType),
                LimitList = Limits.ToList()
            }
        };

        var validation = _testItemValidator.Validate(request);
        if (!validation.IsValid)
        {
            StatusMessage = validation.Issues.FirstOrDefault()?.Message ?? "Validation failed.";
            return;
        }

        var document = _sharedFileContext.LoadedDocument;

        var upsertResult = _jsonUpsertService.Upsert(document, request);
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
