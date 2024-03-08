using System.ComponentModel.DataAnnotations;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf;

namespace SourceEngineTextureTool.Models.Settings;

// TODO: Complete this for all the needed data stores on the frontend
public class Vtf
{
    // TODO: Validate supported version entered
    public Version VtfVersion = Version.VTF_7_1;
    
    public ushort Width = 512;
    public ushort Height = 512;

    public Format FormatOption = Format.DXT1;
    
    public uint FlagsOption = 0;
    
    
    public enum Version
    {
        [Display(Name = "VTF 7.1")] VTF_7_1 = 1,
        [Display(Name = "VTF 7.2")] VTF_7_2,
        [Display(Name = "VTF 7.3")] VTF_7_3,
        [Display(Name = "VTF 7.4")] VTF_7_4,
        [Display(Name = "VTF 7.5")] VTF_7_5,
    }
}