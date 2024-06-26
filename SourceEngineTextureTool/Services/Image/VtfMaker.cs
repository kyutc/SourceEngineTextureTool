using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Writer70 = SourceEngineTextureTool.Services.BinaryAccess.Vtf.v70.Writer;
using Writer71 = SourceEngineTextureTool.Services.BinaryAccess.Vtf.v71.Writer;
using Writer72 = SourceEngineTextureTool.Services.BinaryAccess.Vtf.v72.Writer;
using Writer73 = SourceEngineTextureTool.Services.BinaryAccess.Vtf.v73.Writer;
using Writer74 = SourceEngineTextureTool.Services.BinaryAccess.Vtf.v74.Writer;
using Writer75 = SourceEngineTextureTool.Services.BinaryAccess.Vtf.v75.Writer;

namespace SourceEngineTextureTool.Services.Image;

public static class VtfMaker
{
    public static readonly string BaseDir;
    
    static VtfMaker()
    {
        var tmpdir = Directory.CreateTempSubdirectory("SETT_");
        BaseDir = tmpdir.FullName;
    }
    
    /// <summary>
    /// Construct a VTF file from the input DDS files and provided options. Makes inferences about the VTF's image
    /// format, frame count, mipmap count, slice count, and cubemap requirements based on inputs.
    /// </summary>
    /// <param name="highresFiles">4D array of DDS files in order: mipmap, frame, face, slice</param>
    /// <param name="lowresFile">Single DDS image for thumbnail.</param>
    /// <param name="vtfSettings">VTF Settings Model</param>
    /// <returns>Path to the created VTF file.</returns>
    public static string Make(string[,,,] highresFiles, string? lowresFile, Models.Settings.Vtf settings)
    {
        // TODO: Performance can be improved by precalculating the size of this array instead of using a list.
        var highResData = new List<byte>();
        var lowResData = lowresFile is null ? Array.Empty<byte>() : BinaryAccess.Dds.Reader.FromFile(lowresFile);

        // Assumption: All dimensions are the same. This is a good assumption because the VTF structure requires it.
        // Can also use the dimensions here to make inferences about the VTF, ex. frame count, mipmap count, cube map
        int mipmaps = highresFiles.GetLength(0);
        int frames = highresFiles.GetLength(1);
        int faces = highresFiles.GetLength(2);
        int slices = highresFiles.GetLength(3);

        // TODO: Does this need to be so convoluted?
        string versionString = typeof(Models.Settings.Vtf.VersionEnum).GetField(settings.VtfVersion.ToString()).GetCustomAttribute<DisplayAttribute>(false).Name;

        if (settings.VtfVersion is > Models.Settings.Vtf.VersionEnum.VTF_7_5 or < Models.Settings.Vtf.VersionEnum.VTF_7_0)
            throw new Exception($"Invalid version: {versionString}");

        // Face count must be 1, 6, or 7.
        switch (faces)
        {
            case 1: // Not a cubemap
            case 6: // Standard 6-face cubemap
                break;
            case 7: // Weird 7-face cubemap (VTF version 7.4 or lower. Not supported in 7.5+?)
                if (settings.FirstFrame != ushort.MaxValue)
                    throw new Exception($"Got 7 faces, but first frame was {settings.FirstFrame}. Should be -1.");
                if (settings.VtfVersion >= Models.Settings.Vtf.VersionEnum.VTF_7_5)
                    throw new Exception("A 7-faced cubemap is not supported on VTF version 7.5+");
                break;
            default:
                throw new Exception($"Invalid face count: {faces}");
        }
        
        if (settings.FirstFrame >= frames && faces != 7) // Only checking face count here since we've validated it above
        {
            throw new Exception($"First frame is {settings.FirstFrame}, but there are {frames} frames.");
        }
        
        // TODO: Validate VTF flags make sense
        // ex. mutual exclusion between 1-bit and 8-bit alpha, having alpha enabled when required, ensuring cubemap is
        // set when it has 6 or 7 faces, and so on.
        
        for (int mipmap = 0; mipmap < mipmaps; mipmap++)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                for (int face = 0; face < faces; face++)
                {
                    for (int slice = 0; slice < slices; slice++)
                    {
                        var imgData = BinaryAccess.Dds.Reader.FromFile(highresFiles[mipmap, frame, face, slice]);
                        highResData.AddRange(imgData);
                    }
                }
            }
        }

        var writer = settings.VtfVersion switch
        {
            Models.Settings.Vtf.VersionEnum.VTF_7_0 => new Writer70(),
            Models.Settings.Vtf.VersionEnum.VTF_7_1 => new Writer71(),
            Models.Settings.Vtf.VersionEnum.VTF_7_2 => new Writer72(),
            Models.Settings.Vtf.VersionEnum.VTF_7_3 => new Writer73(),
            Models.Settings.Vtf.VersionEnum.VTF_7_4 => new Writer74(),
            Models.Settings.Vtf.VersionEnum.VTF_7_5 => new Writer75(),
            _ => throw new Exception($"Unsupported version: {versionString}"),
        };

        writer.Width = settings.Width;
        writer.Height = settings.Height;
        writer.VtfFlags = (uint)settings.FlagsOption;
        writer.Frames = (ushort)frames;
        writer.FirstFrame = settings.FirstFrame;
        writer.Reflectivity = settings.Reflectivity;
        writer.BumpmapScale = settings.BumpmapScale;
        writer.HighResFormat = (uint)settings.FormatOption;
        writer.MipmapCount = (byte)mipmaps;
        writer.LowResFormat = (uint)(lowResData.Length == 0 ? Models.Settings.Vtf.Format.NONE : Models.Settings.Vtf.Format.DXT1); // Low res is always DXT1 if present
        writer.LowResWidth = settings.LowResWidth;
        writer.LowResHeight = settings.LowResHeight;

        // Consider: Interfaces might make this more palatable
        if (writer.Version is (Major: 7, Minor: >= 2))
        {
            // Depth added in version 7.2
            ((Writer72)writer).Depth = (ushort)slices;
        }

        if (writer.Version is (Major: 7, Minor: >= 3))
        {
            // Resources added in version 7.3+
            var resources = new List<(Writer73.ResourceTag tag, Writer73.ResourceFlag flag)>();
            
            resources.Add((Writer73.ResourceTag.HIGHRES, Writer73.ResourceFlag.NONE));
            
            if (lowResData.Length != 0)
                resources.Add((Writer73.ResourceTag.LOWRES, Writer73.ResourceFlag.NONE));
            
            // TODO: Support other resource types. Or don't.
            
            ((Writer73)writer).Resources = resources.ToArray();
        }

        writer.LowResData = lowResData;
        writer.HighResData = highResData.ToArray();

        string outfile = Path.Join(BaseDir, RandomNumberGenerator.GetHexString(8) + ".vtf");
        
        // TODO: There must surely be a better way...
        switch (settings.VtfVersion)
        {
            case Models.Settings.Vtf.VersionEnum.VTF_7_0:
                ((Writer70)writer).WriteOut(outfile);
                break;
            case Models.Settings.Vtf.VersionEnum.VTF_7_1:
                ((Writer71)writer).WriteOut(outfile);
                break;
            case Models.Settings.Vtf.VersionEnum.VTF_7_2:
                ((Writer72)writer).WriteOut(outfile);
                break;
            case Models.Settings.Vtf.VersionEnum.VTF_7_3:
                ((Writer73)writer).WriteOut(outfile);
                break;
            case Models.Settings.Vtf.VersionEnum.VTF_7_4:
                ((Writer74)writer).WriteOut(outfile);
                break;
            case Models.Settings.Vtf.VersionEnum.VTF_7_5:
                ((Writer75)writer).WriteOut(outfile);
                break;
            default:
                throw new Exception($"Unsupported version: {versionString}");
        };
        
        return outfile;
    }
}