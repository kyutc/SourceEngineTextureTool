using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;
using SourceEngineTextureTool.Services.Propagator;

namespace SourceEngineTextureTool.ViewModels;

public class TextureViewModel : ViewModelBase
{
    private ReactivePropertyPropagatorManager _reactivePropertyPropagatorManager = new();

    [Reactive] public Texture Texture { get; set; }

    public Resolution TextureResolution
    {
        get => Texture.Resolution;
        set
        {
            Texture.Resolution = value;
            Refresh();
        }
    }

    [Reactive] public ushort FrameCount { get; set; }

    [Reactive] public int MipmapCount { get; set; }

    [Reactive] public bool GenerateMipmaps { get; set; }

    [Reactive] public PropagationStrategy MipmapSourcePropagationStrategy { get; set; }

    [Reactive] public ObservableCollection<MipmapViewModel> MipmapViewModels { get; set; }

    [Reactive] public IObservable<bool> TextureIsReady { get; private set; }
    
    public TextureViewModel(Texture? texture = null)
    {
        MipmapViewModels = new();
        
        Texture = texture ?? new Texture();
        TextureResolution = Texture.Resolution;
        FrameCount = Texture.FrameCount;
        MipmapSourcePropagationStrategy = _reactivePropertyPropagatorManager.PropagationStrategy;
        GenerateMipmaps = MipmapSourcePropagationStrategy != PropagationStrategy.DoNotPropagate;

        this.WhenAnyValue(tvm => tvm.Texture)
            .Skip(1)
            .Subscribe(newTexture =>
            {
                TextureResolution = newTexture.Resolution;
                FrameCount = newTexture.FrameCount;
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