using System;
using System.Collections.Generic;
using ImageMagick;

namespace SourceEngineTextureTool.Services.Image;

public abstract class Operation;

public class AutocropOperation : Operation
{
    public required bool Normalise = false;
}
public class NormaliseToPng32 : Operation;

public class ScaleOperation : Operation
{
    public required int Width;
    public required int Height;
    public required ScaleMode Mode;
    public required ScaleAlgorithm Algorithm;
    public required (byte R, byte G, byte B, byte A) Background;
    
    public enum ScaleMode
    {
        Fit,
        Fill,
        Stretch,
        None,
    }

    public enum ScaleAlgorithm
    {
        Point,
        Kaiser,
    }
}

public class CompositeOperation : Operation
{
    public required byte R;
    public required byte G;
    public required byte B;
    public required byte A;
}

public class WriteOutOperation : Operation
{
    public required string Outfile;
}

public class CrunchOperation : Operation
{
    // TODO: This will include the options necessary to encode the image into a DDS with the provided options using Crunch.
}

public static class Conversion
{
    public static void Run(string infile, ref List<Operation> tasks)
    {
        // TODO: Recursive implementation for inputs with more than 1 frame
        // var img = new MagickImageCollection(infile);
        
        var img = new MagickImage(infile);

        // Task order matters, and tasks can be executed more than once
        // ex. autocrop twice in a row, or scale to 512 and then 1024 (but don't do that)
        foreach (var task in tasks)
        {
            switch (task)
            {
                case AutocropOperation operation:
                    Autocrop(ref img, ref operation);
                    break;
                case ScaleOperation operation:
                    Scale(ref img, ref operation);
                    break;
                case CompositeOperation operation:
                    Composite(ref img, ref operation);
                    break;
                case NormaliseToPng32:
                    Normalise(ref img);
                    break;
                case CrunchOperation operation:
                    CrunchMe(ref img, ref operation);
                    break;
                case WriteOutOperation operation:
                    WriteOut(ref img, ref operation);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    // This might be superfluous as its own operation
    private static void Normalise(ref MagickImage img)
    {
        img.Format = MagickFormat.Png32;
        img.ColorType = ColorType.TrueColorAlpha;
    }

    private static void Autocrop(ref MagickImage img, ref AutocropOperation operation)
    {
        if (operation.Normalise)
            NormalisedAutocrop(ref img);
        else 
            img.Trim();
    }

    /// <summary>
    /// Continue running the autocrop operation until the resolution does not change.
    /// </summary>
    /// <param name="img"></param>
    private static void NormalisedAutocrop(ref MagickImage img)
    {
        (int, int) oldRes;
        do
        {
            oldRes = (img.Width, img.Height);
            img.Trim();
        } while (oldRes != (img.Width, img.Height));
    }

    private static void Scale(ref MagickImage img, ref ScaleOperation operation)
    {
        // Translate our enum to Imagick's enum
        img.FilterType = operation.Algorithm switch
        {
            ScaleOperation.ScaleAlgorithm.Kaiser => FilterType.Kaiser,
            ScaleOperation.ScaleAlgorithm.Point => FilterType.Point,
            _ => throw new NotImplementedException(),
        };

        if (operation.Mode != ScaleOperation.ScaleMode.None)
        {
            img.Resize(new MagickGeometry()
            {
                Width = operation.Width,
                Height = operation.Height,
                FillArea = operation.Mode == ScaleOperation.ScaleMode.Fill, // "Fit" is false here
                IgnoreAspectRatio = operation.Mode == ScaleOperation.ScaleMode.Stretch, // Mutual exclusion with other modes
            });
        }

        // Center the image and ensure the final size matches the user's request. Since this creates new pixels, a
        // background colour must be used.
        img.BackgroundColor = new MagickColor(
            operation.Background.R, operation.Background.G,
            operation.Background.B, operation.Background.A);
        img.Extent(operation.Width, operation.Height, Gravity.Center);
    }

    private static void Composite(ref MagickImage img, ref CompositeOperation operation)
    {
        var bg = new MagickImage(
            new MagickColor(operation.R, operation.G, operation.B, operation.A),
            img.Width,
            img.Height);

        img.Composite(bg, CompositeOperator.DstOver);
    }

    private static void CrunchMe(ref MagickImage img, ref CrunchOperation operation)
    {
        // TODO
        throw new NotImplementedException();
    }

    private static void WriteOut(ref MagickImage img, ref WriteOutOperation operation)
    {
        // TODO: Handle multiple outputs (animated input)
        img.Write(operation.Outfile);
    }
}