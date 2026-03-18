using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace LimitsEditor.ViewModels;

public sealed partial class AddTestSubTestItemViewModel : ObservableObject
{
    public AddTestSubTestItemViewModel(EditableLimitViewModel editableLimit)
    {
        EditableLimit = editableLimit;
        EditableLimit.PropertyChanged += OnEditableLimitPropertyChanged;
    }

    public EditableLimitViewModel EditableLimit { get; }

    public string DisplayName => string.IsNullOrWhiteSpace(EditableLimit.MultipleStepNameCheck)
        ? "(unnamed sub-test)"
        : EditableLimit.MultipleStepNameCheck;

    private void OnEditableLimitPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditableLimitViewModel.MultipleStepNameCheck))
        {
            OnPropertyChanged(nameof(DisplayName));
        }
    }
}
