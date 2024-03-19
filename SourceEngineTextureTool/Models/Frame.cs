namespace SourceEngineTextureTool.Models;

public class Frame
{
    public ushort Index { get; set; }
    public string? Source { get; set; }

    public Frame(ushort index, string? source = null)
    {
        Index = index;
        Source = source;
    }
}