using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SourceEngineTextureTool.Models;

namespace SourceEngineTextureTool.ViewModels;

public class FrameViewModel : ViewModelBase
{
    [Reactive] public Frame Frame { get; set; }

    [Reactive] public int Index { get; set; }

    [Reactive] public string? Source { get; set; }

    public FrameViewModel(Frame frame)
    {
        Frame = frame;

        this.WhenAnyValue(fvm => fvm.Frame)
            .Subscribe(newFrame =>
            {
                Index = newFrame.Index;
                Source = newFrame.Source;
            });

        this.WhenAnyValue(fvm => fvm.Source)
            .Subscribe(newSource => { Frame.Source = newSource; });
    }
}