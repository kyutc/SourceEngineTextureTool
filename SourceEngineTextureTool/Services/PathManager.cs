using System;
using System.IO;

namespace SourceEngineTextureTool.Services;

public static class PathManager
{
    private static readonly string BaseDir;
    // Assumption: mipmaps <= 99; frames <= 999, faces <= 9, slices <= 9
    // Though this shouldn't break at values above those, the path will just be less pretty
    private const string Slug = "mipmap_{0:D2}/frame_{1:D3}/face_{2:D1}/slice_{3:D1}";

    static PathManager()
    {
        BaseDir = Path.Join(
            Path.GetTempPath(),
            // Use PID to avoid clobbering other instances' data
            "SETT_" + Environment.ProcessId
        );
    }

    /// <summary>
    /// Return the base directory of this instance's temporary path
    /// </summary>
    /// <remarks>Intended ONLY for cleanup; do not use this path to build other paths</remarks>
    /// <returns></returns>
    public static string GetBaseDir()
    {
        return BaseDir;
    }

    /// <summary>
    /// Path of the normalised input image after being imported
    /// </summary>
    /// <param name="mipmap"></param>
    /// <param name="frame"></param>
    /// <param name="face"></param>
    /// <param name="slice"></param>
    /// <returns>Full path to imported normalised PNG file</returns>
    public static string ImportedImage(ushort mipmap, ushort frame, ushort face, ushort slice)
    {
        return Path.Join(BaseDir,
            string.Format(Slug, mipmap, frame, face, slice),
            "normalised.png"
        );
    }
    
    /// <summary>
    /// Path for the preview image to be used by SETT after preprocessing is completed
    /// OR after postprocessing and back-converting to PNG
    /// </summary>
    /// <param name="mipmap"></param>
    /// <param name="frame"></param>
    /// <param name="face"></param>
    /// <param name="slice"></param>
    /// <returns>Full path to preview PNG file</returns>
    public static string PreviewImage(ushort mipmap, ushort frame, ushort face, ushort slice)
    {
        return Path.Join(BaseDir,
            string.Format(Slug, mipmap, frame, face, slice),
            "preview.png"
        );
    }

    /// <summary>
    /// Path for the image which has been preprocessed (autocrop, compositing, scaling, etc.)
    /// </summary>
    /// <param name="mipmap"></param>
    /// <param name="frame"></param>
    /// <param name="face"></param>
    /// <param name="slice"></param>
    /// <returns>Full path to preprocessed PNG file</returns>
    public static string PreprocessedImage(ushort mipmap, ushort frame, ushort face, ushort slice)
    {
        return Path.Join(BaseDir,
            string.Format(Slug, mipmap, frame, face, slice),
            "preprocessed.png"
        );
    }
    
    /// <summary>
    /// Path for converted DDS output file
    /// </summary>
    /// <param name="mipmap"></param>
    /// <param name="frame"></param>
    /// <param name="face"></param>
    /// <param name="slice"></param>
    /// <returns>Full path to converted DDS file</returns>
    public static string ConvertedImage(ushort mipmap, ushort frame, ushort face, ushort slice)
    {
        // TODO: Consider multiple output mode for Crunch, will create complexities for path handling
        return Path.Join(BaseDir,
            string.Format(Slug, mipmap, frame, face, slice),
            "converted.dds"
        );
    }
}