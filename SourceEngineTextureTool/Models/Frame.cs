namespace SourceEngineTextureTool.Models;

public class Frame
{
    public ushort Index { get; set; }
    public DropImage.DropImage DropImage { get; } = new DropImage.DropImage();

    public string? Source
    {
        get => DropImage.ImportedImage;
        set => DropImage.ImportedImage = value!;
    }
}