using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace SourceEngineTextureTool.Services.IO;

/// <summary>
/// Opens a file dialog.
/// </summary>
public class FileDialogService : IFileDialogService
{
    /// <summary>
    /// Opens a file dialog bound to the provided target <see cref="Window"/>
    /// </summary>
    /// <param name="target">The owner of this instance</param>
    public FileDialogService(Window target)
    {
        _target = target;
        _defaultStartLocation = WellKnownFolder.Documents;
        _fileFilterDict = new Dictionary<string, IReadOnlyList<FilePickerFileType>>()
        {
            ["Image"] = new[]
            {
                // Todo: Add more or define custom patterns. Doesn't match .tiff, webp, or other formats we may want to support
                FilePickerFileTypes.ImageAll,
            },
        };
    }

    private readonly Window _target;
    private Uri? _lastAccessedLocation;
    private readonly WellKnownFolder _defaultStartLocation;
    private readonly Dictionary<string, IReadOnlyList<FilePickerFileType>> _fileFilterDict;

    /// <summary>
    /// The last folder opened by this instance.
    /// </summary>
    public Uri? LastAccessedLocation
    {
        get => _lastAccessedLocation;
        private set
        {
            string? openedDir = Path.GetDirectoryName(value?.AbsolutePath);
            _lastAccessedLocation = openedDir is not null
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
        var startLocation = LastAccessedLocation is not null
            ? _target.StorageProvider.TryGetFolderFromPathAsync(LastAccessedLocation)
            : _target.StorageProvider.TryGetWellKnownFolderAsync(_defaultStartLocation);

        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Select an Image Source",
            AllowMultiple = false,
            FileTypeFilter = _fileFilterDict["Image"],
            SuggestedStartLocation = await startLocation
        });

        if (files.Count == 1)
        {
            var file = files[0];

            // Remember this file's location so we can open the dialog here next time.
            LastAccessedLocation = file.Path;

            return file;
        }

        // No selection made
        return null;
    }
}