using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;

namespace SourceEngineTextureTool.ViewModels;

public class TextureViewModel : ViewModelBase
{
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

    [Reactive] public ObservableCollection<MipmapViewModel> MipmapViewModels { get; set; }

    public TextureViewModel(Texture? texture = null)
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
                this.RaisePropertyChanged(nameof(ResolutionWidth));
                this.RaisePropertyChanged(nameof(ResolutionHeight));
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