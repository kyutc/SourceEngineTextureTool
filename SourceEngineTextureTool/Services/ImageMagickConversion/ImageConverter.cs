using System;
using System.Diagnostics;
using SourceEngineTextureTool.Services;

public class ImageConverter
{
    //Method to convert image to png32 with all optional parameters
    public static void ConvertToPNG32(string inputFilePath, string outputFilePath, bool autoCrop = false, int resizeWidth = 0, int resizeHeight = 0, int framesToProcess = 1, byte compositeR = 0, byte compositeG = 0, byte compositeB = 0, byte compositeA = 0)
    {
        ValidateInput(inputFilePath);

        //Builds the ImageMagick command based on parameters
        //Appends the parameters to the command with new values if they are updated, otherwise uses defaults
        string magickArguments = $"\"{inputFilePath}\"";

        if (autoCrop)
        {
            magickArguments += " -auto-crop";
        }

        if (resizeWidth > 0 && resizeHeight > 0)
        {
            magickArguments += $" -resize {resizeWidth}x{resizeHeight}";
        }

        if (framesToProcess > 1)
        {
            magickArguments += $" -coalesce -layers OptimizeTransparency -dispose Background";
        }

        
        //Concatenate R, G, B, and A into a single int32 using bit-shift operations
        int compositeColor = (compositeR << 24) | (compositeG << 16) | (compositeB << 8) | compositeA;

        //Convert the int32 to hexadecimal and prepend a hashtag
        string hexColor = compositeColor.ToString("X8");
        magickArguments += $" -fill \"#{hexColor}\" -colorize 100%";
        

        magickArguments += $" -type TrueColorMatte \"{outputFilePath}\"";

        var commandExecuted = ExecuteImageMagickCommand(ExternalDependencyManager.imagemagick, magickArguments);

        if (!commandExecuted)
        {
            throw new Exception("ImageMagick command execution failed.");
        }
    }
    
    private static bool ExecuteImageMagickCommand(string command, string arguments)
    {
        var processInfo = new ProcessStartInfo()
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process? e = Process.Start(processInfo);

        if (e == null) return false;
        e.WaitForExit();

        return e.ExitCode == 0;

    }

    //Checks if input file is valid and rejects it if not
    private static void ValidateInput(string inputFilePath)
    {
        //Checks if the input file path is empty or if the file does not exist at the specified location
        if (string.IsNullOrWhiteSpace(inputFilePath) || !System.IO.File.Exists(inputFilePath))
        {
            //Throws ArgumentException
            throw new ArgumentException("Invalid input file path or file does not exist.");
        }
    }
}
