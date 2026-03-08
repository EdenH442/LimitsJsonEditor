using CommunityToolkit.Mvvm.ComponentModel;
using LimitsEditor.Models;

namespace LimitsEditor.Services;

public sealed partial class SharedFileContext : ObservableObject
{
    [ObservableProperty]
    private string selectedFilePath = string.Empty;

    [ObservableProperty]
    private LimitaDocument loadedDocument = new();
}
