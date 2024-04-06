using System;
using System.ComponentModel.DataAnnotations;

namespace SourceEngineTextureTool.Models.Settings;

// TODO: Complete this for all the needed data stores on the frontend
public class Vtf
{
    // TODO: Merge conflict.
    public (uint Major, uint Minor) Version = (7, 1);
    public VersionEnum VtfVersion = VersionEnum.VTF_7_1;
    
    public ushort Width = 512;
    public ushort Height = 512;

    public Format FormatOption = Format.DXT1;
    
    public Flags FlagsOption = 0; // TODO: Is this wrong?

    public float BumpmapScale = 1.0f;

    public ushort FirstFrame = 0;

    public byte LowResWidth = 0;
    public byte LowResHeight = 0;

    public (float R, float G, float B) Reflectivity = (0.5f, 0.5f, 0.5f);
    
    public enum VersionEnum
    {
        [Display(Name = "VTF 7.1")] VTF_7_1 = 1,
        [Display(Name = "VTF 7.2")] VTF_7_2,
        [Display(Name = "VTF 7.3")] VTF_7_3,
        [Display(Name = "VTF 7.4")] VTF_7_4,
        [Display(Name = "VTF 7.5")] VTF_7_5,
    }
    
    // TODO: Where should these values be defined at?
    // TODO: Map these to "human" text in a reasonable way, or is that not needed?
    [Flags]
    public enum Flags : uint
    {
        [Display(Name = "Point Sample")] POINTSAMPLE = 0x00000001,
        [Display(Name = "Tri-Linear")] TRILINEAR = 0x00000002,
        [Display(Name = "Clamp S")] CLAMPS = 0x00000004,
        [Display(Name = "Clamp T")] CLAMPT = 0x00000008,
        [Display(Name = "Anisotropic")] ANISOTROPIC = 0x00000010,
        [Display(Name = "Hint DXT5")] HINT_DXT5 = 0x00000020,
        [Display(Name = "PWL Corrected")] PWL_CORRECTED = 0x00000040,
        [Display(Name = "Normal")] NORMAL = 0x00000080,
        [Display(Name = "No Mip")] NOMIP = 0x00000100,
        [Display(Name = "No LOD")] NOLOD = 0x00000200,
        [Display(Name = "All Mips")] ALL_MIPS = 0x00000400,
        [Display(Name = "Procedural")] PROCEDURAL = 0x00000800,
        [Display(Name = "One-Bit Alpha")] ONEBITALPHA = 0x00001000,
        [Display(Name = "Eight-Bit Alpha")] EIGHTBITALPHA = 0x00002000,
        [Display(Name = "Environment Map")] ENVMAP = 0x00004000,
        [Display(Name = "Render Target")] RENDERTARGET = 0x00008000,
        [Display(Name = "Depth Render Target")] DEPTHRENDERTARGET = 0x00010000,
        [Display(Name = "No Debug Override")] NODEBUGOVERRIDE = 0x00020000,
        [Display(Name = "Single Copy")] SINGLECOPY = 0x00040000,
        [Display(Name = "Pre SRGB")] PRE_SRGB = 0x00080000,
        [Display(Name = "Unused")] UNUSED_00100000 = 0x00100000,
        [Display(Name = "Unused")] UNUSED_00200000 = 0x00200000,
        [Display(Name = "Unused")] UNUSED_00400000 = 0x00400000,
        [Display(Name = "No Depth Buffer")] NODEPTHBUFFER = 0x00800000,
        [Display(Name = "Unused")] UNUSED_01000000 = 0x01000000,
        [Display(Name = "Clamp U")] CLAMPU = 0x02000000,
        [Display(Name = "Vertex Texture")] VERTEXTEXTURE = 0x04000000,
        [Display(Name = "SS Bump")] SSBUMP = 0x08000000,
        [Display(Name = "Unused")] UNUSED_10000000 = 0x10000000,
        [Display(Name = "Border")] BORDER = 0x20000000,
        [Display(Name = "Unused")] UNUSED_40000000 = 0x40000000,
        [Display(Name = "Unused")] UNUSED_80000000 = 0x80000000,
    };

    public enum Format : uint
    {
        NONE = 0xffffffff,
        RGBA8888 = 0,
        ABGR8888,
        RGB888,
        BGR888,
        RGB565,
        I8,
        IA88,
        P8,
        A8,
        RGB888_BLUESCREEN,
        BGR888_BLUESCREEN,
        ARGB8888,
        BGRA8888,
        DXT1,
        DXT3,
        DXT5,
        BGRX8888,
        BGR565,
        BGRX5551,
        BGRA4444,
        DXT1_ONEBITALPHA, // Note: this is actually broken and doesn't work correctly; use DXT1 instead
        BGRA5551,
        UV88,
        UVWQ8888,
        RGBA16161616F,
        RGBA16161616,
        UVLX8888
    }
    
}