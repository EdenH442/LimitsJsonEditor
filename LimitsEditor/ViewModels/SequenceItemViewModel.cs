using CommunityToolkit.Mvvm.ComponentModel;
using LimitsEditor.Models;

namespace LimitsEditor.ViewModels;

public sealed partial class SequenceItemViewModel : ObservableObject
{
    public SequenceItemViewModel(Sequence model)
    {
        Model = model;
    }

    public Sequence Model { get; }

    public string Name => Model.SeqName;

    public IReadOnlyList<Step> Steps => Model.StepList;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string editableName = string.Empty;

    public Action? Edited { get; set; } //notify the rest of the app about changes 

    public void BeginEdit()
    {
        EditableName = Name;
        IsEditing = true;
    }

    public void CommitEdit()
    {
        var trimmed = EditableName.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            EditableName = Name;
            IsEditing = false;
            return;
        }

        if (string.Equals(trimmed, Name, StringComparison.Ordinal))
        {
            IsEditing = false;
            return;
        }

        Model.SeqName = trimmed;
        OnPropertyChanged(nameof(Name));
        IsEditing = false;
            Edited?.Invoke();

    }

    public void CancelEdit()
    {
        EditableName = Name;
        IsEditing = false;
    }

}
