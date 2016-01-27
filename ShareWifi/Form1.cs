using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace ShareWifi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Load += (sender, args) =>
            {
                if (!IsAdmin())
                {
                    RestartElevated();
                }

                StartShareWifi(Properties.Settings.Default.SSID, Properties.Settings.Default.KEY);
                Close();
            };
        }

        public void RestartElevated()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas"
            };
            try
            {
                Process.Start(startInfo);
            }
            catch { }

            Application.Exit();
        }

        public static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            if (id == null) return false;

            var p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void StartShareWifi(string ssid, string key)
        {
            var processStartInfo = GetCmdProcess();
            var process = Process.Start(processStartInfo);

            if (process == null) return;

            StopShareWifi();

            process.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=" + ssid + " key=" + key);
            process.StandardInput.Close();
            process.Close();


            process = Process.Start(processStartInfo);
            if (process == null) return;

            process.StandardInput.WriteLine("netsh wlan start hosted network");
            process.WaitForExit(5000);
            process.StandardInput.Close();
            process.Close();
        }

        private void StopShareWifi()
        {
            var processStartInfo = GetCmdProcess();
            var process = Process.Start(processStartInfo);

            if (process == null) return;
            process.StandardInput.WriteLine("netsh wlan stop hostednetwork");
            process.WaitForExit(5000);
            process.StandardInput.Close();
            process.Close();
        }

        private ProcessStartInfo GetCmdProcess()
        {
            var processStartInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return processStartInfo;
        }
    }
}
