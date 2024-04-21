using System;
using System.IO;

namespace SourceEngineTextureTool.Services.BinaryAccess.Vtf.v70;

public class Writer : Vtf
{
    public override (uint Major, uint Minor) Version => (7, 0);
    public override uint HeaderSize => 64;

    protected FileStream GetFileHandle(string file)
    {
        bool valid = true;
        valid &= !System.IO.File.Exists(file);
        var handle = new FileStream(file, FileMode.CreateNew);
        valid &= handle.CanWrite;

        if (!valid) throw new Exception("Failed to write VTF file");

        return handle;
    }

    public void WriteOut(string file)
    {
        var handle = GetFileHandle(file);
        BinaryWriter bw = new BinaryWriter(handle);

        MakeHeader(ref bw);
        PadHeader(ref bw);
        
        bw.Write(LowResData);
        bw.Write(HighResData);
        
        bw.Flush();
        bw.Dispose();
    }

    protected override void MakeHeader(ref BinaryWriter bw)
    {
        bw.Write("VTF\0"u8.ToArray());
        bw.Write(Version.Major);
        bw.Write(Version.Minor);
        bw.Write(HeaderSize);
        bw.Write(Width);
        bw.Write(Height);
        bw.Write(VtfFlags);
        bw.Write(Frames);
        bw.Write(FirstFrame);
        bw.Write((uint)0); // Padding
        bw.Write(Reflectivity.Red);
        bw.Write(Reflectivity.Blue);
        bw.Write(Reflectivity.Green);
        bw.Write((uint)0); // Padding
        bw.Write(BumpmapScale);
        bw.Write((uint)HighResFormat);
        bw.Write(MipmapCount);
        bw.Write((uint)LowResFormat);
        bw.Write(LowResWidth);
        bw.Write(LowResHeight);
    }
}