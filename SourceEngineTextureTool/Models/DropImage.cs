namespace SourceEngineTextureTool.Models;

// This class will be used to maintain the images (filesystem path and so on)
public class DropImage
{

    public byte? MipmapOrder
    {
        get => _mipmapOrder;
        set => _mipmapOrder = value;
    }

    private byte? _mipmapOrder;

    public Resolution? TargetResolution
    {
        get => _targetResolution;
        set => _targetResolution = value;
    }

    private Resolution? _targetResolution;

    public ushort? FrameIndex
    {
        get => _frameIndex;
        set => _frameIndex = value;
    }
    private ushort? _frameIndex;

    public string? ImportedImage
    {
        get => _importedImage;
        set => _importedImage = value;
    }
    private string? _importedImage;

    public string? PreviewImage
    {
        get => _previewImage;
        set => _previewImage = value;
    }

    private string? _previewImage;
}