namespace SourceEngineTextureTool.Models;

public class Frame
{
    public ushort Index
    {
        get => _index;
        set => _index = value;
    }

    private ushort _index;

    public byte MipmapOrder
    {
        get => _mipmapOrder;
        set => _mipmapOrder = value;
    }

    private byte _mipmapOrder;

    public DropImage DropImage
    {
        get => _dropImage;
        set => _dropImage = value;
    }

    private DropImage _dropImage;

    public Frame(ushort index, byte mipmapOrder, Resolution mipmapResolution)
    {
        _index = index;
        _mipmapOrder = mipmapOrder;
        DropImage = new() { FrameIndex = Index, MipmapOrder = MipmapOrder, TargetResolution = mipmapResolution};
    }
}