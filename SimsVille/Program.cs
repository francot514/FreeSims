/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.IO;
using System.Threading;
using FSO.Client.Utils.GameLocator;
using FSO.Client.Utils;
using System.Reflection;
using FSO.Common;

namespace FSO.Client
{

    public static class Program
    {

        public static bool UseDX = true;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
             if (InitWithArguments(args))
                 (new GameStartProxy()).Start(UseDX);

        }

        public static bool InitWithArguments(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);


            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;

            ILocator gameLocator;
            bool linux = pid == PlatformID.MacOSX || pid == PlatformID.Unix;
            if (linux) gameLocator = new LinuxLocator();
            else gameLocator = new WindowsLocator();

            bool useDX = false;

            UseDX = MonogameLinker.Link(useDX);


            var path = gameLocator.FindTheSimsOnline();

            if (UseDX) GlobalSettings.Default.AntiAlias = false;

            if (path != null)
            {
                FSOEnvironment.ContentDir = "Content/";
                FSOEnvironment.GFXContentDir = "Content/" + (useDX ? "DX/" : "OGL/");
                FSOEnvironment.Linux = linux;
                FSOEnvironment.DirectX = useDX;
                if (GlobalSettings.Default.LanguageCode == 0) GlobalSettings.Default.LanguageCode = 1;
                Files.Formats.IFF.Chunks.STR.DefaultLangCode = (Files.Formats.IFF.Chunks.STRLangCode)GlobalSettings.Default.LanguageCode;

                GlobalSettings.Default.StartupPath = path;
                string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Sims Ville";
                if (!Directory.Exists(DocumentsPath))
                    Directory.CreateDirectory(DocumentsPath);
                GlobalSettings.Default.DocumentsPath = DocumentsPath;
                GlobalSettings.Default.Save();
                return true;
            }
            else
            {
                
                return false;
            }
        }


        private static System.Reflection.Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyPath = Path.Combine(MonogameLinker.AssemblyDir, args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll");
                var assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            catch (Exception)
            {
                return null;
            }

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //MessageBox.Show("Exception: \r\n" + e.ExceptionObject.ToString());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogThis.Log.LogThis("Exception: " + e.Exception.ToString(), LogThis.eloglevel.error);
            //MessageBox.Show("Exception: \r\n" + e.Exception.ToString());
        }

    }
}
