using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;

namespace SourceEngineTextureTool.ViewModels;

public class FrameViewModel : ViewModelBase
{
    [Reactive] public Frame Frame { get; set; }

    public ushort Index
    {
        get => Frame.Index;
        set
        {
            this.RaisePropertyChanging();
            Frame.Index = value;
            this.RaisePropertyChanged();
        }
    }

    public byte MipmapOrder
    {
        get => Frame.MipmapOrder;
        set
        {
            this.RaisePropertyChanging();
            Frame.MipmapOrder = value;
            this.RaisePropertyChanged();
        }
    }

    [Reactive] public DropImageViewModel DropImageViewModel { get; set; }

    public FrameViewModel(Frame frame)
    {
        Frame = frame;
        DropImageViewModel = new DropImageViewModel(frame.DropImage);

        this.WhenAnyValue(fvm => fvm.Frame)
            .Subscribe(newFrame =>
            {
                this.RaisePropertyChanged(nameof(Index));
                this.RaisePropertyChanged(nameof(MipmapOrder));
            });

        this.WhenAnyValue(fvm => fvm.Index)
            .Subscribe(newIndex => { DropImageViewModel.FrameIndex = Convert.ToUInt16(newIndex); });

        this.WhenAnyValue(fvm => fvm.MipmapOrder)
            .Subscribe(newMipmapOrder => { DropImageViewModel.MipmapOrder = Convert.ToByte(newMipmapOrder); });
    }
}