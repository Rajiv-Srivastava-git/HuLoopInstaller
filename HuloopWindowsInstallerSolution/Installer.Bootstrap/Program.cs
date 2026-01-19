using System;
using System.Diagnostics;
using System.IO;

namespace Installer.Bootstrap
{
    // Simple helper to publish the UI project as a self-contained EXE and optionally call WiX to create an MSI.
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Huloop Installer Bootstrap - publish + package helper");
            var uiProj = Path.Combine("..", "Installer.UI", "Installer.UI.csproj");
            var outDir = Path.Combine("..", "publish-out");

            Directory.CreateDirectory(outDir);

            // Publish self-contained exe for win-x64
            var psi = new ProcessStartInfo("dotnet", $"publish \"{uiProj}\" -c Release -r win-x64 --self-contained true -o \"{outDir}\"") { UseShellExecute=false };
            var p = Process.Start(psi);
            p.WaitForExit();
            if (p.ExitCode != 0) return p.ExitCode;
            Console.WriteLine("Published UI to Publish-Out folder.");

            // Optionally build MSI using WiX if available (wix CLI)
            Console.WriteLine("Attempting to package MSI using WiX if 'wix' is available...");
            try
            {
                var wixCheck = Process.Start(new ProcessStartInfo("wix", "--version") { CreateNoWindow=true, UseShellExecute=false });
                wixCheck.WaitForExit();
                if (wixCheck.ExitCode == 0)
                {
                    // user should create Product.wxs and other wix files in a 'wix' folder
                    var wixFolder = Path.Combine(Directory.GetCurrentDirectory(), "wix");
                    if (Directory.Exists(wixFolder))
                    {
                        var build = Process.Start(new ProcessStartInfo("wix", $"build {wixFolder} -o HuloopInstaller.msi") { UseShellExecute=false });
                        build.WaitForExit();
                        Console.WriteLine("WiX build finished. Check HuloopInstaller.msi");
                    }
                    else
                    {
                        Console.WriteLine("No 'wix' folder found. Skipping MSI build. Place WiX .wxs files in a 'wix' folder to enable packaging.");
                    }
                }
                else
                {
                    Console.WriteLine("WiX CLI not available. Skipping MSI packaging. Install WiX toolset and dotnet wix tool to enable MSI creation.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while attempting WiX packaging: " + ex.Message);
            }

            return 0;
        }
    }
}
