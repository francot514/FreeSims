/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Panels;
using FSO.Client.UI.Model;
using Microsoft.Xna.Framework;
using FSO.Client.Utils;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework.IO;
using FSO.Common.Rendering.Framework;
using FSO.Client.Network;
using FSO.LotView;
using FSO.LotView.Model;
using FSO.SimAntics;
using FSO.SimAntics.Utils;
using FSO.SimAntics.Primitives;
using TSO.HIT;
using FSO.SimAntics.NetPlay.Drivers;
using FSO.SimAntics.NetPlay.Model.Commands;
using System.IO;
using FSO.SimAntics.NetPlay;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Panels.WorldUI;
using FSO.SimAntics.Engine.TSOTransaction;
using FSO.Common;
using SimsVille.UI.Model;
using FSO.Client.Rendering.City;
using tso.world.Model;
using FSO.Vitaboy;
using FSO.SimAntics.Model.TSOPlatform;
using Microsoft.Xna.Framework.Graphics;
using FSO.Files.Formats.IFF;
using FSO.Debug;

namespace FSO.Client.UI.Screens
{
    public class CoreGameScreen : FSO.Client.UI.Framework.GameScreen
    {
        public UIUCP ucp;
        public UIGizmo gizmo;
        public UIInbox Inbox;
        public UIGameTitle Title;
        private UIButton SaveHouseButton;
        private UIButton VMDebug;
        private UIButton CreateChar;
        private string[] CityMusic;
        private String city, lotName;

        private string[] CharacterInfos;      
        public List<XmlCharacter> Characters;

        private bool Connecting, Permissions;
        private UILoginProgress ConnectingDialog;
        private Queue<SimConnectStateChange> StateChanges;
        private UIMouseEventRef MouseHitAreaEventRef = null;
        private Neighborhood CityRenderer; //city view

        public UILotControl LotController; //world, lotcontrol and vm will be null if we aren't in a lot.
        private LotView.World World;
        public FSO.SimAntics.VM vm;
        public bool InLot
        {
            get
            {
                return (vm != null);
            }
        }

        private int m_ZoomLevel;
        public int ZoomLevel
        {
            get
            {
                return m_ZoomLevel;
            }
            set
            {
                value = Math.Max(1, Math.Min(5, value));
                if (value < 4)
                {
                    if (vm == null) ZoomLevel = 4; //call this again but set minimum cityrenderer view
                    else
                    {
                        Title.SetTitle(LotController.GetLotTitle());
                        if (m_ZoomLevel > 3)
                        {
                            HITVM.Get().PlaySoundEvent(UIMusic.None);
                            gizmo.Visible = false;
                            CityRenderer.Visible = false;
                            LotController.Visible = true;
                            World.Visible = true;
                            ucp.SetMode(UIUCP.UCPMode.LotMode);
                        }
                        m_ZoomLevel = value;
                        vm.Context.World.State.Zoom = (WorldZoom)(4 - ZoomLevel); //near is 3 for some reason... will probably revise
                    }
                }
                else //cityrenderer! we'll need to recreate this if it doesn't exist...
                {
                    
                        Title.SetTitle(city);                  

                        if (m_ZoomLevel < 4)
                        {   
                            //coming from lot view... snap zoom % to 0 or 1
                            CityRenderer.m_ZoomProgress = (value == 4) ? 1 : 0;
                            //PlayBackgroundMusic(CityMusic); //play the city music as well
                            CityRenderer.Visible = true;

                            HITVM.Get().PlaySoundEvent(UIMusic.Map); //play the city music as well
                            gizmo.Visible = true;
                            if (World != null)
                            {
                                World.Visible = false;
                                LotController.Visible = false;
                            }
                            ucp.SetMode(UIUCP.UCPMode.CityMode);
                        }
                        m_ZoomLevel = value;

                        CityRenderer.m_Zoomed = (value == 4);

                }
                ucp.UpdateZoomButton();
            }
        } //in future, merge LotDebugScreen and CoreGameScreen so that we can store the City+Lot combo information and controls in there.

