using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;
using System.Collections.Generic;
using Avalonia.Controls.Selection;
using EnumsNET;
using VTFFlags = SourceEngineTextureTool.Services.BinaryAccess.Vtf.Flags;
using VTFFormat = SourceEngineTextureTool.Services.BinaryAccess.Vtf.Format;
using VTFVersion = SourceEngineTextureTool.Models.Settings.Vtf.Version;
using ScaleAlgorithm = SourceEngineTextureTool.Models.Settings.Sett.ScaleAlgorithm;
using ScaleMode = SourceEngineTextureTool.Models.Settings.Sett.ScaleMode;

namespace SourceEngineTextureTool.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Export Properties

    /// <summary>
    /// Gets/sets the target VTF version for this texture.
    /// </summary>
    [Reactive]
    public VTFVersion SelectedVTFVersion { get; set; }

    public IReadOnlyList<VTFVersion> SupportedVTFVersions { get; } = Enums.GetValues<VTFVersion>();

    /// <summary>
    /// Gets/sets the image format to use when converting input files to DDS.
    /// </summary>
    [Reactive]
    public VTFFormat SelectedVTFImageFormat { get; set; }

    public IReadOnlyList<VTFFormat> SupportedVTFImageFormats { get; } = Enums.GetValues<VTFFormat>();

    /// <summary>
    /// Gets/sets the scale algorithm to use for resizing files to match their mipmap's <see cref="Resolution"/>.
    /// </summary>
    [Reactive]
    public ScaleAlgorithm SelectedScaleAlgorithm { get; set; }

    public IReadOnlyList<ScaleAlgorithm> SupportedScaleAlgorithms { get; } = Enums.GetValues<ScaleAlgorithm>();

    /// <summary>
    /// Gets/sets how an the texture will transform if it does not match the <see cref="TextureResolution"/>
    /// </summary>
    [Reactive]
    public ScaleMode SelectedScaleMode { get; set; }

    public IReadOnlyList<ScaleMode> SupportedScaleModes { get; } = Enums.GetValues<ScaleMode>();

    // Todo:
    public uint EnabledVTFFlags { get; }

    public SelectionModel<VTFFlags> SelectedFlags { get; } = new();

    // Todo: Are there permutations of flags that should not be permitted?
    public IReadOnlyList<VTFFlags> OptionalVTFFlags { get; } = Enums.GetValues<VTFFlags>();

    #endregion Export Properties

    public Texture Texture;

    /// <summary>
    /// Gets/sets the <see cref="Resolution"/> of this instance's <see cref="Texture"/>.
    /// </summary>
    [Reactive]
    public Resolution TextureResolution { get; set; }

    /// <summary>
    /// Gets/sets the number of frames in this instance's <see cref="Texture"/>.
    /// </summary>
    [Reactive]
    public int FrameCount { get; set; }

    /// <summary>
    /// Gets the number of mipmap levels this texture has.
    /// </summary>
    [Reactive]
    public int MipmapCount { get; set; }

    /// <summary>
    /// Todo: If this is false, the view should only have access to the first mipmap.
    /// Gets/sets whether this texture should have mipmaps past order 0.
    /// </summary>
    [Reactive]
    public bool GenerateMipmaps { get; set; }

    [Reactive] public ObservableCollection<Mipmap> Mipmaps { get; set; }

    public MainWindowViewModel()
    {
        // Default values
        Resolution defaultResolution = new Resolution(1024, 1024);
        byte defaultFrameCount = 1;

        Texture = new Texture(defaultResolution, defaultFrameCount);
        TextureResolution = Texture.Resolution;
        FrameCount = Texture.FrameCount;
        Mipmaps = new();
        GenerateMipmaps = true;

        #region Default Output Settings

        GenerateMipmaps = true;
        SelectedVTFVersion = VTFVersion.VTF_7_1;
        SelectedVTFImageFormat = VTFFormat.ARGB8888;
        SelectedScaleAlgorithm = ScaleAlgorithm.Kaiser;
        SelectedScaleMode = ScaleMode.Fill;

        #endregion Default Output Settings
    }
}