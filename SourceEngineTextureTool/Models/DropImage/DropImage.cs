using System;

namespace SourceEngineTextureTool.Models.DropImage;

// This class will be used to maintain the images (filesystem path and so on)
public class DropImage
{
    private string? _importedImage;
    public string ImportedImage
    {
        get => _importedImage;
        set
        {
            _importedImage = value;
            _previewImage = null;
        }
    }

    private string? _previewImage;
    public string PreviewImage
    {
        get => _previewImage;
    }
}