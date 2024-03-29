using System.Text;
using NUnit.Framework;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf.v71;

namespace SourceEngineTextureTool.UnitTests.BinaryAccess.Vtf.v71;

[TestFixture]
public class WriterTestv71
{
    public string TestVtfFile = "test_file71.vtf";
    
    [Test]
    public void Test_v71Contents()
    {
        //Arrange
        var writer = new Writer
        {
            Width = 0x00_80,
            Height = 0x00_80,
            VtfFlags = 0x00002040,
            Frames = 0x00_01,
            FirstFrame = 0x00_00,
            Reflectivity = (0.1897f, 0.3916f, 0.5522f),
            BumpmapScale = 1.000f,
            HighResFormat = Format.RGBA8888,
            MipmapCount = 0x08,
            LowResFormat = Format.RGBA8888,
            LowResWidth = 0x10,
            LowResHeight = 0x10,
            LowResData = Encoding.ASCII.GetBytes("AAAA"),
            HighResData = Encoding.ASCII.GetBytes("BBBB"),
        };

        //Act
        writer.WriteOut(TestVtfFile);

        //Assert
        Assert.That(TestVtfFile, Does.Exist);
    }
    
}