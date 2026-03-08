using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimitsEditor.Models;
using LimitsEditor.Services;

namespace LimitsEditor.ViewModels;

public sealed partial class FindTabViewModel : ObservableObject
{
    private readonly SharedFileContext _sharedFileContext;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string sequenceSearchText = string.Empty;

    [ObservableProperty]
    private Sequence? selectedSequence;

    [ObservableProperty]
    private Step? selectedStep;

    [ObservableProperty]
    private string selectedStepDetails = "Select a step to view details.";

    public FindTabViewModel(SharedFileContext sharedFileContext)
    {
        _sharedFileContext = sharedFileContext;

        MatchingSequences = new ObservableCollection<Sequence>();
        StepsInSelectedSequence = new ObservableCollection<Step>();

        _sharedFileContext.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SharedFileContext.LoadedDocument))
            {
                ReloadFromSharedDocument();
            }
        };

        ReloadFromSharedDocument();
    }

    public ObservableCollection<Sequence> MatchingSequences { get; }

    public ObservableCollection<Step> StepsInSelectedSequence { get; }


    partial void OnSelectedSequenceChanged(Sequence? value)
    {
        StepsInSelectedSequence.Clear();
        SelectedStep = null;

        if (value is null)
        {
            return;
        }

        foreach (var step in value.StepList)
        {
            StepsInSelectedSequence.Add(step);
        }

        StatusMessage = $"Loaded {StepsInSelectedSequence.Count} step(s) from sequence '{value.SeqName}'.";
    }

    partial void OnSelectedStepChanged(Step? value)
    {
        if (value is null)
        {
            SelectedStepDetails = "Select a step to view details.";
            return;
        }

        var lines = new List<string>
        {
            $"Step Name: {value.StepName}",
            $"Step Type: {value.StepType}",
            "Limits:"
        };

        if (value.LimitList.Count == 0)
        {
            lines.Add("  (none)");
        }
        else
        {
            for (var i = 0; i < value.LimitList.Count; i++)
            {
                var entry = value.LimitList[i];
                lines.Add($"  [{i + 1}] MultipleStepNameCheck={entry.MultipleStepNameCheck}, LimitType={entry.LimitType}, ComparisonType={entry.ComparisonType}, ThresholdType={entry.ThresholdType}, ExpectedRes={entry.ExpectedRes}, Low={entry.Low}, High={entry.High}, Unit={entry.Unit}");
            }
        }

        SelectedStepDetails = string.Join(Environment.NewLine, lines);
    }

    [RelayCommand]
    private void FindSequence()
    {
        MatchingSequences.Clear();
        StepsInSelectedSequence.Clear();
        SelectedSequence = null;
        SelectedStep = null;

        var query = SequenceSearchText.Trim();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _sharedFileContext.LoadedDocument.Sequences
            : _sharedFileContext.LoadedDocument.Sequences.Where(s => s.SeqName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var sequence in matches)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Found {MatchingSequences.Count} matching sequence(s).";
    }

    private void ReloadFromSharedDocument()
    {
        MatchingSequences.Clear();
        StepsInSelectedSequence.Clear();
        SelectedSequence = null;
        SelectedStep = null;
        SelectedStepDetails = "Select a step to view details.";

        foreach (var sequence in _sharedFileContext.LoadedDocument.Sequences)
        {
            MatchingSequences.Add(sequence);
        }

        StatusMessage = $"Loaded {_sharedFileContext.LoadedDocument.Sequences.Count} sequence(s) from current file context.";
    }
}
