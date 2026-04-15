/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.LotView;
using FSO.SimAntics;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using FSO.LotView.Components;
using FSO.SimAntics.Entities;
using FSO.LotView.Model;
using FSO.Client.UI.Model;
using TSO.HIT;
using FSO.SimAntics.Model;
using Microsoft.Xna.Framework.Input;
using FSO.Client.UI.Framework;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.Common;
using FSO.SimAntics.Model.Platform;
using FSO.Client.UI.Controls;

namespace FSO.Client.UI.Panels
{
    public class UIObjectHolder //controls the object holder interface
    {
        public VM vm;
        public LotView.World World;
        public UILotControl ParentControl;

        public Direction Rotation;
        public int MouseDownX;
        public int MouseDownY;
        private bool MouseIsDown;
        private bool MouseWasDown;
        private bool MouseClicked;

        private UpdateState LastState;

        private int OldMX;
        private int OldMY;
        public bool DirChanged;
        public bool ShowTooltip;

        public event HolderEventHandler OnPickup;
        public event HolderEventHandler OnDelete;
        public event HolderEventHandler OnPutDown;

        public UIObjectSelection Holding;

        public UIObjectHolder(VM vm, LotView.World World, UILotControl parent)
        {
            this.vm = vm;
            this.World = World;
            ParentControl = parent;
        }

        public void SetSelected(VMMultitileGroup Group)
        {
            if (Holding != null) ClearSelected();
            Holding = new UIObjectSelection();
            Holding.Group = Group;
            Holding.PreviousTile = Holding.Group.BaseObject.Position;
            Holding.Dir = Group.Objects[0].Direction;
            VMEntity[] CursorTiles = new VMEntity[Group.Objects.Count];
            for (int i = 0; i < Group.Objects.Count; i++)
            {
                var target = Group.Objects[i];
                target.SetRoom(65534);
                if (target is VMGameObject) ((ObjectComponent)target.WorldUI).ForceDynamic = true;
                CursorTiles[i] = vm.Context.CreateObjectInstance(0x00000437, new LotTilePos(target.Position), FSO.LotView.Model.Direction.NORTH, true).Objects[0];
                CursorTiles[i].SetPosition(new LotTilePos(0,0,1), Direction.NORTH, vm.Context);
                ((ObjectComponent)CursorTiles[i].WorldUI).ForceDynamic = true;
            }
            Holding.TilePosOffset = new Vector2(0, 0);
            Holding.CursorTiles = CursorTiles;

            uint guid;
            var bobj = Group.BaseObject;
            guid = bobj.Object.OBJ.GUID;
            if (bobj.MasterDefinition != null) guid = bobj.MasterDefinition.GUID;
            var catalogItem = Content.Content.Get().WorldCatalog.GetItemByGUID(guid);
            if (catalogItem != null) Holding.Price = (int)catalogItem.Price;
        }


        private void InventoryPlaceHolding()
        {
            var pos = Holding.Group.BaseObject.Position;
            vm.SendCommand(new VMNetPlaceInventoryCmd
            {
                ObjectPID = Holding.InventoryPID,
                dir = Holding.Dir,
                level = pos.Level,
                x = pos.x,
                y = pos.y,

                Mode = PurchaseMode.Normal
            });
        }

        public void MoveToInventory(UIElement button)
        {
            if (Holding == null) return;
            if (Holding.IsBought)
            {
                if (Holding.CanDelete)
                {
                    var obj = vm.GetObjectById(Holding.MoveTarget);
                    if (obj != null)
                    {
                        vm.SendCommand(new VMNetSendToInventoryCmd
                        {
                            ObjectPID = obj.PersistID,
                        });
                    }
                }
                else
                {
                    ShowErrorAtMouse(LastState, Holding.CanPlace);
                    return;
                }
            }
            OnDelete(Holding, null); //TODO: cleanup callbacks which don't need updatestate into another delegate.
            ClearSelected();
        }

        private void ShowErrorAtMouse(UpdateState state, VMPlacementError error)
        {
            state.UIState.TooltipProperties.Show = true;
            state.UIState.TooltipProperties.Color = Color.Black;
            state.UIState.TooltipProperties.Opacity = 1;
            state.UIState.TooltipProperties.Position = new Vector2(MouseDownX,
                MouseDownY);
            state.UIState.Tooltip = GameFacade.Strings.GetString("137", "kPErr" + error.ToString());
            state.UIState.TooltipProperties.UpdateDead = false;
            ShowTooltip = true;
            HITVM.Get().PlaySoundEvent(UISounds.Error);
        }

