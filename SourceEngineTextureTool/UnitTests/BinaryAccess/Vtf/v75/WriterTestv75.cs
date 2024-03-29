using System.Text;
using NUnit.Framework;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf.v75;

namespace SourceEngineTextureTool.UnitTests.BinaryAccess.Vtf.v75;

[TestFixture]
public class WriterTestv75
{
    public string TestVtfFile = "test_file75.vtf";
    
    [Test]
    public void Test_v75Contents()
    {
        //Arrange
        var writer = new Writer
        {
            Width = 0x00_80,
            Height = 0x00_80,
            VtfFlags = 0x00_00_20_40,
            Frames = 0x00_01,
            FirstFrame = 0x00_00,
            Reflectivity = (1.0f, 0.5f, 1.0f),
            BumpmapScale = 0x3f_80_00_00,
            HighResFormat = Format.RGBA8888, 
            MipmapCount = 0x08,
            LowResFormat = Format.RGBA8888,
            LowResWidth = 0x10,
            LowResHeight = 0x10,
            LowResData = Encoding.ASCII.GetBytes("AAAA"),
            HighResData = Encoding.ASCII.GetBytes("BBBB"),
            Depth = 0x00_01,
            Resources = new (Writer.ResourceTag tag, Writer.ResourceFlag flag)[]
            {
                (Writer.ResourceTag.LOWRES, Writer.ResourceFlag.NONE),
                (Writer.ResourceTag.HIGHRES, Writer.ResourceFlag.NONE),
                (Writer.ResourceTag.LOD, Writer.ResourceFlag.NO_DATA),
                (Writer.ResourceTag.KVD, Writer.ResourceFlag.NONE),
            },
            CRC = 0,
            KVD = "aaa",
            LOD = (0x18, 0x1e),
            TSO = 0,
            ParticleSheet = null
        };
        
       
        //Act
        writer.WriteOut(TestVtfFile);

        //Assert
        Assert.That(TestVtfFile, Does.Exist);
    }
}
