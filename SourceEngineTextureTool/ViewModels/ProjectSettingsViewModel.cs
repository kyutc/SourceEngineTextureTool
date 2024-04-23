using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using EnumsNET;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models.Settings;
using SourceEngineTextureTool.Services.Propagator;

namespace SourceEngineTextureTool.ViewModels;

public class ProjectSettingsViewModel : ViewModelBase
{
    /// <summary>
    /// Gets/sets the model describing how SETT processes the inputs for a VTF
    /// </summary>
    [Reactive] public Sett SettSettings { get; set; }

    /// <summary>
    /// Gets/sets the model describing the produced VTF file
    /// </summary>
    [Reactive] public Vtf VtfSettings { get; set; }

    #region Texture Settings

    [Reactive] public TextureViewModel TextureViewModel { get; set; }

    [Reactive] public int? TextureHeight { get; set; }
    [Reactive] public int? TextureWidth { get; set; }

    // Todo: replace with a MipamapGenerationStrategy enum
    [Reactive] public PropagationStrategy SelectedMipmapPropagationStrategy { get; set; }

    public static IReadOnlyList<PropagationStrategy> SupportedPropagationStrategies { get; } =
        Enums.GetValues<PropagationStrategy>();

    #endregion Texture Settings

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
    [Reactive] public Sett.VtfImageFormat SelectedVtfImageFormat { get; set; }

    public IReadOnlyList<Sett.VtfImageFormat> SupportedInputImageFormats { get; } =
        Enums.GetValues<Sett.VtfImageFormat>();

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

    #region Observables

    private IObservable<bool> AnySettingsChangedObserverable { get; set; }

    public IObservable<Sett.PreviewMode> PreviewModeObservable { get; set; }
    public IObservable<TextureViewModel> TextureViewModelObserver { get; set; }
    public IObservable<int?> TextureHeightObservable { get; set; }
    public IObservable<int?> TextureWidthObservable { get; set; }
    public IObservable<PropagationStrategy> MipmapPropagationStrategyObservable { get; set; }
    public IObservable<Sett.AutocropMode> AutocropModeObservable { get; set; }
    public IObservable<Sett.ScaleAlgorithm> ScaleAlgorithmObservable { get; set; }
    public IObservable<Sett.ScaleMode> ScaleModeObservable { get; set; }
    public IObservable<Sett.VtfImageFormat> VtfImageFormatObservable { get; set; }

    #endregion Observables

    #region Commands

    [Reactive] public ICommand ApplySettingsCommand { get; set; }
    [Reactive] public ICommand RevertSettingsCommand { get; set; }

    #endregion Commands