        public void MoveSelected(Vector2 pos, sbyte level)
        {
            Holding.TilePos = pos;
            Holding.Level = level;

            //first, eject the object from any slots
            for (int i = 0; i < Holding.Group.Objects.Count; i++)
            {
                var obj = Holding.Group.Objects[i];
                if (obj.Container != null)
                {
                    obj.Container.ClearSlot(obj.ContainerSlot);
                }
            }

            //rotate through to try all configurations
            var dir = Holding.Dir;
            VMPlacementError status = VMPlacementError.Success;
            for (int i = 0; i < 4; i++)
            {
                status = Holding.Group.ChangePosition(LotTilePos.FromBigTile((short)pos.X, (short)pos.Y, World.State.Level), dir, vm.Context).Status;
                if (status != VMPlacementError.MustBeAgainstWall) break;
                dir = (Direction)((((int)dir << 6) & 255) | ((int)dir >> 2));
            }
            if (Holding.Dir != dir) Holding.Dir = dir;

            if (status != VMPlacementError.Success) 
            {
                Holding.Group.ChangePosition(LotTilePos.OUT_OF_WORLD, Holding.Dir, vm.Context);

                Holding.Group.SetVisualPosition(new Vector3(pos,
                (((Holding.Group.Objects[0].GetValue(VMStackObjectVariable.AllowedHeightFlags) & 1) == 1) ? 0 : 4f / 5f) + (World.State.Level-1)*2.95f),
                    //^ if we can't be placed on the floor, default to table height.
                Holding.Dir, vm.Context);
            }

            for (int i = 0; i < Holding.Group.Objects.Count; i++)
            {
                var target = Holding.Group.Objects[i];
                var tpos = target.VisualPosition;
                tpos.Z = (World.State.Level - 1)*2.95f;
                Holding.CursorTiles[i].MultitileGroup.SetVisualPosition(tpos, Holding.Dir, vm.Context);
            }
            Holding.CanPlace = status;
        }

        public void ClearSelected()
        {
            //TODO: selected items are only spooky ghosts of the items themselves.
            //      ...so that they dont cause serverside desyncs
            //      and so that clearing selections doesnt delete already placed objects.
            if (Holding != null)
            {
                Holding.Group.Delete(vm.Context);

                for (int i = 0; i < Holding.CursorTiles.Length; i++) {
                    Holding.CursorTiles[i].Delete(true, vm.Context);
                    ((ObjectComponent)Holding.CursorTiles[i].WorldUI).ForceDynamic = false;
                }
            }
            Holding = null;
        }

        public void MouseDown(UpdateState state)
        {
            MouseIsDown = true;
            MouseDownX = state.MouseState.X;
            MouseDownY = state.MouseState.Y;
            if (Holding != null)
            {
                Rotation = Holding.Dir;
                DirChanged = false;
            }
        }

        public void MouseUp(UpdateState state)
        {
            MouseIsDown = false;
            if (Holding != null && Holding.Clicked)
            {
                if (Holding.CanPlace == VMPlacementError.Success)
                {
                    HITVM.Get().PlaySoundEvent((Holding.IsBought) ? UISounds.ObjectMovePlace : UISounds.ObjectPlace);
                    //ExecuteEntryPoint(11); //User Placement
                    var putDown = Holding;
                    var badCategory = ((Holding.Group.BaseObject as VMGameObject)?.Disabled ?? 0).HasFlag(VMGameObjectDisableFlags.LotCategoryWrong);

                    if (Holding.IsBought)
                    {
                        var pos = Holding.Group.BaseObject.Position;
                        vm.SendCommand(new VMNetMoveObjectCmd
                        {
                            ObjectID = Holding.MoveTarget,
                            dir = Holding.Dir,
                            level = pos.Level,
                            x = pos.x,
                            y = pos.y
                        });
                    } else
                    {

                        if (badCategory)
                        {
                            
                           
                            HITVM.Get().PlaySoundEvent(UISounds.ObjectPlace);
                            if (Holding.InventoryPID > 0) InventoryPlaceHolding();
                            else BuyHolding();
                            ClearSelected();
                            if (OnPutDown != null) OnPutDown(putDown, state); //call this after so that buy mode etc can produce more.
                               
                            return;
                        }
                        else
                        {
                            HITVM.Get().PlaySoundEvent(UISounds.ObjectPlace);
                            if (Holding.InventoryPID > 0) InventoryPlaceHolding();
                            else BuyHolding();
                        }

                        
                    }
                    ClearSelected();
                    if (OnPutDown != null) OnPutDown(putDown, state); //call this after so that buy mode etc can produce more.
                }
                else
                {
                    
                }
            }

            state.UIState.TooltipProperties.Show = false;
            state.UIState.TooltipProperties.Opacity = 0;
            ShowTooltip = false;
        }

