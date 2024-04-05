using System;
using System.Collections.Generic;

namespace SourceEngineTextureTool.Models;

/// <summary>
/// Represents a mipmap order in a <see cref="Texture"/>.
/// </summary>
public class Mipmap
{
    private Resolution _resolution;

    public Resolution Resolution
    {
        get => _resolution;
    }

    public IEnumerable<Frame> Frames
    {
        get => _frames;
    }

    private readonly List<Frame> _frames;

    public ushort FrameCount
    {
        get => (ushort)_frames.Count;
        set
        {
            if (value == 0)
            {
                throw new ArgumentException($"Mipmap must contain at least 1 frame. Value provided: {{{value}}}");
            }

            _UpdateFrames(value);
        }
    }

    public Mipmap(Resolution resolution, ushort frameCount = 1)
    {
        _resolution = resolution;
        _frames = new();
        FrameCount = frameCount;
    }

    private void _UpdateFrames(ushort newFrameCount)
    {
        if (_frames.Count > newFrameCount)
        {
            _frames.RemoveRange(newFrameCount, _frames.Count - newFrameCount);
            return;
        }

        while (_frames.Count < newFrameCount)
        {
            _frames.Add(new((ushort)_frames.Count));
        }
    }
}