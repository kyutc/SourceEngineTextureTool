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
    private string assetsDirectory;
    
    [SetUp]
    public void Setup()
    {
        string assemblyDirectory = TestContext.CurrentContext.TestDirectory;
        
        assetsDirectory = Path.Combine(assemblyDirectory, "..","..","..","UnitTests", "ddsAssets");
    }

    [Test]
    public void Test_ThrowsException()
    {
        //Arrange
        string ddsFilePath = Path.Combine(assetsDirectory, "chad.dds");
        
        //Act
        TestDelegate act = () => Reader.FromFile(ddsFilePath);
        
        //Assert
        Assert.Throws<Exception>(act);
    }    
    
    [Test]
    public void Test_InvalidFileExtension()
    {
        //Arrange
        string invalidExtension = Path.Combine(assetsDirectory, "toothless-dance.dds");

        //Act
        TestDelegate act = () => Reader.FromFile(invalidExtension);

        //Assert
        Assert.Throws<Exception>(act);
    }

    [Test]
    public void Test_NonExistingFile()
    {
        //Arrange
        string filePath = Path.Combine(assetsDirectory, "nonexistent.dds");

        //Act
        TestDelegate act = () => Reader.FromFile(filePath);

        //Assert
        Assert.Throws<FileNotFoundException>(act);
    }
}