    public ProjectSettingsViewModel(Sett? settSettings = null, Vtf? vtfSettings = null)
    {
        SettSettings = settSettings ?? new();
        {
            SelectedAutocropMode = SettSettings.AutocropModeOption;
            SelectedPreviewMode = SettSettings.PreviewModeOption;
            SelectedScaleAlgorithm = SettSettings.ScaleAlgorithmOption;
            SelectedScaleMode = SettSettings.ScaleModeOption;
            SelectedVtfImageFormat = SettSettings.VtfImageFormatOption;
        }

        // Preview mode only determines which image the gui displays 
        PreviewModeObservable = this.WhenAnyValue(psvm => psvm.SelectedPreviewMode);
        PreviewModeObservable.Subscribe(newPreviewMode =>
        {
            SettSettings.PreviewModeOption = newPreviewMode;
            EnableCompiledTexturePreview = newPreviewMode == Sett.PreviewMode.Postprocessed;
        });

        // This property isn't accessed by the view, but it should update its associated fields when its changed
        TextureViewModelObserver = this.WhenAnyValue(psvm => psvm.TextureViewModel)
            .Skip(1);
        TextureViewModelObserver.Subscribe(newTextureViewModel =>
        {
            TextureHeight = newTextureViewModel.TextureResolution.Height;
            TextureWidth = newTextureViewModel.TextureResolution.Width;
            SelectedMipmapPropagationStrategy = newTextureViewModel.MipmapSourcePropagationStrategy;
        });

        TextureHeightObservable = this.WhenAnyValue(psvm => psvm.TextureHeight);
        TextureWidthObservable = this.WhenAnyValue(psvm => psvm.TextureWidth);

        MipmapPropagationStrategyObservable = this.WhenAnyValue(psvm => psvm.SelectedMipmapPropagationStrategy);

        AutocropModeObservable = this.WhenAnyValue(psvm => psvm.SelectedAutocropMode);
        ScaleAlgorithmObservable = this.WhenAnyValue(psvm => psvm.SelectedScaleAlgorithm);
        ScaleModeObservable = this.WhenAnyValue(psvm => psvm.SelectedScaleMode);
        VtfImageFormatObservable = this.WhenAnyValue(psvm => psvm.SelectedVtfImageFormat);

        // If a setting has been changed, the user should be able to apply or revert those changes.
        _SetupApplyRevertCommands();

        VtfSettings = vtfSettings ?? new();

        this.WhenAnyValue(psvm => psvm.VtfSettings)
            .Subscribe(newVtfSettings =>
            {
                SelectedVtfVersion = newVtfSettings.VtfVersion;

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
    }

    private void _SetupApplyRevertCommands()
    {
        // Create an observable that watches
        AnySettingsChangedObserverable = Observable.CombineLatest(
            TextureHeightObservable.Select(newHeight => newHeight != TextureViewModel?.TextureResolution.Height),
            TextureWidthObservable.Select(newWidth => newWidth != TextureViewModel?.TextureResolution.Width),
            MipmapPropagationStrategyObservable.Select(strategy =>
                strategy != TextureViewModel?.MipmapSourcePropagationStrategy),
            AutocropModeObservable.Select(newAutocropMode => newAutocropMode != SettSettings.AutocropModeOption),
            ScaleAlgorithmObservable.Select(newScaleAlgorithm =>
                newScaleAlgorithm != SettSettings.ScaleAlgorithmOption),
            ScaleModeObservable.Select(newScaleMode => newScaleMode != SettSettings.ScaleModeOption),
            VtfImageFormatObservable.Select(newVtfImageFormat =>
                newVtfImageFormat != SettSettings.VtfImageFormatOption),
            (heightChanged,
                widthChanged,
                strategyChanged,
                autocropModeChanged,
                scaleAlgorithmChanged,
                scaleModeChanged,
                vtfImageFormatChanged) =>
            {
                return heightChanged || widthChanged || strategyChanged || autocropModeChanged ||
                       scaleAlgorithmChanged || scaleModeChanged || vtfImageFormatChanged;
            });

        ApplySettingsCommand = ReactiveCommand.Create(_ApplySettings, AnySettingsChangedObserverable);
        RevertSettingsCommand = ReactiveCommand.Create(_RevertSettings, AnySettingsChangedObserverable);
    }

    private void _ApplySettings()
    {
        if (TextureWidth is not null && TextureHeight is not null)
        {
            TextureViewModel.TextureResolution = new(TextureWidth.Value, TextureHeight.Value);
        }
        TextureViewModel.MipmapSourcePropagationStrategy = SelectedMipmapPropagationStrategy;

        SettSettings.AutocropModeOption = SelectedAutocropMode;
        SettSettings.ScaleAlgorithmOption = SelectedScaleAlgorithm;
        SettSettings.ScaleModeOption = SelectedScaleMode;
        SettSettings.VtfImageFormatOption = SelectedVtfImageFormat;

        // Discard the old observable and replace with a fresh one
        _SetupApplyRevertCommands();

        this.RaisePropertyChanged(nameof(SettSettings));
    }

    private void _RevertSettings()
    {
        TextureHeight = TextureViewModel.TextureResolution.Height;
        TextureWidth = TextureViewModel.TextureResolution.Width;
        SelectedMipmapPropagationStrategy = TextureViewModel.MipmapSourcePropagationStrategy;

        SelectedAutocropMode = SettSettings.AutocropModeOption;
        SelectedScaleAlgorithm = SettSettings.ScaleAlgorithmOption;
        SelectedScaleMode = SettSettings.ScaleModeOption;
        SelectedVtfImageFormat = SettSettings.VtfImageFormatOption;
    }
}