using System;
using System.IO;
using System.Text;

namespace SourceEngineTextureTool.Models.BinaryAccess;

// Struct via http://doc.51windows.net/directx9_sdk/graphics/reference/DDSFileReference/ddsfileformat.htm
/// <summary>
/// Read single mipmap from a DDS file.
/// </summary>
public static class DdsReader
{
    /// <summary>
    /// Get image data from a DDS file.
    /// </summary>
    /// <param name="file">Path to DDS file</param>
    /// <returns>Raw image data</returns>
    /// <exception cref="Exception"></exception>
    public static byte[] FromFile(string file)
    {
        bool valid = true;
        // In our case all valid files will end with .dds
        // This should never have valid = false unless our code does something wrong.
        valid &= System.IO.Path.HasExtension(".dds");
        valid &= System.IO.File.Exists(file);
        var handle = new FileStream(file, FileMode.Open);
        valid &= handle.CanRead;
        
        if (!valid) throw new Exception("Invalid DDS file provided.");

        var br = new BinaryReader(handle);

        // Get and check that the file contains the "magic" bytes of a DDS
        string magic = Encoding.ASCII.GetString(br.ReadBytes(4));
        valid &= magic == "DDS ";
        
        if (!valid) throw new Exception("Invalid DDS file provided.");
        
        // Could just advance the pointer to 128 and check nothing
        br.ReadUInt32(); // Header size
        br.ReadUInt32(); // Flags
        uint height = br.ReadUInt32(); // Height
        uint width = br.ReadUInt32(); // Width
        br.ReadUInt32(); // Pitch
        uint depth = br.ReadUInt32(); // Depth
        uint mipmaps = br.ReadUInt32(); // Mipmaps
        br.ReadBytes(4*11); // Reserved

        // DDS_PIXELFORMAT
        br.ReadUInt32(); // Size
        br.ReadUInt32(); // Flags
        string format = Encoding.ASCII.GetString(br.ReadBytes(4)); // Format
        br.ReadUInt32(); // RGB Bit Count
        br.ReadUInt32(); // R Bit Mask
        br.ReadUInt32(); // G Bit Mask
        br.ReadUInt32(); // B Bit Mask
        br.ReadUInt32(); // A Bit Mask

        br.ReadUInt32(); // Caps 1
        br.ReadUInt32(); // Caps 2
        br.ReadUInt32(); // Caps 3
        br.ReadUInt32(); // Caps 4
        br.ReadUInt32(); // Reserved

        // VTF supports a max width/height of 2^16-1 pixels (in theory)
        valid &= width is > 0 and < 65536;
        valid &= height is > 0 and < 65536;
        valid &= mipmaps == 1; // For our case, mipmaps should always be 1
        valid &= depth == 0; // For our case, depth should always be 0
        
        if (!valid) throw new Exception("Invalid DDS file provided.");
        
        // TODO: Is it worthwhile to check that the pitch, format, and total size are also the expected values?
        
        // Assuming the DDS file contains strictly only 1 mipmap and no depth, the remainder of the file
        // is the image data of one single mipmap.
        return br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
    }
}