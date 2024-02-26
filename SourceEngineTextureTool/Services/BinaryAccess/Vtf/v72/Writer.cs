using System.IO;

namespace SourceEngineTextureTool.Services.BinaryAccess.Vtf.v72;

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
}