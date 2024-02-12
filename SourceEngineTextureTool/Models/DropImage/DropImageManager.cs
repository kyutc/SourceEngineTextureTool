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
    
    // TODO: Validation for faces/slices if we ever implement these
    private int _faces;
    
    public int Faces
    {
        get => _faces;
        set
        {
            this._faces = value;
            UpdateCounts();
        }
    }
    
    private int _slices;
    
    public int Slices
    {
        get => _slices;
        set
        {
            this._slices = value;
            UpdateCounts();
        }
    }
    
    //In order: Mipmap, Frame, Face, Slice
    private List<List<List<List<DropImage?>>>> _dropImages;
    private readonly DropImage _defaultImage;
    
    public DropImageManager(DropImage defaultImage)
    {
        this._defaultImage = defaultImage;
        this._dropImages = new List<List<List<List<DropImage?>>>>
        {
            Capacity = 16 // Probably pointless optimisation
        };

        this.Mipmaps = 1;
        this.Frames = 1;
        this.Faces = 1;
        this.Slices = 1;

        this.SetImage(1, 1, 1, 1, this._defaultImage);
    }

    public void SetImage(int mipmap, int frame, int face, int slice, DropImage image)
    {
        this._dropImages[mipmap][frame][face][slice] = image;
    }

    private void UpdateCounts()
    {
        // Update all mipmap counts
        CollectionsMarshal.SetCount(this._dropImages, Mipmaps);
        // Update all frame counts
        this._dropImages.ForEach(frames => CollectionsMarshal.SetCount(frames, Frames));
        // Update all face counts
        this._dropImages.ForEach(frames =>
            frames.ForEach(faces =>  CollectionsMarshal.SetCount(faces, Faces)));
        // Update all slice counts
        this._dropImages.ForEach(frames =>
            frames.ForEach(faces =>
                faces.ForEach(slices => CollectionsMarshal.SetCount(slices, Slices))));


        // Lazily iterate over all elements to update any new ones with the default image
        for (int mipmap = 0; mipmap < Mipmaps; mipmap++)
        {
            for (int frame = 0; frame < Frames; frame++)
            {
                for (int face = 0; face < Faces; face++)
                {
                    for (int slice = 0; slice < Slices; slice++)
                    {
                        this._dropImages[mipmap][frame][face][slice] ??= this._defaultImage;
                    }
                }
            }
        }
    }

    // Returns true if every image contains content
    public bool ImagesFilled()
    {
        bool result = true;
        
        for (int mipmap = 0; result && mipmap < Mipmaps; mipmap++)
        {
            for (int frame = 0; result && frame < Frames; frame++)
            {
                for (int face = 0; result && face < Faces; face++)
                {
                    for (int slice = 0; result && slice < Slices; slice++)
                    {
                        result &= this._dropImages[mipmap][frame][face][slice] != this._defaultImage;
                    }
                }
            }
        }
        
        return result;
    }
}