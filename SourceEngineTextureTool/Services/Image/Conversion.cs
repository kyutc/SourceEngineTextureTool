using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ImageMagick;

namespace SourceEngineTextureTool.Services.Image;

public abstract class Operation;

public class AutocropOperation : Operation
{
    public bool Normalise = false;
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

public interface IMultipleOutputs
{
    public uint Start { get; set; }
    public uint Wrap { get; set; }
}

public class WriteOutOperation : Operation, IMultipleOutputs
{
    public required string Outfile;
    public uint Start { get; set; } = 0;
    public uint Wrap { get; set; } = uint.MaxValue;
}

/// <summary>
/// Terminal operation. Writes a file, does not consume the file for the next input.
/// </summary>
public class CrunchOperation : Operation, IMultipleOutputs
{
    public required ImageFormat Format;
    /// <summary>
    /// DXT1A alpha transparency threshold.
    /// </summary>
    public byte AlphaThreashold = 128;
    public required string Outfile;

    public uint Start { get; set; } = 0;
    public uint Wrap { get; set; } = uint.MaxValue;

    public enum ImageFormat
    {
        DXT1,
        DXT2,
        DXT3,
        DXT4,
        DXT5,
        // Note: formats without an overlap with VTF are commented out
        //_3DC,
        //DXN,
        DXT5A, // Is this not the same thing as DXT5?
        //DXT5_CCxY,
        //DXT5_xGxR,
        //DXT5_xGBR,
        //DXT5_AGBR,
        DXT1A,
        //ETC1,
        R8G8B8,
        L8,
        A8,
        A8L8,
        A8R8G8B8,
    }
}

public static class Conversion
{
    public static void Run(string infile, Operation[] tasks)
    {
        Run([infile], tasks);
    }
    
    /// <summary>
    /// Reads all frames from all input files. ex. a.gif and b.gif will result in a + b total frames.
    /// </summary>
    /// <param name="infiles"></param>
    /// <param name="tasks"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Run(string[] infiles, Operation[] tasks)
    {
        var imgs = new MagickImageCollection();
        foreach (var file in infiles)
        {
            var frames = new MagickImageCollection(file);
            foreach (var frame in frames)
            {
                imgs.Add(frame);
            }
        }
        
        imgs.Coalesce();

        // Task order matters, and tasks can be executed more than once
        // ex. autocrop twice in a row, or scale to 512 and then 1024 (but don't do that)
        foreach (var task in tasks)
        {
            switch (task)
            {
                case AutocropOperation operation:
                    Autocrop(imgs, operation);
                    break;
                case ScaleOperation operation:
                    Scale(imgs, operation);
                    break;
                case CompositeOperation operation:
                    Composite(imgs, operation);
                    break;
                case NormaliseToPng32:
                    Normalise(imgs);
                    break;
                case CrunchOperation operation:
                    CrunchMe(imgs, operation);
                    break;
                case WriteOutOperation operation:
                    WriteOut(imgs, operation);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        
        imgs.Dispose();
    }

    // This might be superfluous as its own operation
    private static void Normalise(MagickImageCollection imgs)
    {
        foreach (MagickImage img in imgs)
        {
            img.Format = MagickFormat.Png32;
            img.ColorType = ColorType.TrueColorAlpha;
        }
    }

    private static void Autocrop(MagickImageCollection imgs, AutocropOperation operation)
    {
        foreach (MagickImage img in imgs)
        {
            if (operation.Normalise)
                NormalisedAutocrop(img);
            else
                img.Trim();
        }
    }

    /// <summary>
    /// Continue running the autocrop operation until the resolution does not change.
    /// </summary>
    /// <param name="img"></param>
    private static void NormalisedAutocrop(MagickImage img)
    {
        (int, int) oldRes;
        do
        {
            oldRes = (img.Width, img.Height);
            img.Trim();
        } while (oldRes != (img.Width, img.Height));
    }

    private static void Scale(MagickImageCollection imgs, ScaleOperation operation)
    {
        foreach (MagickImage img in imgs)
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
                    IgnoreAspectRatio =
                        operation.Mode == ScaleOperation.ScaleMode.Stretch, // Mutual exclusion with other modes
                });
            }

            // Center the image and ensure the final size matches the user's request. Since this creates new pixels, a
            // background colour must be used.
            img.BackgroundColor = new MagickColor(
                operation.Background.R, operation.Background.G,
                operation.Background.B, operation.Background.A);
            img.Extent(operation.Width, operation.Height, Gravity.Center);
        }
    }

    private static void Composite(MagickImageCollection imgs, CompositeOperation operation)
    {
        var bg = new MagickImage(
            new MagickColor(operation.R, operation.G, operation.B, operation.A),
            imgs[0].Width,
            imgs[0].Height);
        foreach (MagickImage img in imgs)
        {
            img.Composite(bg, CompositeOperator.DstOver);
        }
        bg.Dispose();
    }

    private static void CrunchMe(MagickImageCollection imgs, CrunchOperation operation)
    {
        Directory.CreateDirectory(PathManager.GetTempWorkDirectory());

        uint index = operation.Start;
        foreach (MagickImage img in imgs)
        {
            string infile = PathManager.GetTempWorkPngFile();
            img.Write(infile); // MagickImage cannot talk to the crunch binary, so write out and then read in.
            string outfile = string.Format(operation.Outfile, index++ % operation.Wrap);
            Directory.CreateDirectory(Path.GetDirectoryName(outfile));

            // TODO: Crunch can accept multiple input files at once, but this will require manually renaming the outputs
            // TODO: No deduplication is done here; identical images will be reprocessed which will hurt performance
            string args = $" -file {infile}"
                          + " -fileFormat dds -mipMode None -helperThreads 8 "
                          + (operation.Format == CrunchOperation.ImageFormat.DXT1A
                              ? $" -alphaThreshold {operation.AlphaThreashold} "
                              : "")
                          + $" -{operation.Format.ToString()} " // Texture format
                          + $" -out {outfile} ";

            CrunchExec(args);

#if !DEBUG
        File.Delete(infile);
#endif
        }
    }

    private static void CrunchExec(string args)
    {
        var psi = new ProcessStartInfo(ExternalDependencyManager.crunch, args)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        Process? p = Process.Start(psi);

        if (p == null) throw new Exception("Crunch binary execution error.");
        
        // TODO: Threading solution to not block execution and report progress. Worthwhile?
        p.WaitForExit(); // Block until program completes
    }

    private static void WriteOut(MagickImageCollection imgs, WriteOutOperation operation)
    {
        uint index = operation.Start;
        foreach (MagickImage img in imgs)
        {
            string outfile = string.Format(operation.Outfile, index++ % operation.Wrap);
            Directory.CreateDirectory(Path.GetDirectoryName(outfile));
            img.Write(outfile);
        }
    }
}