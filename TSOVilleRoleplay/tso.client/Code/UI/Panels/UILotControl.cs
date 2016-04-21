/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
RHY3756547. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.UI.Panels;
using TSOVille.Code.UI.Controls;
using TSOVille.Code.UI.Model;
using TSOVille.Code.Rendering.City;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.Code.Utils;
using TSO.Common.rendering.framework.model;
using TSO.Common.rendering.framework.io;
using TSO.Common.rendering.framework;
using TSO.Files.formats.iff.chunks;
using TSO.HIT;

using tso.world;
using TSO.SimsAntics;
using tso.world.Components;
using TSOVille.Code.UI.Panels.LotControls;
using Microsoft.Xna.Framework.Input;
using tso.world.Model;
using TSOVille.Code.UI.Screens;
using TSO.SimsAntics.Model;

namespace TSOVille.Code.UI.Panels
{
    /// <summary>
    /// Generates pie menus when the player clicks on objects.
    /// </summary>
    public class UILotControl : UIContainer
    {
        private UIMouseEventRef MouseEvt;
        private bool MouseIsOn;
        private UIPieMenu PieMenu;
        private bool ShowTooltip;
        private bool TipIsError;
        public TSO.SimsAntics.VM vm;
        public World World;
        public short ObjectHover;
        public bool InteractionsAvailable;
        public UIImage testimg;
        public UIInteractionQueue Queue;
        public short SelectedSimID;
        public bool LiveMode = true;
        public UIObjectHolder ObjectHolder;
        public UIQueryPanel QueryPanel;
        public UICustomLotControl CustomControl;

        public int WallsMode;

        private int OldMX;
        private int OldMY;
        private CoreGameScreen Main;
        private bool RMBScroll;
        private int RMBScrollX;
        private int RMBScrollY;
        private bool TabLastPressed;
        private bool CtrlLastPressed;
        private List<VMPieMenuInteraction> Menu = new List<VMPieMenuInteraction>();

        private static uint GOTO_GUID = 0x000007C4;
        public VMEntity GotoObject;

        private Rectangle MouseCutRect = new Rectangle(-4, -4, 4, 4);

        /// <summary>
        /// Creates a new UILotControl instance.
        /// </summary>
        /// <param name="vm">A SimsAntics VM instance.</param>
        /// <param name="World">A World instance.</param>
        public UILotControl(TSO.SimsAntics.VM vm, World World, CoreGameScreen main)
        {
            this.vm = vm;
            this.World = World;
            Main = main;

            vm.ActiveEntity = vm.Entities.FirstOrDefault(x => x is VMAvatar && x.PersistID == SelectedSimID);
            
            
            MouseEvt = this.ListenForMouse(new Microsoft.Xna.Framework.Rectangle(0, 0, 
                GlobalSettings.Default.GraphicsWidth, GlobalSettings.Default.GraphicsHeight), OnMouse);
            testimg = new UIImage();
            testimg.X = 20;
            testimg.Y = 20;
            this.Add(testimg);
            
            Queue = new UIInteractionQueue(vm.ActiveEntity);
            this.Add(Queue);

            ObjectHolder = new UIObjectHolder(vm, World, this);
            QueryPanel = new UIQueryPanel(World);
            QueryPanel.OnSellBackClicked += ObjectHolder.SellBack;
            QueryPanel.X = 177;
            QueryPanel.Y = GlobalSettings.Default.GraphicsHeight - 228;
            this.Add(QueryPanel);


            vm.OnDialog += vm_OnDialog;
        }

        void vm_OnDialog(VMDialogInfo info)
        {
            var alert = UIScreen.ShowAlert(new UIAlertOptions { Title = info.Title, Message = info.Message, Width = 325+(int)(info.Message.Length/3.5f), Alignment = TextAlignment.Left, TextSize = 12 }, true);
            var entity = info.Icon;
            if (entity is VMGameObject)
            {
                var objects = entity.MultitileGroup.Objects;
                ObjectComponent[] objComps = new ObjectComponent[objects.Count];
                for (int i = 0; i < objects.Count; i++)
                {
                    objComps[i] = (ObjectComponent)objects[i].WorldUI;
                }
                var thumb = World.GetObjectThumb(objComps, entity.MultitileGroup.GetBasePositions(), GameFacade.GraphicsDevice);
                alert.SetIcon(thumb, 110, 110);
            }
        }


