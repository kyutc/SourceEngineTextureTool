using System.IO;
using System.Linq;

namespace SourceEngineTextureTool.Services.BinaryAccess.Vtf;

public abstract class Vtf
{
    public abstract (uint Major, uint Minor) Version { get; }
    public abstract uint HeaderSize { get; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public uint VtfFlags { get; set; }
    public ushort Frames { get; set; }
    public ushort FirstFrame { get; set; }
    public (float Red, float Green, float Blue) Reflectivity { get; set; }
    public float BumpmapScale { get; set; }
    public uint HighResFormat { get; set; }
    public byte MipmapCount { get; set; }
    public uint LowResFormat { get; set; }
    public byte LowResWidth { get; set; }
    public byte LowResHeight { get; set; }
    public byte[] LowResData { get; set; }
    public byte[] HighResData { get; set; }

    protected abstract void MakeHeader(ref BinaryWriter bw);

    protected void PadHeader(ref BinaryWriter bw)
    {
        // Header must be 16-byte aligned
        int remainder = (int)bw.BaseStream.Position % 16;

        if (remainder > 0)
            bw.Write(Enumerable.Repeat((byte)0, 16 - remainder).ToArray());
    }
}