/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.UI.Panels;
using TSOVille.Code.UI.Model;
using TSOVille.LUI;
using TSOVille.Code.Rendering.City;
using Microsoft.Xna.Framework;
using TSOVille.Code.Utils;
using TSO.Common.rendering.framework.model;
using TSO.Common.rendering.framework.io;
using TSO.Common.rendering.framework;
using TSOVille.Network;
using tso.world;
using tso.world.Model;
using TSO.SimsAntics;
using TSO.SimsAntics.Utils;
using tso.debug;
using TSO.SimsAntics.Primitives;
using TSO.HIT;
using System.IO;
using TSO.SimsAntics.NetPlay;
using TSO.SimsAntics.NetPlay.Drivers;

namespace TSOVille.Code.UI.Screens
{
    public class CoreGameScreen : TSOVille.Code.UI.Framework.GameScreen
    {
        public UIUCP ucp;
        public UIGizmo gizmo;
        public UIInbox Inbox;
        public UIGameTitle Title;
        private UIButton VMDebug, SaveHouseButton, CreateChar, CreateHouse;
        private string[] CityMusic, CharInfos;
        public List<XmlCharacter> Characters;
        public List<XmlPet> PetsInfos;
        public Terrain CityRenderer; //city view
        public List<VMAvatar> Avatars, Pets;
        public VMAvatar Sim;
        public XmlHouseData LotInfo;
        public UILotControl LotController; //world, lotcontrol and vm will be null if we aren't in a lot.
        private World World;
        public TSO.SimsAntics.VM vm;
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
                        if (m_ZoomLevel > 3)
                        {
                            PlayBackgroundMusic(new string[] { "none" }); //disable city music
                            CityRenderer.Visible = false;
                            gizmo.Visible = false;
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
                    if (CityRenderer == null) ZoomLevel = 3; //set to far zoom... again, we should eventually create this.
                    else
                    {


                        if (m_ZoomLevel < 4)
                        { //coming from lot view... snap zoom % to 0 or 1
                            CityRenderer.m_ZoomProgress = (value == 4) ? 1 : 0;
                            PlayBackgroundMusic(CityMusic); //play the city music as well
                            CityRenderer.Visible = true;
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

        public CoreGameScreen()
        {
            /** City Scene **/
            ListenForMouse(new Rectangle(0, 0, ScreenWidth, ScreenHeight), new UIMouseEvent(MouseHandler));
            Avatars = new List<VMAvatar>();
            Pets = new List<VMAvatar>();
            Characters = new List<XmlCharacter>();
            PetsInfos = new List<XmlPet>();
            CharInfos = new string[9];

            CreateCity();

            this.Add(GameFacade.MessageController);
            GameFacade.MessageController.OnSendLetter += new LetterSendDelegate(MessageController_OnSendLetter);
            GameFacade.MessageController.OnSendMessage += new MessageSendDelegate(MessageController_OnSendMessage);

            NetworkFacade.Controller.OnNewTimeOfDay += new OnNewTimeOfDayDelegate(Controller_OnNewTimeOfDay);
            NetworkFacade.Controller.OnPlayerJoined += new OnPlayerJoinedDelegate(Controller_OnPlayerJoined);


        }

        public void CreateCity()
        {

            CityRenderer = new Terrain(GameFacade.Game.GraphicsDevice); //The Terrain class implements the ThreeDAbstract interface so that it can be treated as a scene but manage its own drawing and updates.

            String city = "Queen Margaret's";
            //if (PlayerAccount.CurrentlyActiveSim.ResidingCity != null)
                //city = PlayerAccount.CurrentlyActiveSim.ResidingCity.Name;

            CityRenderer.m_GraphicsDevice = GameFacade.GraphicsDevice;

            CityRenderer.Initialize(city, GameFacade.CDataRetriever);
            CityRenderer.LoadContent(GameFacade.GraphicsDevice);
            CityRenderer.RegenData = true;

            CityRenderer.SetTimeOfDay(0.5);

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
            PlayBackgroundMusic(CityMusic);

            VMDebug = new UIButton()
            {
                Caption = "SimsAntics",
                Y = 45,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            VMDebug.OnButtonClick += new ButtonClickDelegate(VMDebug_OnButtonClick);
            VMDebug.Visible = false;
            this.Add(VMDebug);

            SaveHouseButton = new UIButton()
            {
                Caption = "Save House",
                Y = 105,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            SaveHouseButton.OnButtonClick += new ButtonClickDelegate(SaveHouseButton_OnButtonClick);
            SaveHouseButton.Visible = false;
            this.Add(SaveHouseButton);

            CreateChar = new UIButton()
            {
                Caption = "Create Sim",
                Y = 105,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            CreateChar.OnButtonClick += new ButtonClickDelegate(CreateChar_OnButtonClick);
            CreateChar.Visible = true;
            this.Add(CreateChar);

            CreateHouse = new UIButton()
            {
                Caption = "Create House",
                Y = 165,
                Width = 100,
                X = GlobalSettings.Default.GraphicsWidth - 110
            };
            CreateHouse.OnButtonClick += new ButtonClickDelegate(CreateHouse_OnButtonClick);
            CreateHouse.Visible = true;
            this.Add(CreateHouse);

            ucp = new UIUCP(this);
            ucp.Y = ScreenHeight - 210;
            ucp.SetInLot(false);
            ucp.UpdateZoomButton();
            ucp.MoneyText.Caption = PlayerAccount.Money.ToString();
            ucp.Visible = true;
            this.Add(ucp);

            gizmo = new UIGizmo();
            gizmo.X = ScreenWidth - 500;
            gizmo.Y = ScreenHeight - 300;
            gizmo.Visible = true;
            this.Add(gizmo);

            Title = new UIGameTitle();
            Title.SetTitle(city);
            this.Add(Title);

            GameFacade.Scenes.Add(CityRenderer);

            ZoomLevel = 5; //screen always starts at far zoom, city visible.

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
            LotTileEntry[] TileEntries = new LotTileEntry[GameFacade.CDataRetriever.LotTileData.Length + 1];
            TileEntries[0] = TileEntry;
            GameFacade.CDataRetriever.LotTileData.CopyTo(TileEntries, 1);
            CityRenderer.populateCityLookup(TileEntries);
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

        public override void Update(TSO.Common.rendering.framework.model.UpdateState state)
        {
            base.Update(state);
            
            if (ZoomLevel > 3 && CityRenderer.m_Zoomed != (ZoomLevel == 4)) ZoomLevel = (CityRenderer.m_Zoomed) ? 4 : 5;

            if (InLot) //if we're in a lot, use the VM's more accurate time!
                CityRenderer.SetTimeOfDay((vm.Context.Clock.Hours / 24.0) + (vm.Context.Clock.Minutes / 1440.0) + (vm.Context.Clock.Seconds / 86400.0));

            if (vm != null) vm.Update(state.Time);

            if (!Visible)
                CityRenderer.Visible = false;
        }

        public void CleanupLastWorld()
        {
            vm.Context.Ambience.Kill();
            GameFacade.Scenes.Remove(World);
            this.Remove(LotController);
            ucp.SetPanel(-1);
            ucp.SetInLot(false);
            Characters.Clear();
            Avatars.Clear();
        }

        public void InitTestLot(string path)
        {


            LotInfo = XmlHouseData.Parse(path);


            if (vm != null) CleanupLastWorld();

            World = new World(GameFacade.Game.GraphicsDevice);
            GameFacade.Scenes.Add(World);

            vm = new VM(new VMContext(World), new UIHeadlineRendererProvider());
            vm.Init();

            var activator = new VMWorldActivator(vm, World);
            var blueprint = activator.LoadFromXML(LotInfo);

            World.InitBlueprint(blueprint);
            vm.Context.Blueprint = blueprint;

            var mailbox = vm.Entities.First(x => (x.Object.OBJ.GUID == 0xEF121974 || x.Object.OBJ.GUID == 0x1D95C9B0));

            if (gizmo.SelectedCharInfo != null)
            {

            var DirectoryInfo = new DirectoryInfo(GlobalSettings.Default.DocumentsPath + "//Characters");

            for (int i = 0; i <= DirectoryInfo.GetFiles().Count() - 1; i++)
            {

                var file = DirectoryInfo.GetFiles()[i];
                CharInfos[i] = file.FullName;
                Characters.Add(XmlCharacter.Parse(file.FullName));
                Avatars.Add(activator.CreateAvatar(Convert.ToUInt32(Characters[i].ObjID, 16)));
            }

            
            for (int i = 0; i <= Characters.Count - 1; i++)
                {
                //if (Characters[i].Name != gizmo.SelectedCharInfo.Name)
                 
                     short pos = Convert.ToInt16(56 + i);
                     
                     Avatars[i].SetAvatarData(Characters[i]);
                     Avatars[i].Position = LotTilePos.FromBigTile(pos, 33, 1);
                     VMFindLocationFor.FindLocationFor(Avatars[i], mailbox, vm.Context);
                     
                 }

            for (int i = 0; i <= Avatars.Count - 1; i++)
            
                if (Avatars[i].Name == gizmo.SelectedCharInfo.Name)
                {
                short pos = Convert.ToInt16(56 + i);

                Sim = Avatars[i];
                
                Sim.SetAvatarData(gizmo.SelectedCharInfo);
                Sim.Position = LotTilePos.FromBigTile(pos, 33, 1);
                ucp.SelectedAvatar = Sim;

                 }

            ucp.vm = vm;
            ucp.Avatars = Avatars;

            VMFindLocationFor.FindLocationFor(Sim, mailbox, vm.Context);

            DirectoryInfo = new DirectoryInfo(GlobalSettings.Default.DocumentsPath + "//Pets");

            for (int i = 0; i <= DirectoryInfo.GetFiles().Count() - 1; i++)
            {
                
                
                var file = DirectoryInfo.GetFiles()[i];
                if (file.Extension == ".xml")
                    {
                PetsInfos.Add(XmlPet.Parse(file.FullName));
                Pets.Add(activator.CreateAvatar(Convert.ToUInt32(PetsInfos[i].ObjID, 16)));

                    }
            }

            for (int i = 0; i <= PetsInfos.Count - 1; i++)
            {
               

                short pos = Convert.ToInt16(60 + i);

                Pets[i].SetPetData(PetsInfos[i]);
                Pets[i].Position = LotTilePos.FromBigTile(pos, 40, 1);
                
                VMFindLocationFor.FindLocationFor(Pets[i], mailbox, vm.Context);

            }


            }


            

            HITVM.Get().PlaySoundEvent("lot_enter");

            if (Sim != null)
                World.State.CenterTile = new Vector2(Sim.VisualPosition.X, Sim.VisualPosition.Y);
            else
                World.State.CenterTile = new Vector2(mailbox.VisualPosition.X, mailbox.VisualPosition.Y);

            LotController = new UILotControl(vm, World, this);
            
            if (gizmo.SelectedCharInfo != null)
            {
            short id = Convert.ToInt16(gizmo.SelectedCharInfo.Id);
            LotController.SelectedSimID = id;

            }
            this.AddAt(0, LotController);

            
            ucp.SetInLot(true);
            if (m_ZoomLevel > 3) World.Visible = false;

            ZoomLevel = 1;

            CreateChar.Visible = false;
            CreateHouse.Visible = false;
            SaveHouseButton.Visible = true;
            VMDebug.Visible = true;


        }

        private void VMDebug_OnButtonClick(UIElement button)
        {
            if (vm == null) return;

            var debugTools = new DebugView(vm);

            var window = GameFacade.Game.Window;
            debugTools.Show();
            debugTools.Location = new System.Drawing.Point(window.ClientBounds.X + window.ClientBounds.Width, window.ClientBounds.Y);
            debugTools.UpdateAQLocation();

        }


        private void CreateHouse_OnButtonClick(UIElement button)
        {
            if (CityRenderer != null)
            {
                Visible = false;
                CityRenderer.Visible = false;

                GameFacade.Controller.ShowHouseCreation(this);

            }
            

        }

        private void CreateChar_OnButtonClick(UIElement button)
        {

            if (CityRenderer != null)
                 {
                     Visible = false;
            CityRenderer.Visible = false;
                
            GameFacade.Controller.ShowPersonCreation(this);

                 }

        }

        private void SaveHouseButton_OnButtonClick(UIElement button)
        {
            if (vm == null) return;


            var exporter = new VMWorldExporter();
            exporter.SaveHouse(vm, GlobalSettings.Default.DocumentsPath + "Houses//" + LotInfo.Name + ".xml", LotInfo.Name);

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
            //todo: change handler to game engine when in simulation mode.

            CityRenderer.UIMouseEvent(type.ToString()); //all the city renderer needs are events telling it if the mouse is over it or not.
            //if the mouse is over it, the city renderer will handle the rest.
        }
    }
}
