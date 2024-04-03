using SourceEngineTextureTool.Services.BinaryAccess.Vtf;

namespace SourceEngineTextureTool.Models.Settings;

// TODO: Complete this for all the needed data stores on the frontend
public class Vtf
{
    // TODO: Validate supported version entered
    public (uint Major, uint Minor) Version = (7, 1);
    
    public ushort Width = 512;
    public ushort Height = 512;

    public Format FormatOption = Format.DXT1;
    
    public uint Flags = 0;

    public float BumpmapScale = 1.0f;

    public ushort FirstFrame = 0;

    public byte LowResWidth = 0;
    public byte LowResHeight = 0;

    public (float R, float G, float B) Reflectivity = (0.5f, 0.5f, 0.5f);
}