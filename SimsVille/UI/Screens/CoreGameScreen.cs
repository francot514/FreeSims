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
using Microsoft.Xna.Framework;
using TSOVille.Code.Utils;
using TSO.Common.rendering.framework.model;
using TSO.Common.rendering.framework.io;
using TSO.Common.rendering.framework;
using tso.world;
using tso.world.Model;
using TSO.SimsAntics;
using TSO.SimsAntics.Utils;
using TSO.SimsAntics.Primitives;
using System.IO;
using SimsHomeMaker;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.Code.UI.Controls;
using SimsVille.UI.Model;

namespace TSOVille.Code.UI.Screens
{
    public class CoreGameScreen : TSOVille.Code.UI.Framework.GameScreen
    {

        public Neighborhood Neighborhood;
        private UIButton VMDebug, SaveHouseButton;
        public UIUCP ucp;
        //public UIGizmo gizmo;
        private string[] CharacterInfos;
        public List<XmlCharacter> Characters;
        public List<VMAvatar> Avatars;
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
                            Neighborhood.Visible = false;
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

                    if (m_ZoomLevel < 4)
                    { //coming from lot view... snap zoom % to 0 or 1
                        Neighborhood.m_ZoomProgress = (value == 4) ? 1 : 0;
                        //PlayBackgroundMusic(CityMusic); //play the city music as well
                        Neighborhood.Visible = true;
                        //gizmo.Visible = true;
                        if (World != null)
                        {
                            World.Visible = false;
                            LotController.Visible = false;
                        }
                        ucp.SetMode(UIUCP.UCPMode.CityMode);
                    }
                    m_ZoomLevel = value;
                    Neighborhood.m_Zoomed = (value == 4);
                }
                ucp.UpdateZoomButton();
            }
        }

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

            SaveHouseButton = new UIButton()
            {
                Caption = "Enter House",
                Y = 45,
                Width = 100,
                X = GlobalSettings.GraphicsWidth - 110
            };
            SaveHouseButton.OnButtonClick += new ButtonClickDelegate(SaveHouseButton_OnButtonClick);
            SaveHouseButton.Visible = true;
            this.Add(SaveHouseButton);

            Avatars = new List<VMAvatar>();
            Characters = new List<XmlCharacter>();
            CharacterInfos = new string[9];

            ucp = new UIUCP(this);
            ucp.Y = ScreenHeight - 210;
            ucp.UpdateZoomButton();
            ucp.MoneyText.Caption = "";
            ucp.Visible = true;
            this.Add(ucp);

            Neighborhood = new Neighborhood(GameFacade.Game.GraphicsDevice);
            Neighborhood.LoadContent();
            
            Neighborhood.Initialize(GameFacade.HousesDataRetriever);


            Neighborhood.SetTimeOfDay(0.5);

            GameFacade.Scenes.Add(Neighborhood);

            ZoomLevel = 4; //Nhood view.

           

        }


        public override void Update(TSO.Common.rendering.framework.model.UpdateState state)
        {
            base.Update(state);


            if (ZoomLevel > 3 && Neighborhood.m_Zoomed != (ZoomLevel == 4)) ZoomLevel = (Neighborhood.m_Zoomed) ? 4 : 5;

            if (InLot) //if we're in a lot, use the VM's more accurate time!
                Neighborhood.SetTimeOfDay((vm.Context.Clock.Hours / 24.0) + (vm.Context.Clock.Minutes / 1440.0) + (vm.Context.Clock.Seconds / 86400.0));

            if (vm != null) vm.Update(state.Time);

            if (!Visible)
                Neighborhood.Visible = false;
   
        }

        public void CleanupLastWorld()
        {
            //GameFacade.Scenes.Remove(World);
            GameFacade.Scenes.Remove(Neighborhood);
            this.Remove(LotController);
            ucp.SetPanel(-1);
            ucp.SetInLot(false);

        }

        public void InitTestLot(string path)
        {
            if (vm != null) CleanupLastWorld();

            LotInfo = XmlHouseData.Parse(path);


            World = new World(GameFacade.Game.GraphicsDevice);
            GameFacade.Scenes.Add(World);

            vm = new VM(new VMContext(World));
            vm.Init();

            var activator = new VMWorldActivator(vm, World);
            var blueprint = activator.LoadFromXML(LotInfo);

            World.InitBlueprint(blueprint);
            vm.Context.Blueprint = blueprint;


            LotController = new UILotControl(vm, World, this);
            
            this.AddAt(0, LotController);

           
            ucp.vm = vm;
            ucp.SetPanel(2);
            ucp.SetInLot(true);

            if (m_ZoomLevel > 3) World.Visible = false;

            ZoomLevel = 1;

            var mailbox = vm.Entities.First(x => (x.Object.OBJ.GUID == 0xEF121974 || x.Object.OBJ.GUID == 0x1D95C9B0));
            World.State.CenterTile = new Vector2(mailbox.VisualPosition.X, mailbox.VisualPosition.Y);


            SaveHouseButton.Caption = "Save House";
            //VMDebug.Visible = true;

            var DirectoryInfo = new DirectoryInfo(GlobalSettings.DocumentsPath + "\\Characters");

            for (int i = 0; i <= DirectoryInfo.GetFiles().Count() - 1; i++)
            {

                var file = DirectoryInfo.GetFiles()[i];
                CharacterInfos[i] = file.FullName;
                Characters.Add(XmlCharacter.Parse(file.FullName));
                Avatars.Add(activator.CreateAvatar(Convert.ToUInt32(Characters[i].ObjID, 16)));
            }

            if (Characters.Count > 0)
            for (int i = 0; i <= Characters.Count - 1; i++)
            {
                //if (Characters[i].Name != gizmo.SelectedCharInfo.Name)

                short pos = Convert.ToInt16(56 + i);

                Avatars[i].SetAvatarData(Characters[i]);
                Avatars[i].Position = LotTilePos.FromBigTile(pos, 33, 1);
                VMFindLocationFor.FindLocationFor(Avatars[i], mailbox, vm.Context);

            }

        }

        private void VMDebug_OnButtonClick(UIElement button)
        {
            if (vm == null) return;

            //var debugTools = new DebugView(vm);

            var window = GameFacade.Game.Window;
            //debugTools.Show();
            //debugTools.Location = new System.Drawing.Point(window.ClientBounds.X + window.ClientBounds.Width, window.ClientBounds.Y);
            //debugTools.UpdateAQLocation();

        }


        private void SaveHouseButton_OnButtonClick(UIElement button)
        {

            

            if (InLot) 
            {
                if (vm == null) return;

            

                var exporter = new VMWorldExporter();

                string path = "Houses/" + LotInfo.Name;

                exporter.SaveHouse(vm, path + ".xml", LotInfo.Name);

                Stream file = File.Create(path + ".png");
                Texture2D thumbnail = World.GetLotThumb(GameFacade.GraphicsDevice);


                thumbnail.SaveAsPng(file, thumbnail.Width, thumbnail.Height);
                file.Close();

            }
            else
            {
                InitTestLot("Houses/house1.xml");

                ZoomLevel = 1;

            }
       }



        private void MouseHandler(UIMouseEventType type, UpdateState state)
        {

        }
    }
}
