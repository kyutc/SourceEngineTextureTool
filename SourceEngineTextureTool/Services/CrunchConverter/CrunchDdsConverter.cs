using System;
using System.Diagnostics;

namespace SourceEngineTextureTool.Services.CrunchConverter;

public class CrunchDdsConverter
{
    public static void ConvertToDdS(string inputFile, string outputFileFormat, string compressionFormat, string mipmapFilter, int rescaleWidth, int rescaleHeight, bool disableMipmaps)
    {
        ValidateInput(inputFile);

        // Construct the command string for crunch
        string arguments = $"-file \"{inputFile}\" -fileFormat {outputFileFormat} -{compressionFormat} -mipFilter {mipmapFilter} -rescale {rescaleWidth} {rescaleHeight}" +
                           (disableMipmaps ? " -mipMode None" : "");

        var commandExecuted = ExecuteCrunchCommand(ExternalDependencyManager.crunch,  arguments);
        
        if (!commandExecuted)
        {
            throw new Exception("Crunch command execution failed.");
        }
    }

    private static bool ExecuteCrunchCommand(string command, string arguments)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        Process? e = Process.Start(processInfo);

        if (e == null) return false;
        e.WaitForExit(); // Block until program completes

        return e.ExitCode == 0;
        
    }

    private static void ValidateInput(string inputFilePath)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath) || !System.IO.File.Exists(inputFilePath))
        {
            throw new ArgumentException("Invalid input file path or file does not exist.");
        }
    }
}
