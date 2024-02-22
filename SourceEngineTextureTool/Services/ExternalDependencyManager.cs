using System;
using System.Diagnostics;

namespace SourceEngineTextureTool.Services;

public static class ExternalDependencyManager
{
    public static readonly string imagemagick;
    public static readonly string ffmpeg;
    public static readonly string crunch;

    private static readonly Architecture _arch;
    private static readonly OperatingSystem _OS;

    private enum Architecture
    {
        x86,
        x86_64,
    }

    private enum OperatingSystem
    {
        Windows,
        Linux,
        OSX,
    }
    
    static ExternalDependencyManager()
    {
        _arch = _getArch();
        _OS = _getOS();

        imagemagick = _OS == OperatingSystem.Windows ? "convert.exe" : "convert";
        ffmpeg = _OS == OperatingSystem.Windows ? "ffmpeg.exe" : "ffmpeg";
        crunch = _OS == OperatingSystem.Windows ?
            (_arch == Architecture.x86 ? "crunch.exe" : "crunch_x64.exe") : 
            (_arch == Architecture.x86 ? "crunch" : "crunch_x64");

        if (!(_testCrunch() && _testFfmpeg() && _testImageMagick()))
        {
            // TODO: Is it worthwhile to create a unique error for each of these?
            throw new Exception(
                "External binary dependency error: crunch, ffmpeg, and/or imagemagick missing or not functional.");
        }
    }

    private static Architecture _getArch()
    {
        // The program can be run in 32-bit mode on a 64-bit OS, so check both cases.
        return System.Environment.Is64BitProcess || System.Environment.Is64BitOperatingSystem ?
            Architecture.x86_64 : Architecture.x86;
    }

    private static OperatingSystem _getOS()
    {
        return System.Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => OperatingSystem.Windows,
            PlatformID.Unix => OperatingSystem.Linux,
            PlatformID.MacOSX => OperatingSystem.OSX,
            _ => throw new Exception("Unsupported operating system.")
        };
    }

    private static bool _testExec(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        Process? p = Process.Start(psi);

        if (p == null) return false;
        p.WaitForExit(); // Block until program completes

        return p.ExitCode == 0;
    }

    private static bool _testImageMagick()
    {
        return _testExec(imagemagick, "--version");
    }

    private static bool _testFfmpeg()
    {
        return _testExec(ffmpeg, "-version");
    }

    private static bool _testCrunch()
    {
        // TODO: Decide how to distribute the test vector file black.dds, or accept an error code of 1 since crunch
        // always throws an error without any valid arguments and it has no help/version page.
        return _testExec(crunch, "-info -file black.dds");
    }
}