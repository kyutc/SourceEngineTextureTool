using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;

namespace SourceEngineTextureTool.Models;

/// <summary>
/// Represents a texture.
/// Todo: Is framerate property needed?
/// </summary>
public class Texture
{
    /// <summary>
    /// Get/set the texture's max resolution
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public Resolution Resolution
    {
        get => _resolution;
        set => _ResizeTexture(value);
    }

    private Resolution _resolution;

    /// <summary>
    /// Gets the collection of mipmaps for this <see cref="Texture"/>.
    /// </summary>
    public IEnumerable<Mipmap> Mipmaps
    {
        get => _mipmaps;
    }

    private readonly List<Mipmap> _mipmaps = new();

    /// <summary>
    /// Get/set the number of frames this texture has
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public ushort FrameCount
    {
        get => _frameCount;
        set
        {
            if (value == _frameCount) return;

            if (value == 0)
            {
                throw new ArgumentException("A texture must have at least one frame.");
            }

            _frameCount = value;
            _ChangeFrameCounts();
        }
    }

    private ushort _frameCount;

    public Texture(Resolution resolution, ushort frameCount = 1) : this(resolution.Width, resolution.Height, frameCount)
    {
    }

    public Texture(int width = 512, int height = 512, ushort frameCount = 1)
    {
        _frameCount = frameCount;
        Resolution = new Resolution(width, height);
    }

    #region Private Helpers

    private void _ResizeTexture(Resolution newResolution)
    {
        // If the new resolution is the same 
        if (newResolution == _resolution) return;

        Resolution oldResolution = _resolution;
        _resolution = newResolution;

        // Check if the new resolution is a smaller scalar of the old one.
        // If so, truncate the list.
        {
            int index = _mipmaps.FindIndex(mipmap => mipmap.Resolution == newResolution);

            if (index != -1)
            {
                // New resolution is a smaller scalar of the previous resolution, truncate.
                _mipmaps.RemoveRange(0, index - 1);
                return;
            }
        }

        // The new resolution does not exist in the existing mipmaps.
        // Generate a comprehensive list of required resolutions.
        var requiredResolutions = new List<Resolution>() { Resolution };
        {
            var resolution = Resolution;

            while ((resolution.Width | resolution.Height) != 1)
            {
                resolution /= 2;
                requiredResolutions.Add(resolution);
            }
        }

        // Check if new resolution is a larger scalar of the old one.
        // If so, we can preserve the existing mipmaps.
        {
            int index = requiredResolutions.FindIndex(requiredResolution =>
                requiredResolution == oldResolution);

            if (index != -1)
            {
                // New resolution is a larger scalar of the previous resolution
                Mipmap[] oldMipmaps = Mipmaps.ToArray();
                _mipmaps.Clear();
                for (int i = 0; i < index; i++)
                {
                    _mipmaps.Add(new Mipmap(requiredResolutions[i], FrameCount));
                }

                _mipmaps.AddRange(oldMipmaps);
                return;
            }
        }

        // If this point reached, the new resolution is incompatible with the old one.
        // Replace list contents with new mipmaps.
        _mipmaps.Clear();
        foreach (var requiredResolution in requiredResolutions)
        {
            _mipmaps.Add(new Mipmap(requiredResolution, FrameCount));
        }
    }

    private void _ChangeFrameCounts()
    {
        foreach (var mipmap in _mipmaps)
        {
            mipmap.FrameCount = FrameCount;
        }
    }

    #endregion Private Helpers
}