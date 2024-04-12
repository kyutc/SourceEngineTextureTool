using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using SourceEngineTextureTool.Models.Settings;
using SourceEngineTextureTool.Services.Image;

namespace SourceEngineTextureTool.Services.IO;

/// <summary>
/// Opens a file dialog.
/// </summary>
public class FileDialogService : IFileDialogService
{
    public readonly Dictionary<string, FilePickerFileType> SupportedFileTypes = new()
    {
        // Supported image types are any that ImageMagick can process into a PNG.
        ["Image"] = new FilePickerFileType("All Images")
        {
            Patterns = new[]
                // Todo: Add any missing file types that we want to support
                { "*.bmp", "*.gif", "*.jpg", "*.jpeg", "*.pdf", "*.png", "*.svg", "*.tga", "*.tiff", "*.webp" },
            AppleUniformTypeIdentifiers = new[] { "public.image" },
            MimeTypes = new[] { "image/*" }
        },
        
        ["Vtf"] = new FilePickerFileType("Valve Texture Format")
        {
            Patterns = new[] { "*.vtf" },
        }
    };

    /// <summary>
    /// Opens a file dialog bound to the provided target <see cref="Window"/>
    /// </summary>
    /// <param name="target">The owner of this instance</param>
    public FileDialogService(Window target)
    {
        _target = target;
        _defaultStartLocation = WellKnownFolder.Documents;
    }

    private readonly Window _target;
    private Uri? _lastOpenedFileLocation;
    private Uri? _lastSavedFileLocation;
    private readonly WellKnownFolder _defaultStartLocation;

    /// <summary>
    /// The last folder opened by this instance.
    /// </summary>
    public Uri? LastOpenedFileLocation
    {
        get => _lastOpenedFileLocation;
        private set
        {
            string? openedDir = Path.GetDirectoryName(value?.AbsolutePath);
            _lastOpenedFileLocation = openedDir is not null
                ? new Uri(openedDir)
                : null;
        }
    }

    /// <summary>
    /// The last folder opened by this instance.
    /// </summary>
    public Uri? LastSavedFileLocation
    {
        get => _lastSavedFileLocation;
        private set
        {
            string? openedDir = Path.GetDirectoryName(value?.AbsolutePath);
            _lastSavedFileLocation = openedDir is not null
                ? new Uri(openedDir)
                : null;
        }
    }

    /// <summary>
    /// Open up a file dialog that only permits selection of image file types.
    /// </summary>
    /// <returns>Path to image file or null if none selected.</returns>
    public async Task<IStorageFile?> OpenImageFileDialogAsync()
    {
        var startLocation = LastOpenedFileLocation is not null
            ? _target.StorageProvider.TryGetFolderFromPathAsync(LastOpenedFileLocation)
            : _target.StorageProvider.TryGetWellKnownFolderAsync(_defaultStartLocation);

        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Select an Image Source",
            AllowMultiple = false,
            FileTypeFilter = [SupportedFileTypes["Image"]],
            SuggestedStartLocation = await startLocation
        });

        if (files.Count == 1)
        {
            var file = files[0];

            // Remember this file's location so we can open the dialog here next time.
            LastOpenedFileLocation = file.Path;

            return file;
        }

        // No selection made
        return null;
    }

    public async Task<bool> SaveVtfFileDialogAsync(string [,,,] highResFiles, string? lowResFile, Vtf settings)
    {
        var startLocation = LastSavedFileLocation is not null
            ? _target.StorageProvider.TryGetFolderFromPathAsync(LastSavedFileLocation)
            : _target.StorageProvider.TryGetWellKnownFolderAsync(_defaultStartLocation);

        var file = await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            DefaultExtension = "vtf",
            FileTypeChoices = [SupportedFileTypes["Vtf"]],
            Title = "Save VTF file",
            ShowOverwritePrompt = true,
            SuggestedFileName = ".vtf",
            SuggestedStartLocation = await startLocation
        });

        if (file is not null)
        {
            var tmpVtfFileLocation = VtfMaker.Make(highResFiles, lowResFile, settings);
            if (File.Exists(tmpVtfFileLocation))
            {
                File.Move(tmpVtfFileLocation, file.Path.AbsolutePath);
                return true;
            }
        }

        return false;
    }
}