using System;
using System.Windows;
using LimitsEditor.Services;
using LimitsEditor.Validation;
using LimitsEditor.ViewModels;
using LimitsEditor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LimitsEditor;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<SharedFileContext>();
        services.AddSingleton<IJsonFileService, JsonFileService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IJsonUpsertService, JsonUpsertService>();
        services.AddSingleton<IFileValidationService, FileValidationService>();
        services.AddSingleton<ITestItemValidator, TestItemValidator>();

        services.AddSingleton<AddTabViewModel>();
        services.AddSingleton<EditTabViewModel>();
        services.AddSingleton<FindTabViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainView>();
    }
}
