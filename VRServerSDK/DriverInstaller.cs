using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRServerSDK
{
    class DriverInstaller
    {
        // Execute "C:\Program Files (x86)\Steam\steamapps\common\SteamVR\bin\win32\vrpathreg.exe" adddriver "%~dp0
        public static bool InstallDriver()
        {
            string vrpathreg = GetVRPathRegPath();
            if (vrpathreg == null)
            {
                return false;
            }

            string driverPath = Utils.GetDriverPath();
            if (!Directory.Exists(driverPath))
            {
                MessageBox.Show("Driver path: " + driverPath + "\r\nis not found! Please check install location.");
                return false;
            }

            Utils.ExecuteProcess(vrpathreg, "adddriver \"" + driverPath + "\\\"").WaitForExit();

            return true;
        }

        public static bool UninstallDriver(string driverPath = null)
        {
            string vrpathreg = GetVRPathRegPath();
            if (vrpathreg == null)
            {
                return false;
            }

            if (driverPath == null)
            {
                driverPath = Utils.GetDriverPath();
            }
            // We don't check existence when uninstalling.
            Utils.ExecuteProcess(vrpathreg, "removedriver \"" + driverPath + "\\\"").WaitForExit();

            return true;
        }

        public static bool CheckInstalled()
        {
            string vrpathreg = GetVRPathRegPath();
            if (vrpathreg == null)
            {
                //throw new Exception();
                return false;
            }

            string driverPath = Utils.GetDriverPath();

            var process = Utils.ExecuteProcess(vrpathreg, "show");
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                if (line.Trim("\n\t ".ToCharArray()) == driverPath)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<string> GetDriverList()
        {
            string vrpathreg = GetVRPathRegPath();
            if (vrpathreg == null)
            {
                //throw new Exception();
                return new List<string>();
            }

            string driverPath = Utils.GetDriverPath();
            driverPath += "\\";

            var process = Utils.ExecuteProcess(vrpathreg, "show");
            string list = process.StandardOutput.ReadToEnd();
            int index = list.IndexOf("External Drivers:\r\n");
            if (index != -1)
            {
                var tmp = list.Substring(index + "External Drivers:\r\n".Length);
                var drivers = tmp.Split(new []{ "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                return drivers.ToList().Select(x => x.Trim()).ToList();
            }
            return new List<string>();
        }

        public static bool ListDrivers()
        {
            RemoveOtherDriverInstallations();
            MessageBox.Show("installed driver list:\r\n" + string.Join("\r\n", GetDriverList()), "Cloud VR");
            return true;
        }

        public static void RemoveOtherDriverInstallations()
        {
            string uninstalledList = "";
            foreach (var driver in GetDriverList())
            {
                if(driver.ToLower().Contains("\\driver\\"))//识别出本系统的驱动路径特征
                { 
                    if (driver != Utils.GetDriverPath())
                    {
                        UninstallDriver(driver);
                        uninstalledList += driver + "\r\n";
                    }
                }
            }

            if("" != uninstalledList)
            {
                MessageBox.Show("Other drivers installed on SteamVR have been uninstalled.\r\nUnloaded driver list:\r\n" + uninstalledList);
            }
        }

        public static void CheckDriverPath()
        {
            // SteamVR can't load driver which is installed in "multibyte" path.
            if (Utils.GetDriverPath().Any(c => c > 255))
            {
                MessageBox.Show("It seems the Cloud VR driver is installed in the path contains multibyte character. It will cause load error on SteamVR driver. Please install another location.");
            }
        }

        public static string GetVRPathRegPath()
        {
            RegistryKey regkey = Registry.ClassesRoot.OpenSubKey(@"vrmonitor\Shell\Open\Command", false);
            if (regkey == null)
            {
                MessageBox.Show("SteamVR is not installed.\r\n(Registry HKEY_CLASSES_ROOT\\vrmonitor\\Shell\\Open\\Command was not found.)\r\nPlease install and retry.");
                return null;
            }
            string path = (string)regkey.GetValue("");

            var m = Regex.Match(path, "^\"(.+)bin\\\\([^\\\\]+)\\\\vrmonitor.exe\" \"%1\"$");
            if (!m.Success)
            {
                MessageBox.Show("Invalid value in registry HKEY_CLASSES_ROOT\\vrmonitor\\Shell\\Open\\Command.");
                return null;
            }
            return m.Groups[1].Value + @"bin\win32\vrpathreg.exe";
        }
    }
}