        private void BuyHolding()
        {
            var pos = Holding.Group.BaseObject.Position;
            var GUID = (Holding.Group.MultiTile) ? Holding.Group.BaseObject.MasterDefinition.GUID : Holding.Group.BaseObject.Object.OBJ.GUID;
            vm.SendCommand(new VMNetBuyObjectCmd
                        {
                            GUID = GUID,
                            dir = Holding.Dir,
                            level = pos.Level,
                            x = pos.x,
                            y = pos.y
                        });

        }

        private void ExecuteEntryPoint(int num)
        {
            for (int i = 0; i < Holding.Group.Objects.Count; i++) Holding.Group.Objects[i].ExecuteEntryPoint(num, vm.Context, true); 
        }

        public void SellBack(UIElement button)
        {
            if (Holding == null) return;
            if (Holding.IsBought)
            {
                vm.SendCommand(new VMNetDeleteObjectCmd
                {
                    ObjectID = Holding.MoveTarget,
                    CleanupAll = true
                });
                HITVM.Get().PlaySoundEvent(UISounds.MoneyBack);
            }
            OnDelete(Holding, null); //TODO: cleanup callbacks which don't need updatestate into another delegate. will do when refactoring for online
            ClearSelected();
        }

        public void AsyncSale(UIElement button)
        {
           
        }

