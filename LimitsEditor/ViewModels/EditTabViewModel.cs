using CommunityToolkit.Mvvm.ComponentModel;

namespace LimitsEditor.ViewModels;

public sealed partial class EditTabViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditBuffer))]
    private EditableLimitViewModel? editableLimit;

    [ObservableProperty]
    private string editContextDescription = string.Empty;

    public bool HasEditBuffer => EditableLimit is not null;

    public void BeginEdit(EditableLimitViewModel editableLimit, string contextDescription)
    {
        EditableLimit = editableLimit;
        EditContextDescription = contextDescription;
    }
}
