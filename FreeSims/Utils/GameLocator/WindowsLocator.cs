using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FSO.Client.Utils.GameLocator
{
    public class WindowsLocator : ILocator
    {

        private readonly string SteamRegistryPath = Environment.Is64BitOperatingSystem
            ? @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam"
            : @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";

        private readonly string SteamInstallPath;



        public string FindTheSimsOnline()
        {
            //string Software = "";

            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                //Find the path to TSO on the user's system.
                RegistryKey softwareKey = hklm.OpenSubKey("SOFTWARE");


                if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
                {
                    RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                    if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims Online", StringComparison.InvariantCultureIgnoreCase); }))
                    {
                        RegistryKey tsoKey = maxisKey.OpenSubKey("The Sims Online");
                        string installDir = (string)tsoKey.GetValue("InstallDir");
                        installDir += "\\TSOClient\\";
                        return installDir.Replace('\\', '/');
                    }
                }
            }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private static bool TryGetInstallDir(string manifestPath, out string installDir)
        {
            Regex regex = new Regex("\"installdir\"\\s+\"(?<dir>[^\"]+)\"", RegexOptions.Compiled);

            try
            {
                foreach (string line in File.ReadLines(manifestPath))
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        installDir = match.Groups["dir"].Value;
                        return true;
                    }
                }
            }
            catch (IOException) { } //ignore

            installDir = string.Empty;
            return false;
        }

        private static List<string> GetSteamLibraryPaths(string steamPath)
        {
            var libraries = new List<string> { steamPath };
            string libraryVdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (!File.Exists(libraryVdfPath))
                return libraries;

            try
            {
                foreach (string line in File.ReadLines(libraryVdfPath))
                {
                    Regex LibraryPathRegex = new Regex("\"path\"\\s+\"(?<path>[^\"]+)\"", RegexOptions.Compiled);
                    var match = LibraryPathRegex.Match(line);
                    if (!match.Success)
                        continue;

                    string path = match.Groups["path"].Value.Replace(@"\\", @"\");
                    if (Directory.Exists(path))
                        libraries.Add(path);
                }
            }
            catch (IOException) { } //ignore if file is locked or unreadable

            return libraries;
        }

        public string FindTheSimsComplete()
        {


            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                //Find the path to The Sims 1 on the user's system.
                RegistryKey softwareKey = hklm.OpenSubKey("SOFTWARE");


                if (Array.Exists(softwareKey.GetSubKeyNames(), delegate (string s) { return s.Equals("Maxis", StringComparison.InvariantCultureIgnoreCase); }))
                {
                    RegistryKey maxisKey = softwareKey.OpenSubKey("Maxis");
                    if (Array.Exists(maxisKey.GetSubKeyNames(), delegate (string s) { return s.Equals("The Sims", StringComparison.InvariantCultureIgnoreCase); }))
                    {
                        RegistryKey ts1Key = maxisKey.OpenSubKey("The Sims");
                        string installDir = (string)ts1Key.GetValue("InstallPath");
                        return installDir;
                    }
                }
            }

            string path = Registry.GetValue(SteamRegistryPath, "InstallPath", null)?.ToString();
            if (Directory.Exists(path))
            {

                string steamPath = SteamInstallPath;

                List<string> libraryVdfPathlibraries = GetSteamLibraryPaths(steamPath);

                List<string> libraries = GetSteamLibraryPaths(steamPath);


                foreach (string library in libraries)
                {
                    string manifestPath = Path.Combine(library, "steamapps", $"appmanifest_{3314060}.acf");
                    if (!File.Exists(manifestPath)) continue;

                    if (TryGetInstallDir(manifestPath, out var installDir))
                    {
                        string gamePath = Path.Combine(library, "steamapps", "common", installDir);
                        if (Directory.Exists(gamePath))
                            return (gamePath + "\\").Replace('\\', '/');
                    }
                }

              

            }


            return AppDomain.CurrentDomain.BaseDirectory;
        }



        private static bool is64BitProcess = (IntPtr.Size == 8);
        private static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        /// <summary>
        /// Determines if this process is run on a 64bit OS.
        /// </summary>
        /// <returns>True if it is, false otherwise.</returns>
        public static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
