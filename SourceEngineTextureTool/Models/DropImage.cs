namespace SourceEngineTextureTool.Models;

// This class will be used to maintain the images (filesystem path and so on)
public class DropImage
{
    private Resolution? _imageResolution;
    private int? _frameIndex;
    private string? _importedImage;
    private string? _previewImage;

    public Resolution? ImageResolution
    {
        get => _imageResolution;
        set => _imageResolution = value;
    }
    
    public int? FrameIndex
    {
        get => _frameIndex;
        set => _frameIndex = value;
    }

    public string? ImportedImage
    {
        get => _importedImage;
        set => _importedImage = value;
    }

    public string? PreviewImage
    {
        get => _previewImage;
        set => _previewImage = value;
    }
}