        private void OnMouse(UIMouseEventType type, UpdateState state)
        {


            if (type == UIMouseEventType.MouseOver)
            {
                if (QueryPanel.Mode == 1) QueryPanel.Active = false;
                MouseIsOn = true;
            }
            else if (type == UIMouseEventType.MouseOut)
            {
                MouseIsOn = false;
                Tooltip = null;
            }
            else if (type == UIMouseEventType.MouseDown)
            {
                if (!LiveMode)
                {
                    if (CustomControl != null) CustomControl.MouseDown(state);
                    else ObjectHolder.MouseDown(state);
                    return;
                }
                else
                if (PieMenu == null)
                {
                    VMEntity obj;
                    //get new pie menu, make new pie menu panel for it
                    var tilePos = World.State.WorldSpace.GetTileAtPosWithScroll(new Vector2(state.MouseState.X, state.MouseState.Y));
                    
                    LotTilePos targetPos = LotTilePos.FromBigTile((short)tilePos.X, (short)tilePos.Y, World.State.Level);
                    if (vm.Context.SolidToAvatars(targetPos).Solid) targetPos = LotTilePos.OUT_OF_WORLD;

                    
                    GotoObject.SetPosition(targetPos, Direction.NORTH, vm.Context);

                    bool objSelected = ObjectHover > 0 && InteractionsAvailable;
                    if (objSelected || GotoObject.Position != LotTilePos.OUT_OF_WORLD)
                    {
                        HITVM.Get().PlaySoundEvent(UISounds.PieMenuAppear);
                        if (objSelected)
                        {
                            obj = vm.GetObjectById(ObjectHover);
                        }
                        else
                        {
                            obj = GotoObject;
                        }

                        if (vm.ActiveEntity != null)
                        if (vm.ActiveEntity.Object.GUID == 0x5F0C674C)
                            Menu = obj.GetPieMenu(vm, vm.ActiveEntity, true);

                        else
                            Menu = obj.GetPieMenu(vm, vm.ActiveEntity, false);
                        if (Menu.Count != 0)
                        {
                            PieMenu = new UIPieMenu(Menu, obj, vm.ActiveEntity, this);
                            this.Add(PieMenu);
                            PieMenu.X = state.MouseState.X;
                            PieMenu.Y = state.MouseState.Y;
                            PieMenu.UpdateHeadPosition(state.MouseState.X, state.MouseState.Y);
                        }
                    }
                    else
                    {
                        HITVM.Get().PlaySoundEvent(UISounds.Error);
                        GameFacade.Screens.TooltipProperties.Show = true;
                        GameFacade.Screens.TooltipProperties.Opacity = 1;
                        GameFacade.Screens.TooltipProperties.Position = new Vector2(state.MouseState.X,
                            state.MouseState.Y);
                        GameFacade.Screens.Tooltip = GameFacade.Strings.GetString("159", "0");
                        GameFacade.Screens.TooltipProperties.UpdateDead = false;
                        ShowTooltip = true;
                        TipIsError = true;
                    }
                }
                else
                {
                    PieMenu.RemoveSimScene();
                    this.Remove(PieMenu);
                    PieMenu = null;
                }
            }
            else if (type == UIMouseEventType.MouseUp)
            {
                if (!LiveMode)
                {
                    if (CustomControl != null) CustomControl.MouseUp(state);
                    else ObjectHolder.MouseUp(state);
                    return;
                }
                GameFacade.Screens.TooltipProperties.Show = false;
                GameFacade.Screens.TooltipProperties.Opacity = 0;
                ShowTooltip = false;
                TipIsError = false;
            }
        }

        public void ClosePie() 
        {
            if (PieMenu != null) 
            {
                PieMenu.RemoveSimScene();
                Queue.PieMenuClickPos = PieMenu.Position;
                this.Remove(PieMenu);
                PieMenu = null;
            }
        }

        public override Rectangle GetBounds()
        {
            return new Rectangle(0, 0, GlobalSettings.Default.GraphicsWidth, GlobalSettings.Default.GraphicsHeight);
        }

