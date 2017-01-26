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

namespace TSOVille.Code.UI.Screens
{
    public class CoreGameScreen : TSOVille.Code.UI.Framework.GameScreen
    {

        private UIButton VMDebug, SaveHouseButton;
        public UIUCP ucp;
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

            VMDebug = new UIButton()
            {
                Caption = "SimsAntics",
                Y = 45,
                Width = 100,
                X = GlobalSettings.GraphicsWidth - 110
            };
            VMDebug.OnButtonClick += new ButtonClickDelegate(VMDebug_OnButtonClick);
            VMDebug.Visible = false;
            this.Add(VMDebug);

            SaveHouseButton = new UIButton()
            {
                Caption = "Save House",
                Y = 105,
                Width = 100,
                X = GlobalSettings.GraphicsWidth - 110
            };
            SaveHouseButton.OnButtonClick += new ButtonClickDelegate(SaveHouseButton_OnButtonClick);
            SaveHouseButton.Visible = false;
            this.Add(SaveHouseButton);



            ucp = new UIUCP(this);
            ucp.Y = ScreenHeight - 210;
            ucp.UpdateZoomButton();
            ucp.MoneyText.Caption = "";
            ucp.Visible = true;
            this.Add(ucp);


            ZoomLevel = 1; //Lot view.

        }


        public override void Update(TSO.Common.rendering.framework.model.UpdateState state)
        {
            base.Update(state);


            if (vm != null) vm.Update(state.Time);

            
        }

        public void CleanupLastWorld()
        {
            //GameFacade.Scenes.Remove(World);
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


            SaveHouseButton.Visible = true;
            VMDebug.Visible = true;


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
            if (vm == null) return;

            string path = "Houses/" + LotInfo.Name;
            Stream file = File.Create(path + ".png");
            Texture2D thumbnail = World.GetLotThumb(GameFacade.GraphicsDevice);

            var exporter = new VMWorldExporter();
            exporter.SaveHouse(vm, GlobalSettings.DocumentsPath + "Houses//" + LotInfo.Name + ".xml", LotInfo.Name);

            thumbnail.SaveAsPng(file, thumbnail.Width, thumbnail.Height);
            file.Close();

        }



        private void MouseHandler(UIMouseEventType type, UpdateState state)
        {
            //todo: change handler to game engine when in simulation mode.

           // CityRenderer.UIMouseEvent(type.ToString()); //all the city renderer needs are events telling it if the mouse is over it or not.
            //if the mouse is over it, the city renderer will handle the rest.
        }
    }
}
