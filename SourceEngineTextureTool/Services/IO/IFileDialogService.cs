using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SourceEngineTextureTool.Models.Settings;

namespace SourceEngineTextureTool.Services.IO;

/// <summary>
/// Describes operations to be performed by the platform's storage API.
/// </summary>
public interface IFileDialogService
{
    public Task<IStorageFile?> OpenImageFileDialogAsync();

    public Task<bool> SaveVtfFileDialogAsync(string [,,,] highResFiles, string? lowResFile, Vtf settings);
}