        public void LiveModeUpdate(UpdateState state, bool scrolled)
        {
            if (vm.ActiveEntity == null || vm.ActiveEntity.Dead)
                vm.ActiveEntity = vm.Entities.FirstOrDefault(x => x is VMAvatar && x.PersistID == SelectedSimID); //try and hook onto a sim if we have none selected.



            if (vm.ActiveEntity != null)
            {
                //vm.Context.World.State.CenterTile = new Vector2(vm.ActiveEntity.VisualPosition.X, vm.ActiveEntity.VisualPosition.Y);

                Queue.QueueOwner = vm.ActiveEntity;

            }

            if (MouseIsOn && vm.ActiveEntity != null)
            {

                if (state.MouseState.X != OldMX || state.MouseState.Y != OldMY)
                {
                    OldMX = state.MouseState.X;
                    OldMY = state.MouseState.Y;
                    var newHover = World.GetObjectIDAtScreenPos(state.MouseState.X, state.MouseState.Y, GameFacade.GraphicsDevice);
                    //if (newHover == 0) newHover = vm.ActiveEntity.ObjectID;


                    if (ObjectHover != newHover)
                    {
                        ObjectHover = newHover;
                        if (ObjectHover > 0)
                        {
                            var obj = vm.GetObjectById(ObjectHover);

                            if (obj == null) obj = vm.GetObjectById(newHover);

                            if (vm.ActiveEntity.Object.GUID == 0x5F0C674C)
                                Menu = obj.GetPieMenu(vm, vm.ActiveEntity, true);
                                
                            else
                                Menu = obj.GetPieMenu(vm, vm.ActiveEntity, false);
                            InteractionsAvailable = (Menu.Count > 0);
                        }
                    }

                    if (!TipIsError) ShowTooltip = false;
                    if (ObjectHover > 0)
                    {
                        var obj = vm.GetObjectById(ObjectHover);
                        if (obj is VMAvatar && !TipIsError)
                        {
                            GameFacade.Screens.TooltipProperties.Show = true;
                            GameFacade.Screens.TooltipProperties.Opacity = 1;
                            GameFacade.Screens.TooltipProperties.Position = new Vector2(state.MouseState.X,
                                state.MouseState.Y);
                            GameFacade.Screens.Tooltip = obj.ToString();
                            GameFacade.Screens.TooltipProperties.UpdateDead = false;
                            ShowTooltip = true;
                        }
                    }
                    if (!ShowTooltip)
                    {
                        GameFacade.Screens.TooltipProperties.Show = false;
                        GameFacade.Screens.TooltipProperties.Opacity = 0;
                    }
                }
            }
            else
            {
                ObjectHover = 0;
            }

            if (!scrolled)
            { //set cursor depending on interaction availability
                CursorType cursor;
                if (ObjectHover == 0)
                {
                    cursor = CursorType.LiveNothing;
                }
                else
                {
                    if (InteractionsAvailable)
                    {
                        if (vm.GetObjectById(ObjectHover) is VMAvatar) cursor = CursorType.LivePerson;
                        else cursor = CursorType.LiveObjectAvail;
                    }
                    else
                    {
                        cursor = CursorType.LiveObjectUnavail;
                    }
                }

                CursorManager.INSTANCE.SetCursor(cursor);
            }

        }

