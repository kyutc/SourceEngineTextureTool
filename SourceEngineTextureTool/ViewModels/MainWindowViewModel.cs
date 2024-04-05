using ReactiveUI.Fody.Helpers;

namespace SourceEngineTextureTool.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public ProjectSettingsViewModel ProjectSettingsViewModel { get; set; }

    [Reactive] public TextureViewModel TextureViewModel { get; set; }

    public MainWindowViewModel()
    {
        TextureViewModel = new();
        ProjectSettingsViewModel = new();
    }
}