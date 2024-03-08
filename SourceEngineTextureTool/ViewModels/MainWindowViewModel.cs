using Avalonia.Controls.Selection;
using VTFFlags = SourceEngineTextureTool.Services.BinaryAccess.Vtf.Flags;
using VTFFormat = SourceEngineTextureTool.Services.BinaryAccess.Vtf.Format;
using VTFVersion = SourceEngineTextureTool.Models.Settings.Vtf.Version;
using ScaleAlgorithm = SourceEngineTextureTool.Models.Settings.Sett.ScaleAlgorithm;
using ScaleMode = SourceEngineTextureTool.Models.Settings.Sett.ScaleMode;

namespace SourceEngineTextureTool.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
    #region Export Properties

    /// <summary>
    /// Gets/sets the target VTF version for this texture.
    /// </summary>
    public VTFVersion SelectedVTFVersion { get; set; }

    /// <summary>
    /// Gets/sets the image format to use when converting input files to DDS.
    /// </summary>
    public VTFFormat SelectedVTFImageFormat { get; set; }

    /// <summary>
    /// Gets/sets the scale algorithm to use for resizing files to match their mipmap's <see cref="Resolution"/>.
    /// </summary>
    public ScaleAlgorithm SelectedScaleAlgorithm { get; set; }

    /// <summary>
    /// Gets/sets how an the texture will transform if it does not match the <see cref="TextureResolution"/>
    /// </summary>
    public ScaleMode SelectedScaleMode { get; set; }

    // Todo:
    public uint EnabledVTFFlags { get; }

    public SelectionModel<VTFFlags> SelectedFlags { get; } = new();

    #endregion Export Properties
    /// <summary>
    /// Gets/sets the <see cref="Resolution"/> of this instance's <see cref="Texture"/>.
    /// </summary>
    public Resolution TextureResolution { get; set; }

    /// <summary>
    /// Gets/sets the number of frames in this instance's <see cref="Texture"/>.
    /// </summary>
    public int FrameCount { get; set; }

    /// <summary>
    /// Gets the number of mipmap levels this texture has.
    /// </summary>
    public int MipmapCount { get; set; }

    /// <summary>
    /// Todo: If this is false, the view should only have access to the first mipmap.
    /// Gets/sets whether this texture should have mipmaps past order 0.
    /// </summary>
    public bool GenerateMipmaps { get; set; }
    public MainWindowViewModel()
    {
        #region Default Output Settings

        GenerateMipmaps = true;
        SelectedVTFVersion = VTFVersion.VTF_7_1;
        SelectedVTFImageFormat = VTFFormat.ARGB8888;
        SelectedScaleAlgorithm = ScaleAlgorithm.Kaiser;
        SelectedScaleMode = ScaleMode.Fill;

        #endregion Default Output Settings
    }
}