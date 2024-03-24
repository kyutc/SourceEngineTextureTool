using Avalonia.Controls;
using System;
using System.IO;

public static class ImageUtility
{
    public static (int width, int height) GetImageDimensions(string imagePath)
    {
        int width = 0;
        int height = 0;

        try
        {
            using (var stream = File.OpenRead(imagePath))
            {
                var imageControl = new Image();
                imageControl.Source = new Avalonia.Media.Imaging.Bitmap(stream);

                width = (int)imageControl.Width;
                height = (int)imageControl.Height;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return (width, height);
    }
}
