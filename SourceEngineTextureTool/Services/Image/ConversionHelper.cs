using System;
using System.Collections.Generic;
using SourceEngineTextureTool.Models.Settings;

namespace SourceEngineTextureTool.Services.Image;

public static class ConversionHelper
{
    /// <summary>
    /// Convert all of the frames of the provided file into individual PNG32 images.
    /// </summary>
    /// <param name="file">Input file. Can be any supported image including png, gif, webm, dds.</param>
    /// <returns>File paths to created PNG32 files.</returns>
    public static string[] NormaliseToPng32(string file)
    {
        return NormaliseToPng32([file]);
    }
    
    /// <summary>
    /// Convert all of the frames of the provided files into individual PNG32 images.
    /// </summary>
    /// <param name="files">Input files. Can be any supported image including png, gif, webm, dds.</param>
    /// <returns>File paths to created PNG32 files.</returns>
    public static string[] NormaliseToPng32(string[] files)
    {
        string[] outfiles = null;
        Conversion.Run(files, ref outfiles, [
            new NormaliseToPng32(),
            new WriteOutOperation(),
        ]);
        return outfiles;
    }

    public static string[] Convert(string[] files, int width, int height, Models.Settings.Sett settings)
    {
        string[] outfiles = null;
        var tasks = new List<Operation>();
        
        tasks.Add(new NormaliseToPng32());

        switch (settings.Autocrop)
        {
            case Sett.AutocropMode.Autocrop:
                tasks.Add(new AutocropOperation());
                break;
            case Sett.AutocropMode.NormalisedAutocrop:
                tasks.Add(new AutocropOperation() { Normalise = true });
                break;
            case Sett.AutocropMode.Collate:
            case Sett.AutocropMode.NormalisedCollate:
                throw new NotImplementedException();
            case Sett.AutocropMode.None:
                break;
            default:
                throw new NotImplementedException();
        }
        
        tasks.Add(new ScaleOperation
        {
            Width = width,
            Height = height,
            Mode = settings.ScaleModeOption switch
            {
                Sett.ScaleMode.None => ScaleOperation.ScaleMode.None,
                Sett.ScaleMode.Fit => ScaleOperation.ScaleMode.Fit,
                Sett.ScaleMode.Fill => ScaleOperation.ScaleMode.Fill,
                Sett.ScaleMode.Stretch => ScaleOperation.ScaleMode.Stretch,
                _ => throw new NotImplementedException(),
            },
            Algorithm = settings.ScaleAlgorithmOption switch
            {
                Sett.ScaleAlgorithm.Kaiser => ScaleOperation.ScaleAlgorithm.Kaiser,
                Sett.ScaleAlgorithm.Point => ScaleOperation.ScaleAlgorithm.Point,
                _ => throw new NotImplementedException(),
            },
            Background = settings.BackgroundColour,
        });
        
        if (settings.CompositeEnabled)
            tasks.Add(new CompositeOperation { BackgroundColour = settings.BackgroundColour });
        
        tasks.Add(new CrunchOperation
        {
            Format = settings.VtfImageFormatOption switch
            {
                Sett.VtfImageFormat.DXT1 => CrunchOperation.ImageFormat.DXT1,
                Sett.VtfImageFormat.DXT1A => CrunchOperation.ImageFormat.DXT1A,
                Sett.VtfImageFormat.DXT3 => CrunchOperation.ImageFormat.DXT3,
                Sett.VtfImageFormat.DXT5 => CrunchOperation.ImageFormat.DXT5,
                Sett.VtfImageFormat.BGR888 => CrunchOperation.ImageFormat.R8G8B8,
                Sett.VtfImageFormat.BGR888_BLUESCREEN => CrunchOperation.ImageFormat.R8G8B8,
                Sett.VtfImageFormat.BGRA8888 => CrunchOperation.ImageFormat.A8R8G8B8,
                Sett.VtfImageFormat.BGRA8888_BLUESCREEN => CrunchOperation.ImageFormat.A8R8G8B8,
                Sett.VtfImageFormat.A8 => CrunchOperation.ImageFormat.A8,
                Sett.VtfImageFormat.I8 => CrunchOperation.ImageFormat.L8,
                Sett.VtfImageFormat.IA88 => CrunchOperation.ImageFormat.A8L8,
                _ => throw new NotImplementedException(),
            },
            AlphaThreashold = settings.Dxt1aAlphaThreshold,
        });

        Conversion.Run(files, ref outfiles, tasks.ToArray());
        return outfiles;
    }
}