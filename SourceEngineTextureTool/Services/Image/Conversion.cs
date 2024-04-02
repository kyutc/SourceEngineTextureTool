using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
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
    public required (byte R, byte G, byte B, byte A) BackgroundColour;
}

public class WriteOutOperation : Operation;

/// <summary>
/// Terminal operation. Writes a file, does not consume the file for the next input.
/// </summary>
public class CrunchOperation : Operation
{
    public required ImageFormat Format;
    /// <summary>
    /// DXT1A alpha transparency threshold.
    /// </summary>
    public byte AlphaThreashold = 128;

    public enum ImageFormat
    {
        DXT1,
        //DXT2,
        DXT3,
        //DXT4,
        DXT5,
        // Note: formats without an overlap with VTF are commented out
        //_3DC,
        //DXN,
        //DXT5A, // Is this not the same thing as DXT5?
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
    private static readonly string BaseDir;
    
    static Conversion()
    {
        var tmpdir = Directory.CreateTempSubdirectory("SETT_");
        BaseDir = tmpdir.FullName;
    }

    public static void Run(string infile, ref string[]? outfiles, Operation[] tasks)
    {
        Run([infile], ref outfiles, tasks);
    }

    /// <summary>
    /// Reads all frames from all input files. ex. a.gif and b.gif will result in a + b total frames.
    /// </summary>
    /// <param name="infiles"></param>
    /// <param name="outfiles"></param>
    /// <param name="tasks"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Run(string[] infiles, ref string[]? outfiles, Operation[] tasks)
    {
        var imgs = new MagickImageCollection();
        foreach (var file in infiles)
        {
            var frames = new MagickImageCollection(file);
            frames.Coalesce(); // Redundant?
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
                {
                    var files = CrunchMe(imgs, operation);
                    if (outfiles is null)
                        outfiles = files;
                    else
                    {
                        int oldlen = outfiles.Length;
                        Array.Resize(ref outfiles, outfiles.Length + files.Length);
                        files.CopyTo(outfiles, oldlen);
                    }
                }
                    break;
                case WriteOutOperation:
                {
                    var files = WriteOut(imgs);
                    if (outfiles is null)
                        outfiles = files;
                    else
                    {
                        int oldlen = outfiles.Length;
                        Array.Resize(ref outfiles, outfiles.Length + files.Length);
                        files.CopyTo(outfiles, oldlen);
                    }
                }
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

            // Center the image and ensure the final size matches the user's request.
            // FIXME: Extent was affecting transparent pixels rather than only making new pixels this background colour.
            // We'll assume a composite operation will be used as a less-optimal fix. It'll have the same problem, but
            // the operation is more explicit about what it's doing.
            img.Extent(operation.Width, operation.Height, Gravity.Center,
                new MagickColor(0, 0, 0, 0));
        }
    }

    private static void Composite(MagickImageCollection imgs, CompositeOperation operation)
    {
        var clr = new MagickColor(operation.BackgroundColour.R, operation.BackgroundColour.G,
            operation.BackgroundColour.B, operation.BackgroundColour.A);
        var bg = new MagickImage(clr, imgs[0].Width, imgs[0].Height);
        foreach (MagickImage img in imgs)
        {
            img.Composite(bg, CompositeOperator.DstOver);
        }
        bg.Dispose();
    }

    private static string[] CrunchMe(MagickImageCollection imgs, CrunchOperation operation)
    {
        string[] infiles = WriteOut(imgs);
        string[] outfiles = new string[infiles.Length];

        string args = "";
        foreach (var file in infiles)
        {
            // BUG: Theoretically with enough input files/a large temporary directory path, the length limit of the
            // command line arguments could be reached. Crunch has an @file option for this purpose.
            args += $" -file {file} ";
        }
        
        // TODO: No deduplication is done here; identical images (with different file paths) will be reprocessed which
        // will hurt performance; current code pattern does not allow for deduplication here since each input must
        // match 1:1 with an output. It will be the responsibility of the caller to do deduplication if it is desired.
        args += " -fileFormat dds -mipMode None -helperThreads 8 "
                + (operation.Format == CrunchOperation.ImageFormat.DXT1A
                    ? $" -alphaThreshold {operation.AlphaThreashold} "
                    : "")
                + $" -{operation.Format.ToString()} " // Texture format
                + " -outsamedir ";

        CrunchExec(args);
        
        for(int i = 0; i < infiles.Length; i++)
        {
            outfiles[i] = Path.Join(Path.GetDirectoryName(infiles[i]),
                Path.GetFileNameWithoutExtension(infiles[i]) + ".dds");
            File.Delete(infiles[i]);
        }

        return outfiles;
    }

    private static void CrunchExec(string args)
    {
        var psi = new ProcessStartInfo(ExternalDependencyManager.crunch, args)
        {
            UseShellExecute = true,
            // BUG: Redirecting streams causes poor performance / hanging. Unknown reason.
            //RedirectStandardInput = true,
            //RedirectStandardOutput = true,
            //RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        Process? p = Process.Start(psi);

        if (p == null) throw new Exception("Crunch failed to start.");
        
        // TODO: Threading solution to not block execution and report progress. Worthwhile?
        p.WaitForExit(); // Block until program completes

        if (p.ExitCode != 0) throw new Exception("Crunch returned an error.");
    }

    private static string[] WriteOut(MagickImageCollection imgs)
    {
        var outfiles = new string[imgs.Count];
        for (int i = 0; i < imgs.Count; i++)
        {
            outfiles[i] = Path.Join(BaseDir, RandomNumberGenerator.GetHexString(8) + ".png");
            ((MagickImage)imgs[i]).Write(outfiles[i]);
        }

        return outfiles;
    }
}