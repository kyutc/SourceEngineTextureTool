using System;
using NUnit.Framework;
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
}