using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace LimitsEditor.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly PaletteHelper _paletteHelper;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeToggleButtonText))]
    private bool isDarkMode;

    public MainViewModel(AddTabViewModel addTabViewModel, FindTabViewModel findTabViewModel)
    {
        AddTab = addTabViewModel;
        FindTab = findTabViewModel;

        _paletteHelper = new PaletteHelper();
        InitializeThemeState();
    }

    public AddTabViewModel AddTab { get; }

    public FindTabViewModel FindTab { get; }

    public string ThemeToggleButtonText => IsDarkMode ? "Light Mode" : "Dark Mode";

    private void InitializeThemeState()
    {
        var theme = _paletteHelper.GetTheme();
        IsDarkMode = theme.GetBaseTheme() == BaseTheme.Dark;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var theme = _paletteHelper.GetTheme();
        var enableDarkMode = !IsDarkMode;
        theme.SetBaseTheme(enableDarkMode ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);

        IsDarkMode = enableDarkMode;
    }
}
