using SourceEngineTextureTool.Services.BinaryAccess.Vtf;

namespace SourceEngineTextureTool.Models.Settings;

// TODO: Complete this for all the needed data stores on the frontend
public class Vtf
{
    // TODO: Validate supported version entered
    public (uint Major, uint Minor) VtfVersion = (7, 1);
    
    public ushort Width = 512;
    public ushort Height = 512;

    public Format FormatOption = Format.DXT1;
    
    public uint FlagsOption = 0;
}