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

    #region File Commands

    /// <summary>
    /// Initialize the workspace for working on a VTF.
    /// </summary>
    public void InitializeWorkspace()
    {
        // Todo: Open a dialog that queries whether the user wants to work on a texture, cubemap, spheremap, etc.
        InitializeTextureWorkspace();
    }

    /// <summary>
    /// Initialize the workspace now that this viewmodel is attached to the top-level window. 
    /// </summary>
    private void InitializeTextureWorkspace()
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

        int mipmaps = TextureViewModel.MipmapCount;
        int frames = TextureViewModel.FrameCount;
        int faces = 1;
        int slices = 1;
        string[,,,] highResFiles = new string[mipmaps, frames, faces, slices];
        
        for (int mipmap = 0; mipmap < mipmaps; mipmap++)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                for (int face = 0; face < faces; face++)
                {
                    for (int slice = 0; slice < slices; slice++)
                    {
                        highResFiles[mipmaps - mipmap - 1, frame, face, slice] = texture.Mipmaps[mipmap].Frames[frame].DropImage.ConvertedImage;
                    }
                }
            }
        }

        // Todo: Select a low res file
        string? lowResFile = null;

        var saveFileLocation = await App.FetchService<IFileDialogService>()
            .SaveVtfFileDialogAsync(highResFiles, lowResFile, vtfSettings);
    }

    #endregion File Commands
    
}