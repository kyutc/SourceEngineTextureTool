using System.IO;
using System.Linq;

namespace SourceEngineTextureTool.Models.BinaryAccess.Vtf.v72;

public class Writer : v71.Writer
{
    public override (uint Major, uint Minor) Version => (7, 2);
    public override uint HeaderSize => 80;
    
    public ushort Depth { get; set; }

    protected override void MakeHeader(ref BinaryWriter bw)
    {
        base.MakeHeader(ref bw);
        bw.Write(Depth);
    }

    protected override void PadHeader(ref BinaryWriter bw)
    {
        // 16-byte alignment header padding
        // 65 + 15 = 80; 80 % 16 = 0
        bw.Write(Enumerable.Repeat((byte)0, 15).ToArray());
    }
}