        public void Update(UpdateState state, bool scrolled)
        {

            LastState = state;

            if (ShowTooltip) state.UIState.TooltipProperties.UpdateDead = false;
            MouseClicked = (MouseIsDown && (!MouseWasDown));

            if (Holding != null)
            {
                if (state.KeyboardState.IsKeyDown(Keys.Delete))
                {
                    SellBack(null);
                } else if (state.KeyboardState.IsKeyDown(Keys.Escape))
                {
                    OnDelete(Holding, null);
                    ClearSelected();
                }
            }
            if (Holding != null)
            {
                if (MouseClicked) Holding.Clicked = true;
                if (MouseIsDown && Holding.Clicked)
                {
                    bool updatePos = MouseClicked;
                    int xDiff = state.MouseState.X - MouseDownX;
                    int yDiff = state.MouseState.Y - MouseDownY;
                    if (Math.Sqrt(xDiff * xDiff + yDiff * yDiff) > 64)
                    {
                        int dir;
                        if (xDiff > 0)
                        {
                            if (yDiff > 0) dir = 1;
                            else dir = 0;
                        }
                        else
                        {
                            if (yDiff > 0) dir = 2;
                            else dir = 3;
                        }
                        var newDir = (Direction)(1 << (((dir + 4 - (int)World.State.Rotation) % 4) * 2));
                        if (newDir != Holding.Dir || MouseClicked)
                        {
                            updatePos = true;
                            HITVM.Get().PlaySoundEvent(UISounds.ObjectRotate);
                            Holding.Dir = newDir;
                            DirChanged = true;
                        }
                    }
                    if (updatePos)
                    {
                        MoveSelected(Holding.TilePos, Holding.Level);
                        if (!Holding.IsBought && Holding.CanPlace == VMPlacementError.Success && 
                            ParentControl.ActiveEntity != null && ParentControl.ActiveEntity.TSOState.Budget.Value < Holding.Price)
                            Holding.CanPlace = VMPlacementError.InsufficientFunds;
                        if (Holding.CanPlace != VMPlacementError.Success)
                        {
                            state.UIState.TooltipProperties.Show = true;
                            state.UIState.TooltipProperties.Color = Color.Black;
                            state.UIState.TooltipProperties.Opacity = 1;
                            state.UIState.TooltipProperties.Position = new Vector2(MouseDownX,
                                MouseDownY);
                            state.UIState.Tooltip = GameFacade.Strings.GetString("137", "kPErr" + Holding.CanPlace.ToString()
                                + ((Holding.CanPlace == VMPlacementError.CannotPlaceComputerOnEndTable) ? "," : ""));
                            // comma added to curcumvent problem with language file. We should probably just index these with numbers?
                            state.UIState.TooltipProperties.UpdateDead = false;
                            ShowTooltip = true;
                            HITVM.Get().PlaySoundEvent(UISounds.Error);
                        }
                        else
                        {
                            state.UIState.TooltipProperties.Show = false;
                            state.UIState.TooltipProperties.Opacity = 0;
                            ShowTooltip = false;
                        }
                    }
                }
                else
                {
                    var tilePos = World.State.WorldSpace.GetTileAtPosWithScroll(new Vector2(state.MouseState.X, state.MouseState.Y) / FSOEnvironment.DPIScaleFactor) + Holding.TilePosOffset;
                    MoveSelected(tilePos, 1);
                }
            }
            else if (MouseClicked)
            {
                //not holding an object, but one can be selected
                var newHover = World.GetObjectIDAtScreenPos(state.MouseState.X / FSOEnvironment.DPIScaleFactor, state.MouseState.Y / FSOEnvironment.DPIScaleFactor, GameFacade.GraphicsDevice);
                if (MouseClicked && (newHover != 0) && (vm.GetObjectById(newHover) is VMGameObject))
                {
                    var objGroup = vm.GetObjectById(newHover).MultitileGroup;
                    var objBasePos = objGroup.BaseObject.Position;
                    if (objBasePos.Level == World.State.Level)
                    {

                        var ghostGroup = vm.Context.GhostCopyGroup(objGroup);
                        SetSelected(ghostGroup);
                        var deleteAllowed = vm.PlatformState.Validator.GetDeleteMode(
                            DeleteMode.Delete, (VMAvatar)ParentControl.ActiveEntity, ghostGroup.BaseObject) != DeleteMode.Disallowed;
                        var canDelete = deleteAllowed && (objGroup.BaseObject.IsUserMovable(vm.Context, true)) == VMPlacementError.Success;

                        Holding.CanDelete = canDelete;
                        Holding.MoveTarget = newHover;
                        Holding.TilePosOffset = new Vector2(objBasePos.x / 16f, objBasePos.y / 16f) - World.State.WorldSpace.GetTileAtPosWithScroll(new Vector2(state.MouseState.X, state.MouseState.Y) / FSOEnvironment.DPIScaleFactor);
                        if (OnPickup != null) OnPickup(Holding, state);
                        //ExecuteEntryPoint(12); //User Pickup
                    }
                    else
                    {
                        state.UIState.TooltipProperties.Show = true;
                        state.UIState.TooltipProperties.Color = Color.Black;
                        state.UIState.TooltipProperties.Opacity = 1;
                        state.UIState.TooltipProperties.Position = new Vector2(MouseDownX,
                            MouseDownY);
                        state.UIState.Tooltip = GameFacade.Strings.GetString("137", "kPErr" + VMPlacementError.CantEffectFirstLevelFromSecondLevel.ToString());
                        state.UIState.TooltipProperties.UpdateDead = false;
                        ShowTooltip = true;
                        HITVM.Get().PlaySoundEvent(UISounds.Error);
                    }
                }
            }

            MouseWasDown = MouseIsDown;
        }

        public delegate void HolderEventHandler(UIObjectSelection holding, UpdateState state);
    }

    public class UIObjectSelection
    {
        public short MoveTarget = 0;

        public VMMultitileGroup Group;
        public VMEntity[] CursorTiles;
        public LotTilePos PreviousTile;
        public Direction Dir = Direction.NORTH;
        public Vector2 TilePos;
        public Vector2 TilePosOffset;
        public uint InventoryPID = 0;
        public bool Clicked, CanDelete;
        public VMPlacementError CanPlace;
        public sbyte Level;
        public int Price;

        public bool IsBought
        {
            get
            {
                return (MoveTarget != 0);
            }
        }
    }
}
