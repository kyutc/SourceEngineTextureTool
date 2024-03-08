using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace SourceEngineTextureTool.Services.IO;

/// <summary>
/// Describes operations to be performed by the platform's storage API.
/// </summary>
public interface IFileDialogService
{
    public Task<IStorageFile?> OpenImageFileDialogAsync();
}