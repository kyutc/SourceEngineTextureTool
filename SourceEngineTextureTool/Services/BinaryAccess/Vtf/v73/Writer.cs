using System;
using System.IO;
using System.Text;

namespace SourceEngineTextureTool.Services.BinaryAccess.Vtf.v73;

public class Writer : v72.Writer
{
    public override (uint Major, uint Minor) Version => (7, 3);
    public override uint HeaderSize => (uint)(80 + Resources.Length * 8);

    public (ResourceTag tag, ResourceFlag flag)[] Resources { get; set; }
    
    public uint CRC { get; set; }
    
    public String KVD { get; set; }
    
    public (byte ClampU, byte ClampV) LOD { get; set; }

    // TODO: Confirm this data type; a uint bitmask makes the most sense
    public uint TSO { get; set; }
    
    // TODO: Confirm data type
    public String ParticleSheet { get; set; }

    public enum ResourceTag : uint
    {
        LOWRES = 0x00_00_00_01,
        HIGHRES = 0x00_00_00_30,
        PARTICLESHEET = 0x00_00_00_10,
        CRC = 0x00_43_52_43,
        LOD = 0x00_44_4F_4C,
        TSO = 0x00_4F_53_54,
        KVD = 0x00_44_56_4B,
    }

    [Flags]
    public enum ResourceFlag : byte
    {
        NONE = 0x00,
        NO_DATA = 0x02,
    }

    public new void WriteOut(string file)
    {
        var handle = GetFileHandle(file);
        BinaryWriter bw = new BinaryWriter(handle);

        MakeHeader(ref bw);
        PadHeader(ref bw);
        MakeResourceEntries(ref bw);
        
        // Write resource data in user-defined order so offsets are correct.
        foreach (var (tag, _) in Resources)
        {
            switch (tag)
            {
                case ResourceTag.LOWRES:
                    bw.Write(LowResData);
                    break;
                case ResourceTag.HIGHRES:
                    bw.Write(HighResData);
                    break;
                case ResourceTag.KVD:
                    bw.Write((uint)KVD.Length);
                    bw.Write(Encoding.UTF8.GetBytes(KVD));
                    break;
                case ResourceTag.PARTICLESHEET:
                    // TODO: Confirm this is correct
                    bw.Write((uint)ParticleSheet.Length);
                    bw.Write(Encoding.UTF8.GetBytes(ParticleSheet));
                    break;
            }
        }
        
        bw.Flush();
    }

    protected void MakeResourceEntries(ref BinaryWriter bw)
    {
        // Build resources entries
        uint offset = HeaderSize;
        foreach (var (tag, flag) in Resources)
        {
            // Combine 3-byte tag and 1-byte flag into single uint
            uint combined = (uint)tag | (uint)flag << 24;
            bw.Write(combined); // Tag and flag
            
            // Some tags use offset for data
            // Add to the working offset for resources with data bodies
            switch (tag)
            {
                // Note: Flag.NO_DATA isn't being checked
                case ResourceTag.LOWRES:
                    bw.Write(offset);
                    offset += (uint)LowResData.Length;
                    break;
                case ResourceTag.HIGHRES:
                    bw.Write(offset);
                    offset += (uint)HighResData.Length;
                    break;
                case ResourceTag.KVD:
                    bw.Write(offset);
                    offset += 4 + (uint)KVD.Length;
                    break;
                case ResourceTag.PARTICLESHEET:
                    bw.Write(offset);
                    offset += 4 + (uint)ParticleSheet.Length;
                    break;
                case ResourceTag.CRC:
                    bw.Write(CRC);
                    break;
                case ResourceTag.LOD:
                    // LOD is 2 bytes in UV order, stored in offset as uint
                    bw.Write(LOD.ClampU);
                    bw.Write(LOD.ClampV);
                    bw.Write((ushort)0); // Padding
                    break;
                case ResourceTag.TSO:
                    bw.Write(TSO);
                    break;
                default:
                    throw new Exception("Unsupported resource tag");
                    break;
            }
        }
    }

    protected override void MakeHeader(ref BinaryWriter bw)
    {
        base.MakeHeader(ref bw);
        bw.Write((byte)0); // Padding (depth)
        bw.Write((byte)0); // ...
        bw.Write((byte)0); // ...
        bw.Write((uint)Resources.Length); // Number of resources
    }
}