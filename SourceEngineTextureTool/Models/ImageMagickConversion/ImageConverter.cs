using System;
using System.Diagnostics;

public class ImageConverter
{
    //Method to convert image to png32
    public static void ConvertToPNG32(string inputFilePath)
    {
        //Checks if input file is valid and rejects it if not
        if (string.IsNullOrWhiteSpace(inputFilePath) || !System.IO.File.Exists(inputFilePath))
        {
            Console.WriteLine("Invalid input file path or file does not exist.");
            return;
        }
        
        //Generates the output file
        string outputFilePath = System.IO.Path.ChangeExtension(inputFilePath, "png");

        ExecuteImageMagickCommand("magick", $"\"{inputFilePath}\" -type TrueColorMatte \"{outputFilePath}\"");

        Console.WriteLine($"PNG32 File Conversion Complete. Output File Located At: {outputFilePath}");
    }

    //Method which executes the ImageMagick commands automatically. Runs the magick command to convert to png32
    private static void ExecuteImageMagickCommand(string command, string arguments)
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

            //Captures, reads, and prints any error message thrown
            string errorMessage = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine($"Error: {errorMessage}");
            }
        }
    }
}