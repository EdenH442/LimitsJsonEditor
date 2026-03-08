using CommunityToolkit.Mvvm.ComponentModel;

namespace LimitsEditor.Services;

public sealed partial class SharedFileContext : ObservableObject
{
    [ObservableProperty]
    private string selectedFilePath = string.Empty;
}
