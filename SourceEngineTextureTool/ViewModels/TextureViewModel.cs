using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using EnumsNET;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;
using SourceEngineTextureTool.Services.Propagator;

namespace SourceEngineTextureTool.ViewModels;

public class TextureViewModel : ViewModelBase
{
    private ReactivePropertyPropagatorManager _reactivePropertyPropagatorManager = new();

    [Reactive] public Texture Texture { get; set; }

    [Reactive] public Resolution TextureResolution { get; set; }

    // Todo: Width and height should be modified at the same time. Impossible currently.
    public int ResolutionWidth
    {
        get => TextureResolution.Width;
        set => TryUpdateTextureResolution(value, ResolutionHeight);
    }

    // Todo: Width and height should be modified at the same time. Impossible currently.
    public int ResolutionHeight
    {
        get => TextureResolution.Height;
        set => TryUpdateTextureResolution(ResolutionWidth, value);
    }

    /// <summary>
    /// Attempt to assign a new <see cref="TextureResolution"/>
    /// </summary>
    /// <param name="width">A potentially invalid width value</param>
    /// <param name="height">A potentially invalid height value</param>
    /// <remarks>
    /// The <see cref="Avalonia.Controls.NumericUpDown"/> control has a <code>decimal?</code> for it's value property.
    /// A value is propagated every time the content of the control's text box changes (oddly, excluding when that value is 0 e.g.: 01 -> 0, unable to repeat in another project),
    /// including when the content is empty/null. This throws an exception that breaks the application and generates ugly validation errors.
    /// <see cref="https://github.com/AvaloniaUI/Avalonia/issues/10793"/>
    /// Until a solution is implemented, this workaround attempts to set a new resolution. If an exception is thrown,
    /// either null or an invalid value was provided, and nothing happens here.
    /// </remarks>
    public void TryUpdateTextureResolution(int? width, int? height)
    {
        if (width is not null && height is not null)
        {
            try
            {
                Resolution newResolution = new(width.Value, height.Value);
                TextureResolution = newResolution;
            }
            catch (ArgumentException e)
            {
            }
        }
    }

    [Reactive] public ushort FrameCount { get; set; }

    [Reactive] public int MipmapCount { get; set; }

    [Reactive] public bool GenerateMipmaps { get; set; }

    [Reactive] public PropagationStrategy MipmapSourcePropagationStrategy { get; set; }

    public static IReadOnlyList<PropagationStrategy> SupportedPropagationStrategies { get; } =
        Enums.GetValues<PropagationStrategy>();

    [Reactive] public ObservableCollection<MipmapViewModel> MipmapViewModels { get; set; }

    [Reactive] public IObservable<bool> TextureIsReady { get; private set; }
    
    public TextureViewModel(Texture? texture = null)
    {
        Texture = texture ?? new Texture();
        TextureResolution = Texture.Resolution;
        FrameCount = Texture.FrameCount;
        MipmapSourcePropagationStrategy = _reactivePropertyPropagatorManager.PropagationStrategy;
        GenerateMipmaps = MipmapSourcePropagationStrategy != PropagationStrategy.DoNotPropagate;
        MipmapViewModels = new();

        this.WhenAnyValue(tvm => tvm.Texture)
            .Skip(1)
            .Subscribe(newTexture =>
            {
                TextureResolution = newTexture.Resolution;
                FrameCount = newTexture.FrameCount;
            });

        this.WhenAnyValue(tvm => tvm.TextureResolution)
            .Skip(1)
            .Subscribe(textureResolution =>
            {
                Texture.Resolution = textureResolution;
                this.RaisePropertyChanged(nameof(ResolutionWidth));
                this.RaisePropertyChanged(nameof(ResolutionHeight));
                Refresh();
            });

        this.WhenAnyValue(tvm => tvm.FrameCount)
            .Skip(1)
            .Subscribe(frameCount =>
            {
                // If new value is invalid, revert.
                if (frameCount == 0)
                {
                    FrameCount = Texture.FrameCount;
                    return;
                }

                Texture.FrameCount = frameCount;

                foreach (var mipmapViewModel in MipmapViewModels)
                {
                    mipmapViewModel.Refresh();
                }

                _SetupPropagation();
                _WatchForDropImagesToBeReady();
            });

        this.WhenAnyValue(tvm => tvm.GenerateMipmaps)
            .Skip(1)
            .Subscribe(_ => Refresh());

        this.WhenAnyValue(tvm => tvm.MipmapSourcePropagationStrategy)
            .Skip(1)
            .Subscribe(propagationStrategy =>
            {
                _reactivePropertyPropagatorManager.PropagationStrategy = propagationStrategy;
                if (GenerateMipmaps && propagationStrategy == PropagationStrategy.DoNotPropagate)
                {
                    // Bug: The text block that displays the resolution for the 0th mipmap will occlude when GenerateMipmaps is set to false in this manner. It still exists, has the correct properties set, and may display again upon subsequent renders.
                    GenerateMipmaps = false;
                }
                else if (!GenerateMipmaps && propagationStrategy != PropagationStrategy.DoNotPropagate)
                {
                    GenerateMipmaps = true;
                }
            });

        Refresh();
    }

    /// <summary>
    /// Modifies the <see cref="MipmapViewModels"/> collection and contents therein to reflect the state of the underlying <see cref="Texture"/>.
    /// If <see cref="GenerateMipmaps"/> is false, only presents the 0th index mipmap.
    /// </summary>
    public void Refresh()
    {
        // If not generating mipmaps, only exhibit the first mipmap.
        var mipmaps = GenerateMipmaps
            ? Texture.Mipmaps.Select(mipmap => new MipmapViewModel(mipmap))
            : [new MipmapViewModel(Texture.Mipmaps.First())];
        MipmapViewModels.Clear();
        MipmapViewModels.AddRange(mipmaps);

        MipmapCount = MipmapViewModels.Count;

        _SetupPropagation();
        _WatchForDropImagesToBeReady();
    }

    private void _SetupPropagation()
    {
        _reactivePropertyPropagatorManager.Clear();

        MipmapViewModels.SelectMany(mvm =>
                mvm.FrameViewModels.Select((fvm, index) => (index, divm: fvm.DropImageViewModel)))
            .GroupBy(i => i.index, i => i.divm)
            .Select(g => g.ToList())
            .ToList()
            .ForEach(divms => _reactivePropertyPropagatorManager.InitializePropertyPropagationSequence(divms,
                divm => divm.ImportedImage, divm => divm.DefaultImage)
            );
    }

    /// <summary>
    /// Every time DropImages are added/removed from the view this instance needs to rebuild the observer that watches
    /// for all those DropImages to be ready.
    /// </summary>
    private void _WatchForDropImagesToBeReady()
    {
        IList<IObservable<bool>> readinessObservables = MipmapViewModels.SelectMany(mvm =>
            mvm.FrameViewModels.Select(fvm => fvm.DropImageViewModel.HasConvertedImage)).ToList();
        TextureIsReady = readinessObservables.CombineLatest(values => values.All(value => value));
    }
}