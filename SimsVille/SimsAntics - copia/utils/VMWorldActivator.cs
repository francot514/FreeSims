﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tso.world;
using tso.world.Model;
using tso.world.Components;
using TSO.Content;
using Microsoft.Xna.Framework;
using TSO.Files.formats.iff.chunks;
using TSO.SimsAntics.Model;

namespace TSO.SimsAntics.Utils
{
    /// <summary>
    /// Handles object creation and destruction
    /// </summary>
    public class VMWorldActivator
    {
        private VM VM;
        private World World;
        private Blueprint Blueprint;

        public VMWorldActivator(VM vm, World world){
            this.VM = vm;
            this.World = world;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        public Blueprint LoadFromXML(XmlHouseData model){
            this.Blueprint = new Blueprint(model.Size, model.Size);
            VM.Context.Blueprint = Blueprint;
            VM.Context.Architecture = new VMArchitecture(model.Size, model.Size, Blueprint, VM.Context);
            VM.Context.Clock.Hours = model.TimeofDay;

            int category = model.Category;
            VM.Context.LotCategory = category;

            VM.Context.Blueprint.JobLot = (category >= 100) ? true : false;

            if (category == 10)
                VM.Context.Blueprint.LotType = LotTypes.Rock;
            else if (category == 40)
                VM.Context.Blueprint.LotType = LotTypes.Snow;
            else if (category == 80)
                VM.Context.Blueprint.LotType = LotTypes.Sand;
            else if (category >= 100)
                VM.Context.Blueprint.LotType = LotTypes.Grass;

            var arch = VM.Context.Architecture;

            foreach (var floor in model.World.Floors)
            {
                arch.SetFloor(floor.X, floor.Y, (sbyte)(floor.Level + 1), new FloorTile { Pattern = (ushort)floor.Value }, true);
            }

            foreach (var pool in model.World.Pools)
            {
                arch.SetFloor(pool.X, pool.Y, 1, new FloorTile { Pattern = 65535 }, true);
            }

            foreach (var wall in model.World.Walls)
            {
                arch.SetWall((short)wall.X, (short)wall.Y, (sbyte)(wall.Level + 1), new WallTile() //todo: these should read out in their intended formats - a cast shouldn't be necessary
                {
                    Segments = wall.Segments,
                    TopLeftPattern = (ushort)wall.TopLeftPattern,
                    TopRightPattern = (ushort)wall.TopRightPattern,
                    BottomLeftPattern = (ushort)wall.BottomLeftPattern,
                    BottomRightPattern = (ushort)wall.BottomRightPattern,
                    TopLeftStyle = (ushort)wall.LeftStyle,
                    TopRightStyle = (ushort)wall.RightStyle
                });
            }
            arch.RegenRoomMap();
            VM.Context.RegeneratePortalInfo();

            foreach (var obj in model.Objects)
            {
                if (obj.Level == 0) continue;
                //if (obj.GUID == "0xE9CEB12F") obj.GUID = "0x01A0FD79"; //replace onlinejobs door with a normal one
                //if (obj.GUID == "0x346FE2BC") obj.GUID = "0x98E0F8BD"; //replace kitchen door with a normal one
                CreateObject(obj);
            }

            

            if (VM.UseWorld)
            {
                World.State.WorldSize = model.Size;
               
                Blueprint.Terrain = CreateTerrain(model);
               
            }

            return this.Blueprint;
        }

        private TerrainComponent CreateTerrain(XmlHouseData model)
        {
            var terrain = new TerrainComponent(new Rectangle(1, 1, model.Size - 2, model.Size - 2), VM.Context.Blueprint.LotType);
            this.InitWorldComponent(terrain);
            return terrain;
        }

        public VMAvatar CreateAvatar()
        {
            return (VMAvatar)VM.Context.CreateObjectInstance(VMAvatar.TEMPLATE_PERSON, LotTilePos.OUT_OF_WORLD, Direction.NORTH).Objects[0];
        }

        public VMAvatar CreateAvatar(uint guid)
        {
            return (VMAvatar)VM.Context.CreateObjectInstance(guid, LotTilePos.OUT_OF_WORLD, Direction.NORTH).Objects[0];
        }


        public VMEntity CreateObject(XmlHouseDataObject obj){
            LotTilePos pos = (obj.Level == 0) ? LotTilePos.OUT_OF_WORLD : LotTilePos.FromBigTile((short)obj.X, (short)obj.Y, (sbyte)obj.Level);

            var nobj = VM.Context.CreateObjectInstance(obj.GUIDInt, pos, obj.Direction).Objects[0];

            if (obj.Group != 0)
            {
                foreach (var sub in nobj.MultitileGroup.Objects)
                {
                    sub.SetValue(VMStackObjectVariable.GroupID, (short)obj.Group);
                }
            }

            for (int i = 0; i < nobj.MultitileGroup.Objects.Count; i++) nobj.MultitileGroup.Objects[i].ExecuteEntryPoint(11, VM.Context, true);

            return null;
            
        }


        private void InitWorldComponent(WorldComponent component)
        {
            component.Initialize(this.World.State.Device, this.World.State);
        }

    }
}
