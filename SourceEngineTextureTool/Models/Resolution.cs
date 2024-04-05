using System;

namespace SourceEngineTextureTool.Models;

/// <summary>
/// Represents a constant resolution.
/// </summary>
public class Resolution
{
    public int Width { get; }
    public int Height { get; }

    public Resolution(int width, int height)
    {
        if (width < 1 || height < 1)
        {
            throw new ArgumentException(
                $"Neither axis of a Resolution can be less than 1! Arguments provided: {{width: {width}}} {{height: {height}}}.");
        }

        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }

    public static bool operator ==(Resolution? l, Resolution? r)
    {
        if (l is { } a && r is { } b)
        {
            return a.Width == b.Width && a.Height == b.Height;
        }

        return (l is null && r is null);
    }

    public static bool operator !=(Resolution a, Resolution b) => !(a == b);

    public static Resolution operator *(Resolution a, int factor)
    {
        return new Resolution(Convert.ToUInt16(a.Width * factor), Convert.ToUInt16(a.Height * factor));
    }

    public static Resolution operator /(Resolution a, int divisor)
    {
        return new Resolution(
            Convert.ToUInt16(Math.Max(1, a.Width / divisor)),
            Convert.ToUInt16(Math.Max(1, a.Height / divisor))
        );
    }

    /// <summary>
    /// Elegant pairing hash algorithm. Only supports non-negative integers.
    /// http://szudzik.com/ElegantPairing.pdf
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        if (Width > Height)
        {
            return Width * Width + Width + Height;
        }
        else
        {
            return Height * Height + Width;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is Resolution resolution)
        {
            return this == resolution;
        }

        return false;
    }
}