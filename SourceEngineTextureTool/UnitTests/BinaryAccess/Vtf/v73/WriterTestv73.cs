using System.Reactive;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf.v73;

namespace SourceEngineTextureTool.UnitTests.BinaryAccess.Vtf.v73;

[TestFixture]
public class WriterTestv73
{
    // public string TestVtfFile = "test_file73.vtf";

    Writer writer = new()
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
    
    
    [Test]
    public void Test_v73Version()
    {
        //Assert
        ClassicAssert.AreEqual((7,3), writer.Version);
    }
    [Test]
    public void Test_v73Width()
    {
        //Assert
        ClassicAssert.AreEqual(128, writer.Width);
    }
    
    [Test]
    public void Test_v73Height()
    {
        //Assert
        ClassicAssert.AreEqual(128, writer.Height);
    }
    
    [Test]
    public void Test_v73VtfFlags()
    {
        //Assert
        ClassicAssert.AreEqual(8256, writer.VtfFlags);
    }
    
    [Test]
    public void Test_v73Frames()
    {
        //Assert
        ClassicAssert.AreEqual(1, writer.Frames);
    }
    
    [Test]
    public void Test_v73FirstFrames()
    {
        //Assert
        ClassicAssert.AreEqual(0, writer.FirstFrame);
    }
    
    [Test]
    public void Test_v73Reflectivity()
    {
        //Assert
        ClassicAssert.AreEqual((1.0,0.5,1.0), writer.Reflectivity);
    }
    
    [Test]
    public void Test_v73BumpMapScale()
    {
        //Assert
        ClassicAssert.AreEqual(1065353216, writer.BumpmapScale);
    }
    
    [Test]
    public void Test_v73MipMapCount()
    {
        //Assert
        ClassicAssert.AreEqual(8, writer.MipmapCount);
    }
    
    [Test]
    public void Test_v73LowResWidth()
    {
        //Assert
        ClassicAssert.AreEqual(16, writer.LowResWidth);
    }
    
    [Test]
    public void Test_v73LowResHeight()
    {
        //Assert
        ClassicAssert.AreEqual(16, writer.LowResHeight);
    }
    
    [Test]
    public void Test_v73Depth()
    {
        //Assert
        ClassicAssert.AreEqual(1, writer.Depth);
    }
    
    [Test]
    public void Test_v73LOD()
    {
        //Assert
        ClassicAssert.AreEqual((24,30), writer.LOD);
    }
    
}
