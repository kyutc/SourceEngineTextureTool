using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf;
using SourceEngineTextureTool.Services.BinaryAccess.Vtf.v70;

namespace SourceEngineTextureTool.UnitTests.BinaryAccess.Vtf.v70;

[TestFixture]
public class WriterTestv70
{
    // public string TestVtfFile = "test_file70.vtf";

        Writer writer = new()
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
        
            [Test]
           public void Test_v70Version()
           {
               //Assert
               ClassicAssert.AreEqual((7,0), writer.Version);
           }
           [Test]
           public void Test_v70Width()
           {
               //Assert
               ClassicAssert.AreEqual(128, writer.Width);
           }
           
           [Test]
           public void Test_v70Height()
           {
               //Assert
               ClassicAssert.AreEqual(128, writer.Height);
           }
           
           [Test]
           public void Test_v70VtfFlags()
           {
               //Assert
               ClassicAssert.AreEqual(8256, writer.VtfFlags);
           }
           
           [Test]
           public void Test_v70Frames()
           {
               //Assert
               ClassicAssert.AreEqual(1, writer.Frames);
           }
           
           [Test]
           public void Test_v70FirstFrames()
           {
               //Assert
               ClassicAssert.AreEqual(0, writer.FirstFrame);
           }
           
           [Test]
           public void Test_v70Reflectivity()
           {
               //Assert
               ClassicAssert.AreEqual((0.189700007f, 0.391600013f, 0.552200019f), writer.Reflectivity);
           }
           
           [Test]
           public void Test_v70BumpMapScale()
           {
               //Assert
               ClassicAssert.AreEqual(1.0, writer.BumpmapScale);
           }
           
           [Test]
           public void Test_v70MipMapCount()
           {
               //Assert
               ClassicAssert.AreEqual(8, writer.MipmapCount);
           }
           
           [Test]
           public void Test_v70LowResWidth()
           {
               //Assert
               ClassicAssert.AreEqual(16, writer.LowResWidth);
           }
           
           [Test]
           public void Test_v70LowResHeight()
           {
               //Assert
               ClassicAssert.AreEqual(16, writer.LowResHeight);
           }
    }