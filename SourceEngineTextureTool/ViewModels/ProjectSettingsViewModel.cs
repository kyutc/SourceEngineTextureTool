using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using EnumsNET;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models.Settings;

namespace SourceEngineTextureTool.ViewModels;

public class ProjectSettingsViewModel : ViewModelBase
{
    /// <summary>
    /// Gets/sets the model describing how SETT processes the inputs for a VTF
    /// </summary>
    public Sett SettSettings { get; set; }

    /// <summary>
    /// Gets/sets the model describing the produced VTF file
    /// </summary>
    public Vtf VtfSettings { get; set; }

    public IObservable<Unit> RenderSettingChanged { get; set; }

    #region SETT Settings

    /// <summary>
    /// Gets/sets the technique used to crop animated textures.
    /// </summary>
    [Reactive] public Sett.AutocropMode SelectedAutocropMode { get; set; }

    public static IReadOnlyList<Sett.AutocropMode> SupportedAutocropModes { get; } =
        Enums.GetValues<Sett.AutocropMode>();

    /// <summary>
    /// Gets/sets the method used for presenting a texture for preview.
    /// </summary>
    [Reactive] public Sett.PreviewMode SelectedPreviewMode { get; set; }

    public static IReadOnlyList<Sett.PreviewMode> SupportedPreviewModes { get; } = Enums.GetValues<Sett.PreviewMode>();

    /// <summary>
    /// Gets/sets the scale algorithm to use for resizing files to match their mipmap's <see cref="Resolution"/>.
    /// </summary>
    [Reactive] public Sett.ScaleAlgorithm SelectedScaleAlgorithm { get; set; }

    public static IReadOnlyList<Sett.ScaleAlgorithm> SupportedScaleAlgorithms { get; } =
        Enums.GetValues<Sett.ScaleAlgorithm>();

    /// <summary>
    /// Gets/sets how an the texture will transform if it does not match the <see cref="TextureResolution"/>
    /// </summary>
    [Reactive] public Sett.ScaleMode SelectedScaleMode { get; set; }

    public static IReadOnlyList<Sett.ScaleMode> SupportedScaleModes { get; } = Enums.GetValues<Sett.ScaleMode>();

    #endregion SETT Settings

    #region VTF Settings

    /// <summary>
    /// Gets/sets the target VTF version for this texture.
    /// </summary>
    [Reactive] public Vtf.VersionEnum SelectedVtfVersion { get; set; }

    public IReadOnlyList<Vtf.VersionEnum> SupportedVTFVersions { get; } = Enums.GetValues<Vtf.VersionEnum>();

    /// <summary>
    /// Gets/sets the image format to use when converting input files to DDS.
    /// </summary>
    [Reactive] public Vtf.Format SelectedVtfImageFormat { get; set; }

    public IReadOnlyList<Vtf.Format> SupportedVTFImageFormats { get; } = Enums.GetValues<Vtf.Format>();

    /// <summary>
    /// Gets the observable list of currently selected VTF flags.
    /// </summary>
    public ObservableCollection<Vtf.Flags> SelectedVtfFlagItems { get; } = new();

    private IDisposable? SelectedVtfFlagItemsSubscription { get; set; }

    /// <summary>
    /// Gets/sets the contents of <see cref="SelectedVtfFlagItems"/>
    /// </summary>
    public Vtf.Flags SelectedVtfFlags
    {
        get => VtfSettings.FlagsOption;
        set
        {
            SelectedVtfFlagItems.Clear();
            SelectedVtfFlagItems.AddRange(OptionalVTFFlags.Where(flag => (flag & value) == flag));
        }
    }

    public IReadOnlyList<Vtf.Flags> OptionalVTFFlags { get; } = Enums.GetValues<Vtf.Flags>();

    #endregion VTF Settings
    
    #region GUI Settings

    /// <summary>
    /// Gets/sets whether the imported image or the formatted image should be displayed in the workspace.
    /// </summary>
    [Reactive] public bool EnableCompiledTexturePreview { get; set; }
    
    // Todo: Compact/Native display setting

    #endregion GUI Settings
    
    public ProjectSettingsViewModel(Sett? settSettings = null, Vtf? vtfSettings = null)
    {
        SettSettings = settSettings ?? new();
        {
            // SelectedAutocropMode = SettSettings.AutocropModeOption;
            SelectedPreviewMode = SettSettings.PreviewModeOption;
            SelectedScaleAlgorithm = SettSettings.ScaleAlgorithmOption;
            SelectedScaleMode = SettSettings.ScaleModeOption;
        }
        
        var autocropModeObservable = this.WhenAnyValue(psvm => psvm.SelectedAutocropMode);
        autocropModeObservable.Subscribe(newAutocropMode =>
        {
            SettSettings.AutocropModeOption = newAutocropMode;
            this.RaisePropertyChanged(nameof(SettSettings));
        });
        
        var previewModeObservable = this.WhenAnyValue(psvm => psvm.SelectedPreviewMode);
        previewModeObservable.Subscribe(newPreviewMode =>
        {
            SettSettings.PreviewModeOption = newPreviewMode;
            this.RaisePropertyChanged(nameof(SettSettings));
        });
        
        var scaleAlgorithmObservable = this.WhenAnyValue(psvm => psvm.SelectedScaleAlgorithm);
        scaleAlgorithmObservable.Subscribe(newScaleAlgorithm =>
        {
            SettSettings.ScaleAlgorithmOption = newScaleAlgorithm;
            this.RaisePropertyChanged(nameof(SettSettings));
        });
        
        var scaleModeObservable = this.WhenAnyValue(psvm => psvm.SelectedScaleMode);
        scaleModeObservable.Subscribe(newScaleMode =>
        {
            SettSettings.ScaleModeOption = newScaleMode;
            this.RaisePropertyChanged(nameof(SettSettings));
        });
        
        RenderSettingChanged = Observable.Merge(
            autocropModeObservable.Select(_ => Unit.Default),
            previewModeObservable.Select(_ => Unit.Default),
            scaleAlgorithmObservable.Select(_ => Unit.Default),
            scaleModeObservable.Select(_ => Unit.Default));
        
        VtfSettings = vtfSettings ?? new();
        
        this.WhenAnyValue(psvm => psvm.VtfSettings)
            .Subscribe(newVtfSettings =>
            {
                SelectedVtfVersion = newVtfSettings.VtfVersion;
                SelectedVtfImageFormat = newVtfSettings.FormatOption;
                SelectedVtfFlags = newVtfSettings.FlagsOption;

                SelectedVtfFlagItemsSubscription?.Dispose();
                SelectedVtfFlagItemsSubscription = SelectedVtfFlagItems
                    .ToObservableChangeSet()
                    .AsObservableList()
                    .Connect()
                    .Subscribe(changeset =>
                    {
                        foreach (var change in changeset)
                        {
                            switch (change.Reason)
                            {
                                case ListChangeReason.Add:
                                    VtfSettings.FlagsOption |= change.Item.Current;
                                    break;
                                case ListChangeReason.Remove:
                                    VtfSettings.FlagsOption &= ~change.Item.Current;
                                    break;
                                case ListChangeReason.Clear:
                                    VtfSettings.FlagsOption = 0;
                                    break;
                            }
                        }
                    });
            });

        this.WhenAnyValue(psvm => psvm.SelectedVtfVersion)
            .Subscribe(newVtfVersion => VtfSettings.VtfVersion = newVtfVersion);

        this.WhenAnyValue(psvm => psvm.SelectedVtfImageFormat)
            .Subscribe(newVtfImageFormat => VtfSettings.FormatOption = newVtfImageFormat);
    }
}