        private int _Rotation = 0;
        public int Rotation
        {
            get
            {
                return _Rotation;
            }
            set
            {
                _Rotation = value;
                if (World != null)
                {
                    switch (_Rotation)
                    {
                        case 0:
                            World.State.Rotation = WorldRotation.TopLeft; break;
                        case 1:
                            World.State.Rotation = WorldRotation.TopRight; break;
                        case 2:
                            World.State.Rotation = WorldRotation.BottomRight; break;
                        case 3:
                            World.State.Rotation = WorldRotation.BottomLeft; break;
                    }
                }
            }
        }

        public sbyte Level
        {
            get
            {
                if (World == null) return 1;
                else return World.State.Level;
            }
            set
            {
                if (World != null)
                {
                    World.State.Level = value;
                }
            }
        }

        public sbyte Stories
        {
            get
            {
                if (World == null) return 2;
                return World.Stories;
            }
        }

        public CoreGameScreen() : base()
        {
            /** City Scene **/
            ListenForMouse(new Rectangle(0, 0, ScreenWidth, ScreenHeight), new UIMouseEvent(MouseHandler));


            city = "Queen Margaret's";
            if (PlayerAccount.CurrentlyActiveSim != null)
                city = PlayerAccount.CurrentlyActiveSim.ResidingCity.Name;


            StateChanges = new Queue<SimConnectStateChange>();

            /**
            * Music
            */
            CityMusic = new string[]{
                GlobalSettings.Default.StartupPath + "\\music\\modes\\map\\tsobuild1.mp3",
                GlobalSettings.Default.StartupPath + "\\music\\modes\\map\\tsobuild3.mp3",
                GlobalSettings.Default.StartupPath + "\\music\\modes\\map\\tsomap2_v2.mp3",
                GlobalSettings.Default.StartupPath + "\\music\\modes\\map\\tsomap3.mp3",
                GlobalSettings.Default.StartupPath + "\\music\\modes\\map\\tsomap4_v1.mp3"
            };
            HITVM.Get().PlaySoundEvent(UIMusic.Map);

            VMDebug = new UIButton()
            {
                Caption = "Simantics",
                Y = 45,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            VMDebug.OnButtonClick += new ButtonClickDelegate(VMDebug_OnButtonClick);
            this.Add(VMDebug);
            //InitializeMouse();

            
            CharacterInfos = new string[9];


            SaveHouseButton = new UIButton()
            {
                Caption = "Save House",
                Y = 10,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            SaveHouseButton.OnButtonClick += new ButtonClickDelegate(SaveHouseButton_OnButtonClick);
            this.Add(SaveHouseButton);
            SaveHouseButton.Visible = false;

            CreateChar = new UIButton()
            {
                Caption = "Create Sim",
                Y = 10,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            CreateChar.OnButtonClick += new ButtonClickDelegate(CreateChar_OnButtonClick);
            CreateChar.Visible = true;
            this.Add(CreateChar);

            ucp = new UIUCP(this);
            ucp.Y = ScreenHeight - 210;
            ucp.SetInLot(false);
            ucp.UpdateZoomButton();
            ucp.MoneyText.Caption = PlayerAccount.Money.ToString();
            this.Add(ucp);

            gizmo = new UIGizmo();
            gizmo.X = ScreenWidth - 500;
            gizmo.Y = ScreenHeight - 300;
            this.Add(gizmo);

            Title = new UIGameTitle();
            Title.SetTitle(city);
            this.Add(Title);

            

            //OpenInbox();

            this.Add(GameFacade.MessageController);
            GameFacade.MessageController.OnSendLetter += new LetterSendDelegate(MessageController_OnSendLetter);
            GameFacade.MessageController.OnSendMessage += new MessageSendDelegate(MessageController_OnSendMessage);

            NetworkFacade.Controller.OnNewTimeOfDay += new OnNewTimeOfDayDelegate(Controller_OnNewTimeOfDay);
            NetworkFacade.Controller.OnPlayerJoined += new OnPlayerJoinedDelegate(Controller_OnPlayerJoined);


            CityRenderer = new Neighborhood(GameFacade.Game.GraphicsDevice);
            CityRenderer.LoadContent();

            CityRenderer.Initialize(GameFacade.HousesDataRetriever);


            CityRenderer.SetTimeOfDay(0.5);

            GameFacade.Scenes.Add(CityRenderer);

            ZoomLevel = 4; //Nhood view.

            
        }

        private void InitializeMouse()
        {
            /** City Scene **/
            UIContainer mouseHitArea = new UIContainer();
            MouseHitAreaEventRef = mouseHitArea.ListenForMouse(new Rectangle(0, 0, ScreenWidth, ScreenHeight), new UIMouseEvent(MouseHandler));
            AddAt(0, mouseHitArea);
        }

        public override void GameResized()
        {
            base.GameResized();
            Title.SetTitle(Title.Label.Caption);
            ucp.Y = ScreenHeight - 210;
            gizmo.X = ScreenWidth - 430;
            gizmo.Y = ScreenHeight - 230;
            //MessageTray.X = ScreenWidth - 70;

            //if (World != null)
               // World.GameResized();
            var oldPanel = ucp.CurrentPanel;
            ucp.SetPanel(-1);
            ucp.SetPanel(oldPanel);
            if (MouseHitAreaEventRef != null)
            {
                MouseHitAreaEventRef.Region = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
            }

        }

        #region Network handlers

        private void Controller_OnNewTimeOfDay(DateTime TimeOfDay)
        {
            if (TimeOfDay.Hour <= 12)
                ucp.TimeText.Caption = TimeOfDay.Hour + ":" + TimeOfDay.Minute + "am";
            else ucp.TimeText.Caption = TimeOfDay.Hour + ":" + TimeOfDay.Minute + "pm";

            double time = TimeOfDay.Hour / 24.0 + TimeOfDay.Minute / (1440.0) + TimeOfDay.Second / (86400.0);
        }

        private void Controller_OnPlayerJoined(LotTileEntry TileEntry)
        {
            
        }

        #endregion

        private void MessageController_OnSendMessage(string message, string GUID)
        {
            //TODO: Implement special packet for message (as opposed to letter)?
            //Don't send empty strings!!
            Network.UIPacketSenders.SendLetter(Network.NetworkFacade.Client, message, "Empty", GUID);
        }

        /// <summary>
        /// Message was sent by player to another player.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="subject">Subject of message.</param>
        /// <param name="destinationUser">GUID of destination user.</param>
        private void MessageController_OnSendLetter(string message, string subject, string destinationUser)
        {
            Network.UIPacketSenders.SendLetter(Network.NetworkFacade.Client, message, subject, destinationUser);
        }

        public override void Update(FSO.Common.Rendering.Framework.Model.UpdateState state)
        {
            GameFacade.Game.IsFixedTimeStep = (vm == null || vm.Ready);

            base.Update(state);


            lock (StateChanges)
            {
                while (StateChanges.Count > 0)
                {
                    var e = StateChanges.Dequeue();
                    ClientStateChangeProcess(e.State, e.Progress);
                }
            }

            if (vm != null) vm.Update();

            if (!Visible)
                CityRenderer.Visible = false;

            if (ZoomLevel < 4)
            {

                CreateChar.Visible = false;
                SaveHouseButton.Visible = true;

            }
            else if (ZoomLevel >= 4)
            {

                CreateChar.Visible = true;
                SaveHouseButton.Visible = false;

            }
        }

        public void CleanupLastWorld()
        {
            if (ZoomLevel < 4) ZoomLevel = 5;
            vm.Context.Ambience.Kill();
            foreach (var ent in vm.Entities) { //stop object sounds
                var threads = ent.SoundThreads;
                for (int i = 0; i < threads.Count; i++)
                {
                    threads[i].Sound.RemoveOwner(ent.ObjectID);
                }
                threads.Clear();
            }
            vm.CloseNet(VMCloseNetReason.LeaveLot);
            
            GameFacade.Scenes.Remove(World);
            this.Remove(LotController);
            ucp.SetPanel(-1);
            ucp.SetInLot(false);
        }

        public void ClientStateChange(int state, float progress)
        {
            lock (StateChanges) StateChanges.Enqueue(new SimConnectStateChange(state, progress));
        }

        public void ClientStateChangeProcess(int state, float progress)
        {
            //TODO: queue these up and try and sift through them in an update loop to avoid UI issues. (on main thread)
            if (state == 4) //disconnected
            {
                var reason = (VMCloseNetReason)progress;
                if (reason == VMCloseNetReason.Unspecified)
                {
                    var alert = UIScreen.ShowAlert(new UIAlertOptions
                    {
                        Title = GameFacade.Strings.GetString("222", "3"),
                        Message = GameFacade.Strings.GetString("222", "2", new string[] { "0" }),
                    }, true);

                    if (Connecting)
                    {
                        UIScreen.RemoveDialog(ConnectingDialog);
                        ConnectingDialog = null;
                        Connecting = false;
                    }

                    alert.ButtonMap[UIAlertButtonType.OK].OnButtonClick += DisconnectedOKClick;
                } else
                {
                    DisconnectedOKClick(null);
                }
            }

            if (ConnectingDialog == null) return;
            switch (state)
            {
                case 1:
                    ConnectingDialog.ProgressCaption = GameFacade.Strings.GetString("211", "26");
                    ConnectingDialog.Progress = 25f;
                    break;
                case 2:
                    ConnectingDialog.ProgressCaption = GameFacade.Strings.GetString("211", "27");
                    ConnectingDialog.Progress = 100f*(0.5f+progress*0.5f);
                    break;
                case 3:
                    UIScreen.RemoveDialog(ConnectingDialog);
                    ConnectingDialog = null;
                    Connecting = false;
                    ZoomLevel = 1;
                    ucp.SetInLot(true);
                    break;
            }
        }

        private void DisconnectedOKClick(UIElement button)
        {
            if (vm != null) CleanupLastWorld();
            Connecting = false;
        }

        public void InitTestLot(string path, string name, bool host, bool TS1)
        {
            if (Connecting) return;

            lotName = name.Split('.')[0];



            Characters = new List<XmlCharacter>();

            SaveHouseButton.Visible = true;
            CreateChar.Visible = false;

            if (vm != null) CleanupLastWorld();

            World = new LotView.World(GameFacade.Game.GraphicsDevice);
            GameFacade.Scenes.Add(World);

            

            vm = new VM(new VMContext(World), new UIHeadlineRendererProvider());
            vm.Init();
            vm.LotName = (path == null) ? "localhost" : path.Split('/').LastOrDefault(); //quick hack just so we can remember where we are
            

            var DirectoryInfo = new DirectoryInfo(Path.Combine(FSOEnvironment.UserDir, "Characters/"));

            for (int i = 0; i <= DirectoryInfo.GetFiles().Count() - 1; i++)
            {


                var file = DirectoryInfo.GetFiles()[i];
                CharacterInfos[i] = Path.GetFileNameWithoutExtension(file.FullName);

                if (CharacterInfos[i] != null && CharacterInfos[i] != gizmo.SelectedCharInfo.Name)
                {
                    Characters.Add(XmlCharacter.Parse(file.FullName));


                }

            }


            VMNetDriver driver;
            if (host )
            {

                driver = new VMServerDriver(37564, null);
            }
            else
            {
                Connecting = true;
                ConnectingDialog = new UILoginProgress();

                ConnectingDialog.Caption = GameFacade.Strings.GetString("211", "1");
                ConnectingDialog.ProgressCaption = GameFacade.Strings.GetString("211", "24");
                //this.Add(ConnectingDialog);

                UIScreen.ShowDialog(ConnectingDialog, true);

                driver = new VMClientDriver(path, 37564, ClientStateChange);
            }

           
            vm.VM_SetDriver(driver);



            IffFile HouseFile = new IffFile();

            if (host)
            {
                //check: do we have an fsov to try loading from?

                
                string filename = Path.GetFileName(path);
                try
                {
                    using (var file = new BinaryReader(File.OpenRead(Path.Combine(FSOEnvironment.UserDir, "Houses/") + filename.Substring(0, filename.Length - 4) + ".fsov")))
                    {
                        var marshal = new SimAntics.Marshals.VMMarshal();
                        marshal.Deserialize(file);
                        vm.Load(marshal);
                        vm.Reset();
                    }
                }
                catch (Exception)
                {
                    short jobLevel = -1;

                    //quick hack to find the job level from the chosen blueprint
                    //the final server will know this from the fact that it wants to create a job lot in the first place...

                    try
                    {
                        if (filename.StartsWith("nightclub") || filename.StartsWith("restaurant") || filename.StartsWith("robotfactory"))
                            jobLevel = Convert.ToInt16(filename.Substring(filename.Length - 9, 2));
                    }
                    catch (Exception) { }

                    if (TS1)
                        HouseFile = new IffFile(path);

                    vm.SendCommand(new VMBlueprintRestoreCmd
                    {
                        JobLevel = -1,
                        XMLData = File.ReadAllBytes(path),
                        Characters = Characters,
                        HouseFile = HouseFile,
                        TS1 = TS1

                    });
                }
            }

            
            //Check the clients loaded;
            List<VMAvatar> Clients = new List<VMAvatar>();

            foreach (VMEntity entity in vm.Entities)
                if (entity is VMAvatar && entity.PersistID > 0)
                    Clients.Add((VMAvatar)entity);


            if (Clients.Count == 0)
                Permissions = true;

            uint simID = (uint)(new Random()).Next();
            vm.MyUID = simID;


            //Load clients data
            AppearanceType type;

            VMWorldActivator activator = new VMWorldActivator(vm, World);

            var headPurchasable = Content.Content.Get().AvatarPurchasables.Get(Convert.ToUInt64(gizmo.SelectedCharInfo.Head, 16));
            var bodyPurchasable = Content.Content.Get().AvatarPurchasables.Get(Convert.ToUInt64(gizmo.SelectedCharInfo.Body, 16));
            var HeadID = headPurchasable != null ? headPurchasable.OutfitID :
                Convert.ToUInt64(gizmo.SelectedCharInfo.Head, 16);
            var BodyID = bodyPurchasable != null ? bodyPurchasable.OutfitID :
                Convert.ToUInt64(gizmo.SelectedCharInfo.Body, 16);


            Enum.TryParse(gizmo.SelectedCharInfo.Appearance, out type);
            bool Male = (gizmo.SelectedCharInfo.Gender == "male") ? true : false;

            vm.SendCommand(new VMNetSimJoinCmd
            {
                ActorUID = simID,
                HeadID = HeadID,
                BodyID = BodyID,
                SkinTone = (byte)type,
                Gender = Male,
                Name = gizmo.SelectedCharInfo.Name,
                Permissions = (Permissions == true) ?
                VMTSOAvatarPermissions.Owner : VMTSOAvatarPermissions.Visitor
            });


            LotController = new UILotControl(vm, World);
            this.AddAt(0, LotController);

            vm.Context.Clock.Hours = 10;
         
            if (m_ZoomLevel > 3)
            {
                World.Visible = false;
                LotController.Visible = false;
            }
        

            if (host)
            {
                ZoomLevel = 1;
                ucp.SetInLot(true);



            } else
            {
                ZoomLevel = Math.Max(ZoomLevel, 4);
            }

            vm.OnFullRefresh += VMRefreshed;
            vm.OnChatEvent += Vm_OnChatEvent;
            vm.OnEODMessage += LotController.EODs.OnEODMessage;

        }

        private void Vm_OnChatEvent(SimAntics.NetPlay.Model.VMChatEvent evt)
        {
            if (ZoomLevel < 4)
            {
                Title.SetTitle(LotController.GetLotTitle());
            }
        }

        private void VMRefreshed()
        {
            if (vm == null) return;
            LotController.ActiveEntity = null;
            LotController.RefreshCut();
        }

        private void VMDebug_OnButtonClick(UIElement button)
        {
            
            if (vm != null) 
            {
                var debugTools = new Simantics(vm);

                var window = GameFacade.Game.Window;
                debugTools.Show();
                debugTools.Location = new System.Drawing.Point(window.ClientBounds.X + window.ClientBounds.Width, window.ClientBounds.Y);
                debugTools.UpdateAQLocation();
            }

        }

        private void CreateChar_OnButtonClick(UIElement button)
        {

            if (CityRenderer != null)
            {
                Visible = false;
                CityRenderer.Visible = false;

                GameFacade.Controller.ShowPersonCreation(new ProtocolAbstractionLibraryD.CityInfo(false));

            }

        }

        private void SaveHouseButton_OnButtonClick(UIElement button)
        {
            int houses = 0;


            DirectoryInfo HousesDir;

            if (vm == null) return;

            if (!Directory.Exists(Path.Combine(FSOEnvironment.UserDir, "Houses/")))
            {
                HousesDir = Directory.CreateDirectory(Path.Combine(FSOEnvironment.UserDir, "Houses/"));
            }

            HousesDir = new DirectoryInfo(Path.Combine(FSOEnvironment.UserDir, "Houses/"));

            foreach (FileInfo file in HousesDir.GetFiles())
                if (file.Extension == ".xml")
                    houses += 1;

            var exporter = new VMWorldExporter();

            if (lotName == "empty_lot")
                lotName = "house0" + houses;

            string housePath = Path.Combine(FSOEnvironment.UserDir, "Houses/", lotName);

            exporter.SaveHouse(vm, housePath + ".xml");
            var marshal = vm.Save();

            if (marshal != null)
            using (var output = new FileStream(Path.Combine(FSOEnvironment.UserDir, "Houses/" + lotName + ".fsov"), FileMode.Create))
                {
                    marshal.SerializeInto(new BinaryWriter(output));
                }

            Texture2D lotThumb = World.GetLotThumb(GameFacade.GraphicsDevice, null);

            if (lotThumb != null)
                using (var output = File.Open(housePath + ".png", FileMode.OpenOrCreate))
                {
                    lotThumb.SaveAsPng(output, lotThumb.Width, lotThumb.Height);
                }
         

            if (vm.GlobalLink != null) ((VMTSOGlobalLinkStub)vm.GlobalLink).Database.Save();
        }

        public void CloseInbox()
        {
            this.Remove(Inbox);
            Inbox = null;
        }

        public void OpenInbox()
        {
            if (Inbox == null)
            {
                Inbox = new UIInbox();
                this.Add(Inbox);
                Inbox.X = GlobalSettings.Default.GraphicsWidth / 2 - 332;
                Inbox.Y = GlobalSettings.Default.GraphicsHeight / 2 - 184;
            }
            //todo, on already visible move to front
        }

        private void MouseHandler(UIMouseEventType type, UpdateState state)
        {
            
            //if (CityRenderer != null) CityRenderer.UIMouseEvent(type.ToString()); //all the city renderer needs are events telling it if the mouse is over it or not.
            //if the mouse is over it, the city renderer will handle the rest.
        }
    }

    public class SimConnectStateChange
    {
        public int State;
        public float Progress;
        public SimConnectStateChange(int state, float progress)
        {
            State = state; Progress = progress;
        }
    }
}
