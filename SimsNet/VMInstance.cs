/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSO.SimAntics;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.SimAntics.Marshals;
using FSO.SimAntics.Model;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.SimAntics.Utils;
using FSO.LotView;
using SimsNet.Properties;
using FSO.LotView.Model;
using FSO.Files.Formats.IFF;

namespace SimsNet
{
    public class VMInstance
    {
        public VM state;
        private Stopwatch timeKeeper;
        private int TicksSinceSave;
        private static int SaveTickFreq = 60 * 60; //save every minute for safety
        private int Port;
        private bool TS1, Dedicated;

        public VMInstance(int port)
        {
            VM.UseWorld = false;
            Port = port;
            TS1 = false;
            Dedicated = false;
            ResetVM();
        }

        public void LoadState(VM vm, string path)
        {
            using (var file = new BinaryReader(File.OpenRead(path)))
            {
                var marshal = new VMMarshal();
                marshal.Deserialize(file);
                vm.Load(marshal);
                CleanLot();
                vm.Reset();
            }
        }

        public void ResetVM()
        {
            VMNetDriver driver;
            driver = new VMServerDriver(Port, NetClosed);

            var vm = new VM(new VMContext(null),  new VMNullHeadlineProvider());
            state = vm;
            vm.Init();
            vm.VM_SetDriver(driver);
            vm.OnChatEvent += Vm_OnChatEvent;

            Console.WriteLine("Select the lot type");
            Console.WriteLine("1-Empty");
            Console.WriteLine("2-Blueprint");
            Console.WriteLine("3-House");
            
            string path = "";
            int lot = Convert.ToInt32(Console.ReadLine());

            if (lot == 1)
            {

                if (Settings.Default.DebugLot != String.Empty)
                    path = AppDomain.CurrentDomain.BaseDirectory + "Content/Houses/" + Settings.Default.DebugLot;
                else
                    path = AppDomain.CurrentDomain.BaseDirectory + "Content/Houses/empty_lot.xml";

            }     
            else if (lot == 2)
            {

                    Console.WriteLine("Specify lot name");
                    path = AppDomain.CurrentDomain.BaseDirectory + "Content/Houses/" + Console.ReadLine() + ".xml";


            }
            else if (lot == 3)
            {

                    Console.WriteLine("Specify house name");
                    path = AppDomain.CurrentDomain.BaseDirectory + "Content/Houses/" + Console.ReadLine() + ".iff";
                    TS1 = true;

            }
            
            
            XmlHouseData lotInfo;
            IffFile HouseInfo = null;
            string filename = Path.GetFileName(path);

            if (!TS1)
                try
            {
                //try to load from FSOV first.
                LoadState(vm, "Content/LocalHouse/" + filename.Substring(0, filename.Length - 4) + ".fsov");
            }
            catch (Exception)
            {
                try
                {
                    Console.WriteLine("Failed FSOV load... Trying Backup");
                    LoadState(vm, "Content/LocalHouse/" + filename.Substring(0, filename.Length - 4) + "_backup.fsov");
                }
                catch (Exception)
                {
                    Console.WriteLine("CRITICAL::: Failed FSOV load... Trying Blueprint (first run or something went wrong)");
                    short jobLevel = -1;

                    //quick hack to find the job level from the chosen blueprint
                    //the final server will know this from the fact that it wants to create a job lot in the first place...

                    try
                    {
                        if (filename.StartsWith("nightclub") || filename.StartsWith("restaurant") || filename.StartsWith("robotfactory"))
                            jobLevel = Convert.ToInt16(filename.Substring(filename.Length - 9, 2));
                    }
                    catch (Exception) { }

                        //vm.SendCommand(new VMBlueprintRestoreCmd
                        //{
                        // JobLevel = jobLevel,
                        // XMLData = File.ReadAllBytes(path)
                        //});

                        using (var stream = new MemoryStream(File.ReadAllBytes(path)))
                        {
                            lotInfo = XmlHouseData.Parse(stream);
                        }

                        VMWorldActivator activator = new VMWorldActivator(vm, vm.Context.World);

                        vm.Activator = activator;

                        var blueprint = activator.LoadFromXML(lotInfo);

                        if (VM.UseWorld)
                        {
                            vm.Context.World.InitBlueprint(blueprint);
                            vm.Context.Blueprint = blueprint;
                        }


                        vm.Context.Clock.Hours = 10;

                        vm.MyUID = uint.MaxValue - 1;


                    }

              }
            else
            {



                    if (File.Exists(path))
                    {
                        HouseInfo = new IffFile(path);
                    }

                    VMWorldActivator activator = new VMWorldActivator(vm, vm.Context.World);

                    vm.Activator = activator;

                    var blueprint = activator.LoadFromIff(HouseInfo);

                    if (VM.UseWorld)
                    {
                        vm.Context.World.InitBlueprint(blueprint);
                        vm.Context.Blueprint = blueprint;
                    }
                  

                    vm.Context.Clock.Hours = 10;

                vm.MyUID = uint.MaxValue - 1;

            }
              
                              


            Console.WriteLine("Select the server type");
            Console.WriteLine("1-Host");
            Console.WriteLine("2-Dedicated");

            int host = Convert.ToInt32(Console.ReadLine());

            if (host == 1)
                vm.SendCommand(new VMNetSimJoinCmd
                {
                    ActorUID = uint.MaxValue - 1,
                    Name = "server"
                });
            else if (host == 2)
                Dedicated = true;
        }

