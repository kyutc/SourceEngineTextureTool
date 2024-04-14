using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SourceEngineTextureTool.Services.Image;
using SourceEngineTextureTool.Services.IO;
using SourceEngineTextureTool.ViewModels;
using SourceEngineTextureTool.Views;

namespace SourceEngineTextureTool;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow()
            {
                DataContext = mainWindowViewModel,
            };
            
            ////////////
            // Services
            ////////////
            {
                var services = new ServiceCollection();

                services.AddSingleton<IFileDialogService>(x => new FileDialogService(desktop.MainWindow));
                services.AddSingleton<ImageImporter>(x => new ImageImporter());
                
                // The Current MainWindowViewModel
                services.AddTransient<MainWindowViewModel>(x => 
                    (MainWindowViewModel)desktop.MainWindow.DataContext);
                // The Current project settings
                services.AddTransient<ProjectSettingsViewModel>(x =>
                    FetchService<MainWindowViewModel>().ProjectSettingsViewModel);

                Services = services.BuildServiceProvider();
            }
            
            // The workspace cannot be initialized until service container is built otherwise it'll cause null
            // exceptions to be thrown.
            mainWindowViewModel.InitializeWorkspace();
            
            //////////////////
            // Event handlers
            //////////////////
            desktop.MainWindow.Closing += (sender, args) =>
            {
                // Todo: Ask for verification if workspace has been modified before a vtf has been exported
            };
            desktop.Exit += (sender, args) => _DeleteTemporaryResources();

            
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static IServiceProvider Services { get; private set; }

    public static T FetchService<T>()
    {
        var serviceProvider = Services ??
                              throw new NullReferenceException(
                                  $"Method invoked before property {nameof(Services)} was initialized.");
        return serviceProvider.GetService<T>() ??
               throw new NullReferenceException($"{typeof(T)} service not registered.");
    }

    /// <summary>
    /// Delete temporary directories created by this app when managing resources.
    /// </summary>
    private static void _DeleteTemporaryResources()
    {
        if (Conversion.BaseDir is not null)
        {
            Directory.Delete(Conversion.BaseDir, true);
        }
        if (VtfMaker.BaseDir is not null)
        {
            Directory.Delete(VtfMaker.BaseDir, true);
        }
    }
}