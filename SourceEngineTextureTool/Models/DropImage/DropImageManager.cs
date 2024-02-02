using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SourceEngineTextureTool.Models.DropImage;

public class DropImageManager
{
    private int _frames;

    public int Frames
    {
        get => _frames;
        set
        {
            if (value is < 1 or > 65535)
            {
                throw new Exception("Frame count must be in range 1-65535");
            }

            this._frames = value;
            UpdateCounts();
        }
    }
    
    private int _mipmaps;
    
    public int Mipmaps
    {
        get => _mipmaps;
        set
        {
            if (value is < 1 or > 65535)
            {
                throw new Exception("Mipmap count must be in range 1-65535");
            }

            this._mipmaps = value;
            UpdateCounts();
        }
    }
    
    private List<List<DropImage?>> _dropImages;
    private readonly DropImage _defaultImage;
    
    public DropImageManager(DropImage defaultImage)
    {
        this._defaultImage = defaultImage;
        this._dropImages = new List<List<DropImage?>>
        {
            Capacity = 16 // Probably pointless optimisation
        };

        this.Mipmaps = 1;
        this.Frames = 1;

        this.SetImage(1, 1, this._defaultImage);
    }

    public void SetImage(int mipmap, int frame, DropImage image)
    {
        this._dropImages[mipmap][frame] = image;
    }

    private void UpdateCounts()
    {
        // Update to new mipmap count
        CollectionsMarshal.SetCount(this._dropImages, Mipmaps);
        // Update all frame counts
        this._dropImages.ForEach(frames => CollectionsMarshal.SetCount(frames, Frames));

        // Lazily iterate over all elements to update any new ones with the default image
        for (int mipmap = 0; mipmap < Mipmaps; mipmap++)
        {
            for (int frame = 0; frame < Frames; frame++)
            {
                this._dropImages[mipmap][frame] ??= this._defaultImage;
            }
        }
    }

    // Returns true if every image contains content
    public bool ImagesFilled()
    {
        bool result = true;
        
        // TODO: Small optimisation: break out of the loop.
        this._dropImages.ForEach(frames => 
            frames.ForEach(frame => result &= frame != this._defaultImage)
        );
        return result;
    }
}