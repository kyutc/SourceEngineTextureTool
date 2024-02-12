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
        Collate, // Ensure animation frames are "overlayed" before autocrop boundaries and determined
    }
    public AutocropMode Autocrop = AutocropMode.Collate;
    
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

    // TODO: Is it worth supporting several other options? Kaiser is quite good in most cases, and Box covers pixel art.
    public enum ScaleAlgorithm
    {
        Box,
        Kaiser,
    }
    public ScaleAlgorithm ScaleAlgorithmOption = ScaleAlgorithm.Kaiser;
    
    // TODO: Validate colour or change storage object
    // RGBA in Hexadecimal notation
    public string CompositeColour = "00000000";
}