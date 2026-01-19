using System.Diagnostics;

namespace Installer.Core
{
    public class WindowsServiceInstaller
    {
        public string ServiceName { get; }
        public WindowsServiceInstaller(string serviceName)
        {
            ServiceName = serviceName;
        }

        public void Install(string exePath, string displayName, string description)
        {
            // Use sc.exe to create service. Requires admin.
            var psi = new ProcessStartInfo("sc.exe", $"create \"{ServiceName}\" binPath= \"{exePath}\" DisplayName= \"{displayName}\" start= auto")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            p.WaitForExit();
            // description
            var psiDesc = new ProcessStartInfo("sc.exe", $"description \"{ServiceName}\" \"{description}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p2 = Process.Start(psiDesc);
            p2.WaitForExit();
        }

        public void Uninstall()
        {
            var psi = new ProcessStartInfo("sc.exe", $"delete \"{ServiceName}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        public void Start()
        {
            var psi = new ProcessStartInfo("sc.exe", $"start \"{ServiceName}\"") { CreateNoWindow = true, UseShellExecute = false };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        public void Stop()
        {
            var psi = new ProcessStartInfo("sc.exe", $"stop \"{ServiceName}\"") { CreateNoWindow = true, UseShellExecute = false };
            var p = Process.Start(psi);
            p.WaitForExit();
        }
    }
}
