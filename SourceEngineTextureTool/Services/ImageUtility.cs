using System;
using System.IO;
using SourceEngineTextureTool.Models;

public static class ImageUtility
{
    public static Resolution GetImageDimensions(string imagePath)
    {
        using BinaryReader br = new BinaryReader(File.OpenRead(imagePath));
        {
            br.BaseStream.Position = 16;
            byte[] widthBytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++) widthBytes[sizeof(int) - 1 - i] = br.ReadByte();
            int width = BitConverter.ToInt32(widthBytes, 0);
            byte[] heightBytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++) heightBytes[sizeof(int) - 1 - i] = br.ReadByte();
            int height = BitConverter.ToInt32(heightBytes, 0);
            
            return new Resolution(width, height);
        }
    }
}
