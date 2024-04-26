namespace SourceEngineTextureTool.Models.Settings;

/// <summary>
/// Settings specific to SETT.
/// </summary>
public class Sett
{
    public enum AutocropMode
    {
        None,
        Autocrop,
        NormalisedAutocrop,
        Collate, // Ensure animation frames are "overlayed" before autocrop boundaries and determined
        NormalisedCollate,
    }
    public AutocropMode AutocropModeOption = AutocropMode.None;
    
    public enum PreviewMode
    {
        Placeholder,
        //Preprocessed, // Probably not worth doing
        Postprocessed,
    }
    public PreviewMode PreviewModeOption = PreviewMode.Postprocessed;

    public enum ScaleMode
    {
        None,
        Fit,
        Fill,
        Stretch,
    }
    public ScaleMode ScaleModeOption = ScaleMode.Fit;

    // TODO: Is it worth supporting several other options? Kaiser is quite good in most cases, and Point covers pixel art.
    public enum ScaleAlgorithm
    {
        Point,
        Kaiser,
    }
    public ScaleAlgorithm ScaleAlgorithmOption = ScaleAlgorithm.Kaiser;


    /// <summary>
    /// A list of all *supported* VTF image formats.
    /// </summary>
    public enum VtfImageFormat
    {
        DXT1,
        DXT1A,
        DXT3,
        DXT5,
        BGR888,
        BGR888_BLUESCREEN,
        BGRA8888,
        BGRA8888_BLUESCREEN,
        I8,
        A8,
        IA88,
    }

    public VtfImageFormat VtfImageFormatOption = VtfImageFormat.DXT1;

    public byte Dxt1aAlphaThreshold = 128;

    public bool CompositeEnabled = true;
    public (byte R, byte G, byte B, byte A) BackgroundColour = (0, 0, 0, 0);
}