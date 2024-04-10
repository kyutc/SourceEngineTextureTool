using System;
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            var services = new ServiceCollection();

            services.AddSingleton<IFileDialogService>(x => new FileDialogService(desktop.MainWindow));
            services.AddSingleton<ImageImporter>(x => new ImageImporter());

            Services = services.BuildServiceProvider();
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
}