using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;

namespace SourceEngineTextureTool.ViewModels;

public class MipmapViewModel : ViewModelBase
{
    [Reactive] public Mipmap Mipmap { get; set; }

    [Reactive] public Resolution MipmapResolution { get; set; }

    [Reactive] public ObservableCollection<FrameViewModel> FrameViewModels { get; set; }

    public MipmapViewModel(Mipmap mipmap)
    {
        Mipmap = mipmap;
        MipmapResolution = Mipmap.Resolution;
        FrameViewModels = new();

        this.WhenAnyValue(mvm => mvm.Mipmap)
            .Subscribe(newMipmap =>
            {
                MipmapResolution = newMipmap.Resolution;
                Refresh();
            });
    }

    /// <summary>
    /// Modifies the <see cref="FrameViewModels"/> collection and contents therein to reflect the state of the underlying <see cref="Mipmap"/>.
    /// </summary>
    public void Refresh()
    {
        int index = 0;
        // Update view w/ frame changes
        foreach (var frame in Mipmap.Frames)
        {
            if (index < FrameViewModels.Count)
            {
                FrameViewModels[index++].Frame = frame;
            }
            else
            {
                FrameViewModels.Insert(index++, new(frame));
            }
        }

        // Cull excess viewmodels
        while (FrameViewModels.Count > Mipmap.FrameCount)
        {
            FrameViewModels.RemoveAt(index);
        }
    }
}