        private void CleanLot()
        {
            Console.WriteLine("Cleaning up the lot...");
            var avatars = new List<VMEntity>(state.Entities.Where(x => x is VMAvatar && x.PersistID > 65535));
            //TODO: all avatars with persist ID are not npcs in TSO. right now though everything has a persist ID...
            //step 1, force everyone to leave.
            foreach (var avatar in avatars)
                state.ForwardCommand(new VMNetSimLeaveCmd()
                {
                    ActorUID = avatar.PersistID,
                    FromNet = false
                });

            //simulate for a bit to try get rid of the avatars on the lot
            try
            {
                for (int i = 0; i < 30 * 60 && state.Entities.FirstOrDefault(x => x is VMAvatar && x.PersistID > 65535) != null; i++)
                {
                    if (i == 30 * 60 - 1) Console.WriteLine("Failed to clean lot...");
                    state.Update();
                }
            }
            catch (Exception) { } //if something bad happens just immediately try to delete everyone

            avatars = new List<VMEntity>(state.Entities.Where(x => x is VMAvatar && (x.PersistID > 65535 || (!(x as VMAvatar).IsPet))));
            foreach (var avatar in avatars) avatar.Delete(true, state.Context);
        }

        private void NetClosed(VMCloseNetReason reason)
        {
            if (reason != VMCloseNetReason.ServerShutdown) return; //only handle clean closes
            var server = state.GetObjectByPersist(uint.MaxValue - 1);
            if (server != null) server.Delete(true, state.Context);
            CleanLot();
            SaveLot();
            System.Environment.Exit(0);
        }

        private void Vm_OnChatEvent(FSO.SimAntics.NetPlay.Model.VMChatEvent evt)
        {
            var print = "";
            switch (evt.Type)
            {
                case VMChatEventType.Message:
                    print = "<%> says: ".Replace("%", evt.Text[0]) + evt.Text[1]; break;
                case VMChatEventType.MessageMe:
                    print = "You say: " + evt.Text[1]; break;
                case VMChatEventType.Join:
                    print = "<%> has entered the property.".Replace("%", evt.Text[0]); break;
                case VMChatEventType.Leave:
                    print = "<%> has left the property.".Replace("%", evt.Text[0]); break;
                case VMChatEventType.Arch:
                    print = "<" + evt.Text[0] + " (" + evt.Text[1] + ")" + "> " + evt.Text[2]; break;
                case VMChatEventType.Generic:
                    print = evt.Text[0]; break;
            }

            Console.WriteLine(print);
        }

        public void SendMessage(string msg)
        {
            state.SendCommand(new VMNetChatCmd
            {
                ActorUID = uint.MaxValue - 1,
                Message = msg
            });
        }

        public void Start()
        {
            Thread oThread = new Thread(new ThreadStart(TickVM));
            oThread.Start();
        }

        public void SaveLot()
        {
            string filename = Path.GetFileName(Settings.Default.DebugLot);
            var exporter = new VMWorldExporter();
            exporter.SaveHouse(state, Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Content/Houses/" + filename));

            var marshal = state.Save();
            Directory.CreateDirectory("Content/LocalHouse/");
            var extensionless = filename.Substring(0, filename.Length - 4);

            //backup old state
            try { File.Copy("Content/LocalHouse/" + extensionless + ".fsov", "Content/LocalHouse/" + extensionless + "_backup.fsov", true); }
            catch (Exception) { }

            using (var output = new FileStream("Content/LocalHouse/"+extensionless+".fsov", FileMode.Create))
            {
                marshal.SerializeInto(new BinaryWriter(output));
            }

            ((VMTSOGlobalLinkStub)state.GlobalLink).Database.Save();
        }

        private void TickVM()
        {
            timeKeeper = new Stopwatch();
            timeKeeper.Start();
            long lastMs = 0;
            while (true)
            {
                lastMs += 16;
                TicksSinceSave++;
                try {
                    state.Update();
                } catch (Exception e)
                {
                    state.CloseNet(VMCloseNetReason.Unspecified);
                    Console.WriteLine(e.ToString());

                    if (!TS1 && !Dedicated)
                        SaveLot();
                    Thread.Sleep(500);

                    ResetVM();
                    //restart on exceptions... but print them to console
                    //just for people who like 24/7 servers.
                }

                if (TicksSinceSave > SaveTickFreq)
                {
                    //quick and dirty periodic save
                    if (!TS1)
                    SaveLot();
                    TicksSinceSave = 0;
                }

                Thread.Sleep((int)Math.Max(0, (lastMs + 16)-timeKeeper.ElapsedMilliseconds));
            }
        }
    }
}
