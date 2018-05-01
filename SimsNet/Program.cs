using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GonzoNet;
using System.IO;
using ProtocolAbstractionLibraryD;
using SimsNet.Properties;
using FSO.Client.Utils.GameLocator;
using System.Reflection;
using FSO.Client;
using FSO.LotView;
using FSO.Client.Utils;
using System.Threading;
using FSO.Common;
using FSO.Content;

namespace SimsNet
{
    class Program
    {
        private static VMInstance Inst;
        private static bool UseDX;

        static void Main(string[] args)
        {

            Console.Title = "SimsNet 1.0";
            if (CreateProxy(true))
            {
                InitWithArguments(args);

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

        public static bool CreateProxy(bool useDX)  {

            GameStartProxy Proxy = new GameStartProxy();

            if (Proxy != null)
                return true;


            return false;
         

        }

        public static bool InitWithArguments(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Console.WriteLine("Loading Content...");
            ILocator locator;

            OperatingSystem os = Environment.OSVersion;
            PlatformID pid = os.Platform;

            bool linux = pid == PlatformID.MacOSX || pid == PlatformID.Unix;
            if (linux) locator = new LinuxLocator();
            else locator = new WindowsLocator();

            string path = locator.FindTheSimsOnline();

            bool useDX = false;

            UseDX = MonogameLinker.Link(useDX);

            
            if (path != null)
            {

                GlobalSettings.Default.Load();


                var simspath = locator.FindTheSimsComplete();

                FSOEnvironment.SimsCompleteDir = simspath;
                FSOEnvironment.ContentDir = "Content/";
                FSOEnvironment.GFXContentDir = "Content/" + (useDX ? "DX/" : "OGL/");
               

                GlobalSettings.Default.StartupPath = path;


                GlobalSettings.Default.Save();

            }
                    

            FSO.Content.Content.Init(path, null);
            Console.WriteLine("Success!");


            Console.WriteLine("Starting VM server...");

            PacketHandlers.Register((byte)PacketType.VM_PACKET, false, 0, new OnPacketReceive(VMPacket));

            StartVM();
            Stream inputStream = Console.OpenStandardInput();

            while (true)
                Inst.SendMessage(Console.ReadLine());
        }

        static private void VMPacket(NetworkClient client, ProcessedPacket packet)
        {

        }

        static void StartVM()
        {
            Inst = new VMInstance(37564);
            Console.WriteLine("Stunning success.");
            Inst.Start();
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
