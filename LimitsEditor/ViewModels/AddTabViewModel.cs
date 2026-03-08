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
    private string testName = string.Empty;

    [ObservableProperty]
    private TestType testType = TestType.Single;

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

        TestValues = new ObservableCollection<TestValue> { new() };
        AvailableTestTypes = new[] { TestType.Single, TestType.Multiple };

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.SelectedFilePath))
            {
                OnPropertyChanged(nameof(SelectedFilePath));
            }
        };
    }

    public ObservableCollection<TestValue> TestValues { get; }

    public IReadOnlyList<TestType> AvailableTestTypes { get; }

    public string SelectedFilePath => _sharedFileContext.SelectedFilePath;

    [RelayCommand]
    private void AddTestValue()
    {
        TestValues.Add(new TestValue());
        StatusMessage = $"Added TestValue ({TestValues.Count} total).";
    }

    [RelayCommand]
    private void RemoveTestValue(TestValue? value)
    {
        if (value is null)
        {
            return;
        }

        TestValues.Remove(value);
        if (TestValues.Count == 0)
        {
            TestValues.Add(new TestValue());
        }

        StatusMessage = "Removed TestValue.";
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
            TestItem = new TestItem
            {
                TestName = TestName,
                TestType = TestType,
                TestValues = TestValues.ToList()
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
                TestItem = request.TestItem,
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