        public override void Update(UpdateState state)
        {
            base.Update(state);

            

            if (GotoObject == null) 
            GotoObject = vm.Context.CreateObjectInstance(GOTO_GUID, LotTilePos.OUT_OF_WORLD, Direction.NORTH, true).Objects[0];


            if (state.KeyboardState.IsKeyDown(Keys.LeftControl))
            {
                if (!CtrlLastPressed)
                {
                    //switch active Sim

                    vm.ActiveEntity = vm.Entities.FirstOrDefault(x => (x is VMAvatar && x.ObjectID == ObjectHover));
                    if (vm.ActiveEntity == null) vm.ActiveEntity = vm.Entities.FirstOrDefault(x => (x is VMAvatar));
                    HITVM.Get().PlaySoundEvent(UISounds.Speed1To2);
                    Queue.QueueOwner = vm.ActiveEntity;
                    Main.ucp.SelectedAvatar = (VMAvatar)vm.ActiveEntity;
                    CtrlLastPressed = true;
                }

            }
            else CtrlLastPressed = false;

            if (state.KeyboardState.IsKeyDown(Keys.Tab))
            {
                if (!TabLastPressed)
                {
                    //switch active Sim

                    vm.ActiveEntity = vm.Entities.FirstOrDefault(x => (x is VMAvatar && x.ObjectID > vm.ActiveEntity.ObjectID));
                    if (vm.ActiveEntity == null) vm.ActiveEntity = vm.Entities.FirstOrDefault(x => (x is VMAvatar ));
                    HITVM.Get().PlaySoundEvent(UISounds.Speed1To2);
                    Queue.QueueOwner = vm.ActiveEntity;
                    Main.ucp.SelectedAvatar = (VMAvatar)vm.ActiveEntity;
                    TabLastPressed = true;
                }
                
            } else TabLastPressed = false;

            if (state.KeyboardState.IsKeyDown(Keys.P) || state.KeyboardState.IsKeyDown(Keys.D0))
                vm.Ready = !vm.Ready;

            if (state.KeyboardState.IsKeyDown(Keys.D1))
                vm.Speed = 3;

            if (state.KeyboardState.IsKeyDown(Keys.D2))
                vm.Speed = 2;

            if (state.KeyboardState.IsKeyDown(Keys.D3))
                vm.Speed = 1;

            if (Visible)
            {
                if (ShowTooltip) GameFacade.Screens.TooltipProperties.UpdateDead = false;

                bool scrolled = false;
                if (RMBScroll)
                {
                    Vector2 scrollBy = new Vector2();
                    if (state.TouchMode)
                    {
                        scrollBy = new Vector2(RMBScrollX - state.MouseState.X, RMBScrollY - state.MouseState.Y);
                        RMBScrollX = state.MouseState.X;
                        RMBScrollY = state.MouseState.Y;
                        scrollBy /= 128f;
                    } else
                    {
                        scrollBy = new Vector2(state.MouseState.X - RMBScrollX, state.MouseState.Y - RMBScrollY);
                        scrollBy *= 0.0005f;
                    }
                    World.Scroll(scrollBy);
                    scrolled = true;
                }
                if (MouseIsOn)
                {
                    if (state.MouseState.RightButton == ButtonState.Pressed)
                    {
                        if (RMBScroll == false)
                        {
                            RMBScroll = true;
                            RMBScrollX = state.MouseState.X;
                            RMBScrollY = state.MouseState.Y;
                        }
                    }
                    else
                    {
                        RMBScroll = false;
                        if (!scrolled && GlobalSettings.Default.EdgeScroll && !state.TouchMode) scrolled = World.TestScroll(state);
                    }

                }

                if (LiveMode) LiveModeUpdate(state, scrolled);
                else if (CustomControl != null) CustomControl.Update(state, scrolled);
                else ObjectHolder.Update(state, scrolled);

                //set cutaway around mouse
                if (vm.Context.Blueprint != null)
                {

                var cuts = vm.Context.Blueprint.Cutaway;
                Rectangle newCut;
                if (WallsMode == 0){
                    newCut = new Rectangle(-1, -1, 1024, 1024); //cut all; walls down.
                }
                else if (WallsMode == 1)
                {
                    var mouseTilePos = World.State.WorldSpace.GetTileAtPosWithScroll(new Vector2(state.MouseState.X, state.MouseState.Y + 128));
                    newCut = new Rectangle((int)(mouseTilePos.X - 5.5), (int)(mouseTilePos.Y - 5.5), 11, 11);
                }
                else
                {
                    newCut = new Rectangle(0, 0, 0, 0); //walls up or roof
                }


                if (!newCut.Equals(MouseCutRect)) {
                    if (cuts.Contains(MouseCutRect)) cuts.Remove(MouseCutRect);
                    MouseCutRect = newCut;
                    cuts.Add(MouseCutRect);
                    vm.Context.Blueprint.Damage.Add(new tso.world.Model.BlueprintDamage(tso.world.Model.BlueprintDamageType.WALL_CUT_CHANGED));
                }

                }
            }
        }
    }
}
