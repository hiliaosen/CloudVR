using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRServerSDK
{
    class Utils
    {
        public static string GetDriverPath()
        {
            string exePath = Assembly.GetEntryAssembly().Location;

            string driverPath = Path.GetDirectoryName(exePath) + "\\driver";
            if (Environment.GetCommandLineArgs().Length >= 2)
            {
                driverPath = Environment.GetCommandLineArgs()[1];
            }
            // This is for compatibility to driver_uninstall.bat.
            // vrpathreg requires completely same path notation including trailing backslash.
            if (!driverPath.EndsWith("\\"))
            {
                driverPath += "\\";
            }
            return driverPath;
        }

        public static string GetOutputPath()
        {
            string exePath = Assembly.GetEntryAssembly().Location;

            return Path.GetDirectoryName(exePath) + "\\logs\\";
        }

        public static string GetDllDirectory(string driverPath)
        {
            return driverPath + @"bin\win64";
        }

        public static string GetDllPath(string driverPath)
        {
            return driverPath + @"bin\win64\driver_cloudvr_server.dll";
        }

        public static int ParseInt(string s)
        {
            int i = 0;
            int.TryParse(s, out i);
            return i;
        }

        public static float ParseFloat(string s)
        {
            float f = 0.0f;
            float.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out f);
            return f;
        }

        // Execute command without showing command prompt window.
        public static Process ExecuteProcess(string path, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            try
            {
                process.Start();
            }
            catch (Exception e)
            {

                string vrpathreg = DriverInstaller.GetVRPathRegPath();
                if (vrpathreg != null && vrpathreg.Equals(path))//如果是Steam或者SteamVR命令无法执行则提示可执行文件已经损坏
                {
                    MessageBox.Show
                    (
                        "Steam or SteamVR executable may be corrupted, please reinstall them.\r\nSteam或SteamVR可执行程序可能已损坏，请重新安装它们。",
                        "Warning 警告",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    Environment.Exit(0);
                }
                else//其它命令无法执行就直接抛出异常
                {
                    throw;
                }
            }
            return process;
        }

        public static void LaunchOnlySteamVR()
        {
            Process.Start("vrmonitor:");
        }

        public static void LaunchSteam()
        {
            Process.Start("steam://run/250820");
        }
    }
}
