using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceEngineTextureTool.Models;

/// <summary>
/// Represents a mipmap order in a <see cref="Texture"/>.
/// </summary>
public class Mipmap
{
    private Resolution _resolution;
    private List<Frame> _frames;
    private byte _frameCount;

    public Resolution Resolution
    {
        get => _resolution;
    }
    
    public DropImage.DropImage[] Frames
    {
        get => _frames.Select(frame => frame.DropImage).ToArray();
    }

    public byte FrameCount
    {
        get => _frameCount;
        set
        {
            if (value == 0)
            {
                throw new ArgumentException($"Mipmap must contain at least 1 frame. Value provided: {{{value}}}");
            }
            _frameCount = value;
            _UpdateFrames();
        }
    }

    public Mipmap(Resolution resolution, byte frameCount = 1)
    {
        _resolution = resolution;
        _frames = new ();
        FrameCount = frameCount;
    }
    
    private void _UpdateFrames()
    {
        if (_frames.Count > _frameCount)
        {
            _frames.RemoveRange(_frameCount, _frames.Count - _frameCount);
            return;
        }
        while (_frames.Count < _frameCount)
        {
            _frames.Add(new ());
        }
    }
}