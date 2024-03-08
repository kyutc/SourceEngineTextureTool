using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Legacy;
using SourceEngineTextureTool.Services.BinaryAccess.Dds;

namespace SourceEngineTextureTool.UnitTests.BinaryAccess.Dds;

[TestFixture]
public class ReaderTest
{
    [Test]
    public void Test_ThrowsException()
    {
        //Arrange
        string DdsFilePath = @"C:\Users\Abraham\Desktop\chad.dds";

        //Act
        TestDelegate act = () => Reader.FromFile(DdsFilePath);

        //Assert
        Assert.Throws<Exception>(act);
    }

    [Test]
    public void Test_InvalidFileExtension()
    {
        //Arrange
        string invalidExtension = @"C:\Users\Abraham\Desktop\Burger.png";
        
        //Act
        TestDelegate act = () => Reader.FromFile(invalidExtension);
        
        //Assert
        Assert.Throws<Exception>(act);
    }

    [Test]
    public void Test_NonExistingFile()
    {
        //Arrange
        string filePath = @"C:\Users\Abraham\Desktop\nonexistent.dds";

        //Act
        TestDelegate act = () => Reader.FromFile(filePath);
        
        //Assert
        Assert.Throws<FileNotFoundException>(act);
    }

    [Test]
    public void Test_ValidDdsFile()
    {
        //Arrange
        string filePath = @"C:\Users\Abraham\Desktop\black.dds";

        //Act
        byte[] result = Reader.FromFile(filePath);

        //Assert
        ClassicAssert.NotNull(result);
    }
    
}