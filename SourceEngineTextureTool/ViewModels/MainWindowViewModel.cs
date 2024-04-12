using System;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;
using SourceEngineTextureTool.Models.Settings;
using SourceEngineTextureTool.Services.IO;

namespace SourceEngineTextureTool.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public ProjectSettingsViewModel ProjectSettingsViewModel { get; set; }

    [Reactive] public TextureViewModel? TextureViewModel { get; set; }

    [Reactive] public ICommand? ExportVtfCommand { get; private set; }

    public MainWindowViewModel()
    {
        ProjectSettingsViewModel = new();
    }

    /// <summary>
    /// Initialize the workspace now that this viewmodel is attached to the top-level window. 
    /// </summary>
    public void InitializeWorkspace()
    {
        TextureViewModel = new();
        // Update the Command when the Texture's properties are modified.
        TextureViewModel.WhenAnyValue(tvm => tvm.TextureIsReady)
            .Subscribe(observer => ExportVtfCommand = ReactiveCommand.Create(ExportVtfFile, observer));
    }

    public async void ExportVtfFile()
    {
        Texture texture = TextureViewModel.Texture;
        Vtf vtfSettings = ProjectSettingsViewModel.VtfSettings; 
        
        int numberOfMipmaps = TextureViewModel.MipmapCount;
        int numberOfFramesPerMipmap = TextureViewModel.FrameCount;
        int numberOfFacesPerFrame = 1;
        int numberOfSlicesPerFace = 1;
        string[,,,] highResFiles = new string[numberOfMipmaps, numberOfFramesPerMipmap, numberOfFacesPerFrame, numberOfSlicesPerFace];
        for (int mipmap = 0; mipmap < highResFiles.GetLength(0); mipmap++)
        {
            for (int frame = 0; frame < highResFiles.GetLength(1); frame++)
            {
                for (int face = 0; face < highResFiles.GetLength(2); face++)
                {
                    for (int slice = 0; slice < highResFiles.GetLength(3); slice++)
                    {
                        highResFiles[mipmap, frame, face, slice] = texture.Mipmaps[mipmap].Frames[frame].DropImage.ConvertedImage;
                    }
                }
            }
        }
        
        // Todo: Select a low res file
        string? lowResFile = null;
        
        var saveFileLocation = await App.FetchService<IFileDialogService>().SaveVtfFileDialogAsync(highResFiles, lowResFile, vtfSettings);
    }
}