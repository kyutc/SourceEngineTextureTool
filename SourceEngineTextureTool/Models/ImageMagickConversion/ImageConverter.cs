using System;
using System.Diagnostics;

public class ImageConverter
{
    //Method to convert image to png32
    public static void ConvertToPNG32(string inputFilePath, string outputFilePath)
    {
        ValidateInput(inputFilePath);

<<<<<<< Updated upstream
        ExecuteImageMagickCommand("magick", $"\"{inputFilePath}\" -type TrueColorMatte \"{outputFilePath}\"");
=======
        //Builds the ImageMagick command based on parameters
        //Appends the parameters to the command with new values if they are updated, otherwise uses defaults
        string magickCommand = "convert";
        string magickArguments = $"\"{inputFilePath}\"";

        if (autoCrop)
        {
            magickArguments += " -auto-crop";
        }

        if (scalePercent != 100)
        {
            magickArguments += $" -resize {scalePercent}%";
        }

        if (framesToProcess > 1)
        {
            magickArguments += $" -coalesce -layers OptimizeTransparency -dispose Background";
        }

        if (!string.IsNullOrEmpty(compositeImage))
        {
            magickArguments += $" \"{compositeImage}\" -composite";
        }

        magickArguments += $" -type TrueColorMatte \"{outputFilePath}\"";

        ExecuteImageMagickCommand(magickCommand, magickArguments);
>>>>>>> Stashed changes
    }

    //Method which executes the ImageMagick commands automatically. Runs the magick command to convert to png32
    private static void ExecuteImageMagickCommand(string command, string arguments)
    {
        try
        {
            //Creates a new process that uses the following configuration
            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            })
            {
                //Starts the previously outlined process
                process.Start();
                //Waits for the process to finish
                process.WaitForExit();

                string errorMessage = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    throw new Exception($"Error: {errorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            //Handles exceptions or rethrows them with a message
            throw new Exception("ImageMagick command execution failed.", ex);
        }
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