using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        //Takes input from the user to get the file-path of the image for conversion
        Console.WriteLine("Enter the full path of the input image file (Make sure to include the filename):");
        string inputFilePath = Console.ReadLine();

        //Checks if the location specified and file are valid
        if (string.IsNullOrWhiteSpace(inputFilePath) || !System.IO.File.Exists(inputFilePath))
        {
            Console.WriteLine("Invalid input file path or file does not exist.");
            return;
        }

        //Generates the output file-path by changing the extension to "png"
        string outputFilePath = System.IO.Path.ChangeExtension(inputFilePath, "png");

        //Execute the ImageMagick command to convert the specified image to png32 format by specifying color depth
        ExecuteImageMagickCommand("magick", $"\"{inputFilePath}\" -type TrueColorMatte \"{outputFilePath}\"");

        //Displays a message that the file conversion is complete
        Console.WriteLine($"Conversion completed. Output file location: {outputFilePath}");
    }

    //Executes the imagemagick command
    static void ExecuteImageMagickCommand(string command, string arguments)
    {
        using (Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                // Specify the command to execute, in this case the "magick" command for file conversion
                FileName = command,
                Arguments = arguments,
                //Redirects errors to account for any error message
                RedirectStandardError = true,
                //Avoid using the system shell to execute the command
                UseShellExecute = false,
                //Do not create a visible window for the process
                CreateNoWindow = true
            }
        })
        {
            
            process.Start();
            //Waits for process to finish
            process.WaitForExit();

            //Reads error messages from the error stream
            string errorMessage = process.StandardError.ReadToEnd();
            //Displays any error messages
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Console.WriteLine($"Error: {errorMessage}");
            }
        }
    }
}
