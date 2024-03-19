using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;
using SourceEngineTextureTool.Models.DropImage;

namespace SourceEngineTextureTool.ViewModels;

public class TextureViewModel : ViewModelBase
{
    [Reactive] public Texture Texture { get; set; }

    [Reactive] public Resolution TextureResolution { get; set; }

    [Reactive] public ushort FrameCount { get; set; }

    [Reactive] public int MipmapCount { get; set; }

    [Reactive] public bool GenerateMipmaps { get; set; }

    [Reactive] public ObservableCollection<MipmapViewModel> MipmapViewModels { get; set; }

    public TextureViewModel(Texture? texture)
    {
        Texture = texture ?? new Texture();
        GenerateMipmaps = true;
        MipmapViewModels = new();

        this.WhenAnyValue(tvm => tvm.Texture)
            .Subscribe(newTexture =>
            {
                TextureResolution = newTexture.Resolution;
                FrameCount = newTexture.FrameCount;
            });

        this.WhenAnyValue(tvm => tvm.TextureResolution)
            .Subscribe(textureResolution =>
            {
                Texture.Resolution = textureResolution;
                Refresh();
            });

        this.WhenAnyValue(tvm => tvm.FrameCount)
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
            });

        this.WhenAnyValue(tvm => tvm.GenerateMipmaps)
            .Subscribe(_ => Refresh());
    }

    /// <summary>
    /// Modifies the <see cref="MipmapViewModels"/> collection and contents therein to reflect the state of the underlying <see cref="Texture"/>.
    /// If <see cref="GenerateMipmaps"/> is false, only presents the 0th index mipmap.
    /// </summary>
    public void Refresh()
    {
        if (!GenerateMipmaps)
        {
            MipmapViewModels.Clear();
            MipmapViewModels.Insert(0, new MipmapViewModel(Texture.Mipmaps.First()));
        }
        else
        {
            int index = 0;
            // Update viewmodels w/ mipmap changes
            foreach (var mipmap in Texture.Mipmaps)
            {
                if (index < MipmapViewModels.Count)
                {
                    var mipmapViewModel = MipmapViewModels[index++];
                    mipmapViewModel.Mipmap = mipmap;
                }
                else
                {
                    MipmapViewModels.Insert(index++, new(mipmap));
                }
            }

            // Cull excess viewmodels
            while (MipmapViewModels.Count > Texture.Mipmaps.Count())
            {
                MipmapViewModels.RemoveAt(index);
            }
        }

        MipmapCount = MipmapViewModels.Count;
    }
}