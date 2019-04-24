/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.LotView;
using FSO.LotView.Model;
using FSO.LotView.Components;
using FSO.Content;
using Microsoft.Xna.Framework;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics.Model;
using tso.world.Model;
using FSO.SimAntics.Primitives;
using FSO.Files.Formats.IFF;
using static FSO.Files.Formats.IFF.Chunks.OBJM;

namespace FSO.SimAntics.Utils
{
    /// <summary>
    /// Handles object creation and destruction
    /// </summary>
    public class VMWorldActivator
    {

        public static HashSet<uint> ControllerObjects = new HashSet<uint>()
        {
            0xAED879C5, //phone line
            0xC61F8102, //go studio plugin
            0xEAA79D86, //go neighbourhood
            0xA6F31853, //go downtown
            0xABA9DF4A, //go vacation
            0x99197314, //go magictown
        };

        public static HashSet<ushort> ValidWallStyles = new HashSet<ushort>()
        {
            0x2, 0xc, 0xe, 0xd
        };

        private VM VM;
        private LotView.World World;
        private Blueprint Blueprint;
        private bool FlipRoad;

        public VMWorldActivator(VM vm, LotView.World world){
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

            var arch = VM.Context.Architecture;

            foreach (var floor in model.World.Floors){
                arch.SetFloor(floor.X, floor.Y, (sbyte)(floor.Level+1), new FloorTile { Pattern = (ushort)floor.Value }, true);
            }

            foreach (var pool in model.World.Pools)
            {
                arch.SetFloor(pool.X, pool.Y, 1, new FloorTile { Pattern = 65535 }, true);
            }

            foreach (var wall in model.World.Walls)
            {
                arch.SetWall((short)wall.X, (short)wall.Y, (sbyte)(wall.Level+1), new WallTile() //todo: these should read out in their intended formats - a cast shouldn't be necessary
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
                //if (obj.Level == 0) continue;
                //if (obj.GUID == "0xE9CEB12F") obj.GUID = "0x01A0FD79"; //replace onlinejobs door with a normal one
                //if (obj.GUID == "0x346FE2BC") obj.GUID = "0x98E0F8BD"; //replace kitchen door with a normal one
                CreateObject(obj);
            }

            if (VM.UseWorld)
            {
                foreach (var obj in model.Sounds)
                {
                    VM.Context.Ambience.SetAmbience(VM.Context.Ambience.GetAmbienceFromGUID(obj.ID), (obj.On == 1));
                    World.State.WorldSize = model.Size;
                    
                }
                Blueprint.Terrain = CreateTerrain(model);
            }

            var testObject = new XmlHouseDataObject(); //test npc controller, not normally present on a job lot.
            testObject.GUID = "0x70F69082";
            testObject.X = 0;
            testObject.Y = 0;
            testObject.Level = 1;
            testObject.Dir = 0;
            CreateObject(testObject);

            arch.Tick();
            return this.Blueprint;
        }

        public Blueprint LoadFromIff(Files.Formats.IFF.IffFile iff)
        {
            var simi = iff.Get<SIMI>(1);
            var hous = iff.Get<HOUS>(0);

            short size = simi.GlobalData[23];
            var type = simi.GlobalData[35];  //ts1 lots are 64x64... but we convert them into dynamic size.
            //if (VM.UseWorld) 
            this.Blueprint = new Blueprint(size, size);
            //VM.Entities = new List<VMEntity>();
            //VM.Context = new VMContext(VM.Context.World);
            //VM.Context.VM = VM;
            VM.Context.Blueprint = Blueprint;
            VM.Context.Architecture = new VMArchitecture(size, size, Blueprint, VM.Context);

            FlipRoad = (hous.CameraDir & 1) > 0;

            VM.GlobalState = simi.GlobalData;

            VM.GlobalState[20] = 255; //Game Edition. Basically, what "expansion packs" are running. Let's just say all of them.
            VM.GlobalState[25] = 4; //as seen in EA-Land edith's simulator globals, this needs to be set for people to do their idle interactions.
            VM.GlobalState[17] = 4; //Runtime Code Version, is this in EA-Land.

            VM.SetGlobalValue(10, 1); //set house number
            VM.SetGlobalValue(32, 0); //simless build mode
            VM.Context.Clock.Hours = VM.GlobalState[0];
            VM.Context.Clock.DayOfMonth = VM.GlobalState[1];
            VM.Context.Clock.Minutes = VM.GlobalState[5];
            VM.Context.Clock.MinuteFractions = (VM.GlobalState[6] * VM.Context.Clock.TicksPerMinute);
            VM.Context.Clock.Month = VM.GlobalState[7];
            VM.Context.Clock.Year = VM.GlobalState[8];


            var floorM = iff.Get<FLRm>(1)?.Entries ?? iff.Get<FLRm>(0)?.Entries ?? new List<WALmEntry>();
            var wallM = iff.Get<WALm>(1)?.Entries ?? iff.Get<WALm>(0)?.Entries ?? new List<WALmEntry>();

            var floorDict = BuildFloorDict(floorM);
            var wallDict = BuildWallDict(wallM);

            var arch = VM.Context.Architecture;
            //altitude as 0
            var advFloors = iff.Get<ARRY>(11);
            var flags = iff.Get<ARRY>(8).TransposeData;
            if (advFloors != null)
            {
                //advanced walls and floors from modern ts1. use 16 bit wall/floor data.
                arch.Floors[0] = RemapFloors(DecodeAdvFloors(advFloors.TransposeData), floorDict, flags);
                arch.Floors[1] = RemapFloors(DecodeAdvFloors(iff.Get<ARRY>(111).TransposeData), floorDict, flags);
                //objects as 3
                arch.Walls[0] = RemapWalls(DecodeAdvWalls(iff.Get<ARRY>(12).TransposeData), wallDict, floorDict);
                arch.Walls[1] = RemapWalls(DecodeAdvWalls(iff.Get<ARRY>(112).TransposeData), wallDict, floorDict);
            }
            else
            {
                arch.Floors[0] = RemapFloors(DecodeFloors(iff.Get<ARRY>(1).TransposeData), floorDict, flags);
                arch.Floors[1] = RemapFloors(DecodeFloors(iff.Get<ARRY>(101).TransposeData), floorDict, flags);
                //objects as 3
                arch.Walls[0] = RemapWalls(DecodeWalls(iff.Get<ARRY>(2).TransposeData), wallDict, floorDict);
                arch.Walls[1] = RemapWalls(DecodeWalls(iff.Get<ARRY>(102).TransposeData), wallDict, floorDict);
            }
            //objects as 103
            //arch.Terrain.GrassState = iff.Get<ARRY>(6).TransposeData.Select(x => (byte)(127 - x)).ToArray();
            //arch.Terrain.DarkType = Content.Model.TerrainType.SAND;
            //arch.Terrain.LightType = Content.Model.TerrainType.GRASS;
            arch.SignalRedraw();
            //targetgrass is 7
            //flags is 8/108
            var pools = iff.Get<ARRY>(9).TransposeData;
            var water = iff.Get<ARRY>(10).TransposeData;

            for (int i = 0; i < pools.Length; i++)
            {
                //pools in freeso are slightly different
                if (pools[i] != 0xff && pools[i] != 0x0) arch.Floors[0][i].Pattern = 65535;
                if (water[i] != 0xff && water[i] != 0x0) arch.Floors[0][i].Pattern = 65534;
            }

            arch.Floors[0] = ResizeFloors(arch.Floors[0], size);
            arch.Floors[1] = ResizeFloors(arch.Floors[1], size);
            arch.Walls[0] = ResizeWalls(arch.Walls[0], size);
            arch.Walls[1] = ResizeWalls(arch.Walls[1], size);
            //arch.FineBuildableArea = ResizeFlags(flags, size);
            //arch.Terrain.GrassState = ResizeGrass(arch.Terrain.GrassState, size);
            //arch.Terrain.Heights = Array.ConvertAll(ResizeGrass(DecodeHeights(iff.Get<ARRY>(0).TransposeData), size), x => (short)(x * 10));
            //arch.Terrain.RegenerateCenters();
            //arch.RoofStyle = (uint)Content.Content.Get().WorldRoofs.NameToID(hous.RoofName.ToLowerInvariant() + ".bmp");

            if (VM.UseWorld)
            {
                World.State.WorldSize = size;
                Blueprint.Terrain = CreateTerrain(size);
                //Blueprint.Altitude = arch.Terrain.Heights;
                //Blueprint.FineArea = arch.FineBuildableArea;
            }

            arch.RegenWallsAt();

            arch.RegenRoomMap();
            VM.Context.RegeneratePortalInfo();


            var objm = iff.Get<OBJM>(1);

            var objt = iff.Get<OBJT>(0);
            int j = 0;

            for (int i = 0; i < objm.IDToOBJT.Length; i += 2)
            {
                if (objm.IDToOBJT[i] == 0) continue;
                MappedObject target;
                if (!objm.ObjectData.TryGetValue(objm.IDToOBJT[i], out target)) continue;
                var entry = objt.Entries[objm.IDToOBJT[i + 1] - 1];
                target.Name = entry.Name;
                target.GUID = entry.GUID;

                //Console.WriteLine((objm.IDToOBJT[i]) + ": " + objt.Entries[objm.IDToOBJT[i + 1] - 1].Name);
            }

            var objFlrs = new ushort[][] { DecodeObjID(iff.Get<ARRY>(3)?.TransposeData), DecodeObjID(iff.Get<ARRY>(103)?.TransposeData) };
            for (int flr = 0; flr < 2; flr++)
            {
                var objs = objFlrs[flr];
                if (objs == null) continue;
                for (int i = 0; i < objs.Length; i++)
                {
                    var obj = objs[i];
                    if (obj != 0)
                    {
                        var x = i % 64;
                        var y = i / 64;
                        MappedObject targ;
                        if (!objm.ObjectData.TryGetValue(obj, out targ)) continue;
                        targ.ArryX = x;
                        targ.ArryY = y;
                        targ.ArryLevel = flr + 1;
                    }
                }
            }

            var content = Content.Content.Get();
            foreach (var controller in ControllerObjects)
            {
                VM.Context.CreateObjectInstance(controller, LotTilePos.OUT_OF_WORLD, Direction.NORTH);
            }
            var ents = new List<Tuple<VMEntity, OBJM.MappedObject>>();
            foreach (var obj in objm.ObjectData.Values)
            {
                if (ControllerObjects.Contains(obj.GUID)) continue;
                var res = content.WorldObjects.Get(obj.GUID);
                if (res == null) continue; //failed to load this object
                var objd = res.OBJ;
                if (res.OBJ.MasterID != 0)
                {
                    var allObjs = res.Resource.List<OBJD>().Where(x => x.MasterID == res.OBJ.MasterID);
                    var hasLead = allObjs.Any(x => x.MyLeadObject != 0);
                    if ((hasLead && res.OBJ.MyLeadObject != 0) || (!hasLead && res.OBJ.SubIndex == 0))
                    {
                        //this is the object
                        //look for its master

                        //multitile object.. find master objd

                        var master = allObjs.FirstOrDefault(x => x.SubIndex < 0);
                        if (master == null) continue;
                        objd = master;
                        obj.GUID = master.GUID;
                    }
                    else
                    {
                        continue;
                    }
                }

                //if (DeleteAvatars && objd.ObjectType == OBJDType.Person) continue;

                //objm parent positioning
                //objects without positions inherit position from the objects in their "parent id".
                MappedObject src = obj;
                while (src != null && src.ParentID != 0)
                {
                    if (objm.ObjectData.TryGetValue(src.ParentID, out src))
                    {
                        if (src.ParentID == 0)
                        {
                            obj.ArryX = src.ArryX;
                            obj.ArryY = src.ArryY;
                            obj.ArryLevel = src.ArryLevel;
                        }
                    }
                }

                LotTilePos pos = LotTilePos.OUT_OF_WORLD;
                var dir = (Direction)(1 << obj.Direction);
                var nobj = VM.Context.CreateObjectInstance(obj.GUID, pos, dir).BaseObject;
                if (nobj == null) continue;
                if (obj.ContainerID == 0 && obj.ArryX != 0 && obj.ArryY != 0)
                    nobj.SetPosition(LotTilePos.FromBigTile((short)(obj.ArryX), (short)(obj.ArryY), (sbyte)obj.ArryLevel), dir, VM.Context);

                ents.Add(new Tuple<VMEntity, OBJM.MappedObject>(nobj, obj));
            }

            //place objects in slots
            foreach (var ent in ents)
            {
                var obj = ent.Item2;
                if (ent.Item1.Position == LotTilePos.OUT_OF_WORLD && obj.ContainerID != 0 && obj.ArryX != 0 && obj.ArryY != 0)
                {
                    var dir = (Direction)(1 << obj.Direction);
                    ent.Item1.SetPosition(LotTilePos.FromBigTile((short)(obj.ArryX), (short)(obj.ArryY), (sbyte)obj.ArryLevel), dir, VM.Context);
                    /*
                    var target = ents.First(x => x.Item2.ObjectID == obj.ContainerID);
                    target.Item1.PlaceInSlot(ent.Item1, obj.ContainerSlot, true, VM.Context);
                    */
                }
            }

            var entClone = new List<VMEntity>(VM.Entities);
            foreach (var nobj in entClone) nobj.ExecuteEntryPoint(11, VM.Context, true);

            arch.SignalRedraw();
            VM.Context.World?.InitBlueprint(Blueprint);
            arch.Tick();


            return Blueprint;
        }

        private bool[] ResizeFlags(byte[] flags, int size)
        {
            //flag 1 = not sloped
            //flag 2 = ? (tends to be active alongside 4)
            //flag 4 = ?
            //flag 32 = user buildable
            if (size >= 64) return flags.Select(x => (x & 32) == 0).ToArray();
            var result = new bool[size * size];
            int iS = 0;
            int iD = 0;
            for (int y = 0; y < 64; y++)
            {
                if (y >= size) return result;
                for (int x = 0; x < 64; x++)
                {
                    if (x < size) result[iD++] = (flags[iS] & 32) == 0;
                    iS++;
                }

            }
            return result;
        }

        private FloorTile[] ResizeFloors(FloorTile[] floors, int size)
        {
            if (size >= 64) return floors;
            var result = new FloorTile[size * size];
            int iS = 0;
            int iD = 0;
            for (int y = 0; y < 64; y++)
            {
                if (y >= size) return result;
                for (int x = 0; x < 64; x++)
                {
                    if (x < size) result[iD++] = floors[iS];
                    iS++;
                }

            }
            return result;
        }

        private WallTile[] ResizeWalls(WallTile[] walls, int size)
        {
            if (size >= 64) return walls;
            var result = new WallTile[size * size];
            int iS = 0;
            int iD = 0;
            for (int y = 0; y < 64; y++)
            {
                if (y >= size) return result;
                for (int x = 0; x < 64; x++)
                {
                    if (x < size) result[iD++] = walls[iS];
                    iS++;
                }
            }
            return result;
        }

        private FloorTile[] RemapFloors(FloorTile[] floors, Dictionary<byte, ushort> dict, byte[] flags)
        {
            for (int i = 0; i < floors.Length; i++)
            {
                var floor = floors[i];
                if (FlipRoad)
                {
                    if (floor.Pattern == 10)
                        floors[i].Pattern = 11;
                    else if (floor.Pattern == 11)
                        floors[i].Pattern = 10;
                }
                if (floor.Pattern != 0 && ((flags[i] & 32) == 0) || floor.Pattern > 30)
                {
                    ushort newID;
                    if (dict.TryGetValue((byte)floor.Pattern, out newID))
                    {
                        floors[i].Pattern = newID;
                    }
                }
            }
            return floors;
        }

        private WallTile[] RemapWalls(WallTile[] walls, Dictionary<byte, ushort> dict, Dictionary<byte, ushort> floorDict)
        {
            for (int i = 0; i < walls.Length; i++)
            {
                var wall = walls[i];
                ushort newID;
                if (wall.BottomLeftPattern != 0)
                {
                    if (dict.TryGetValue((byte)wall.BottomLeftPattern, out newID))
                        walls[i].BottomLeftPattern = newID;
                }
                if (wall.BottomRightPattern != 0)
                {
                    if (dict.TryGetValue((byte)wall.BottomRightPattern, out newID))
                        walls[i].BottomRightPattern = newID;
                }
                if ((wall.Segments & WallSegments.AnyDiag) > 0)
                {
                    //diagonally split floors
                    if (wall.TopLeftPattern != 0)
                    {
                        if (floorDict.TryGetValue((byte)wall.TopLeftPattern, out newID))
                            walls[i].TopLeftPattern = newID;
                    }
                    if (wall.TopLeftStyle != 0)
                    {
                        if (floorDict.TryGetValue((byte)wall.TopLeftStyle, out newID))
                            walls[i].TopLeftStyle = newID;
                    }
                }
                else
                {
                    if (wall.TopLeftPattern != 0)
                    {
                        if (dict.TryGetValue((byte)wall.TopLeftPattern, out newID))
                            walls[i].TopLeftPattern = newID;
                    }
                    if (wall.TopRightPattern != 0)
                    {
                        if (dict.TryGetValue((byte)wall.TopRightPattern, out newID))
                            walls[i].TopRightPattern = newID;
                    }
                }
            }
            return walls;
        }

        public FloorTile[] DecodeFloors(byte[] data)
        {
            //todo: flrm map
            return data.Select(x => new FloorTile { Pattern = x }).ToArray();
        }

        public FloorTile[] DecodeAdvFloors(byte[] data)
        {
            int j = 0;
            var result = new FloorTile[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                result[j++] = new FloorTile() { Pattern = (ushort)(data[i] | (data[i + 1] << 8)) };
            }
            return result;
        }

        public ushort[] DecodeObjID(byte[] data)
        {
            if (data == null) return null;
            int j = 0;
            var result = new ushort[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                result[j++] = (ushort)(data[i] | (data[i + 1] << 8));
            }
            return result;
        }

        public WallTile[] DecodeWalls(byte[] data)
        {
            int j = 0;
            var result = new WallTile[data.Length / 8];
            for (int i = 0; i < data.Length; i += 8)
            {
                var tile = new WallTile
                {
                    Segments = (WallSegments)data[i],
                    TopLeftStyle = (ushort)(data[i + 2]),
                    TopRightStyle = (ushort)(data[i + 3]),
                    TopLeftPattern = (ushort)(data[i + 4]),
                    TopRightPattern = (ushort)(data[i + 5]),
                    BottomLeftPattern = (ushort)(data[i + 6]),
                    BottomRightPattern = (ushort)(data[i + 7])
                };

                if ((tile.Segments & WallSegments.AnyDiag) == 0)
                {
                    if (!ValidWallStyles.Contains(tile.TopLeftStyle)) tile.TopLeftStyle = 1;
                    if (!ValidWallStyles.Contains(tile.TopRightStyle)) tile.TopRightStyle = 1;
                    if ((tile.Segments & WallSegments.TopLeft) == 0) tile.TopLeftStyle = 0;
                    if ((tile.Segments & WallSegments.TopRight) == 0) tile.TopRightStyle = 0;
                }
                result[j++] = tile;
            }
            return result;
        }

        public WallTile[] DecodeAdvWalls(byte[] data)
        {
            int j = 0;
            var result = new WallTile[data.Length / 14];
            for (int i = 0; i < data.Length; i += 14)
            {
                var tile = new WallTile
                {
                    Segments = (WallSegments)data[i],
                    TopLeftStyle = (ushort)(data[i + 2] | (data[i + 3] << 8)),
                    TopRightStyle = (ushort)(data[i + 4] | (data[i + 5] << 8)),
                    TopLeftPattern = (ushort)(data[i + 6] | (data[i + 7] << 8)),
                    TopRightPattern = (ushort)(data[i + 8] | (data[i + 9] << 8)),
                    BottomLeftPattern = (ushort)(data[i + 10] | (data[i + 11] << 8)),
                    BottomRightPattern = (ushort)(data[i + 12] | (data[i + 13] << 8))
                };

                if ((tile.Segments & WallSegments.AnyDiag) == 0)
                {
                    if (!ValidWallStyles.Contains(tile.TopLeftStyle)) tile.TopLeftStyle = 1;
                    if (!ValidWallStyles.Contains(tile.TopRightStyle)) tile.TopRightStyle = 1;
                    if ((tile.Segments & WallSegments.TopLeft) == 0) tile.TopLeftStyle = 0;
                    if ((tile.Segments & WallSegments.TopRight) == 0) tile.TopRightStyle = 0;
                }
                result[j++] = tile;
            }
            return result;
        }

        private Dictionary<byte, ushort> BuildFloorDict(List<WALmEntry> entries)
        {
            var c = Content.Content.Get().WorldFloors.DynamicFloorFromID;
            var result = new Dictionary<byte, ushort>();
            foreach (var entry in entries)
            {
                ushort newID;
                if (c.TryGetValue(new string(entry.Name.TakeWhile(x => x != '.').ToArray()).ToLowerInvariant(), out newID))
                {
                    result[entry.ID] = newID;
                }
            }
            return result;
        }

        private Dictionary<byte, ushort> BuildWallDict(List<WALmEntry> entries)
        {
            var c = Content.Content.Get().WorldWalls.DynamicWallFromID;
            var result = new Dictionary<byte, ushort>();
            foreach (var entry in entries)
            {
                ushort newID;
                if (c.TryGetValue(new string(entry.Name.TakeWhile(x => x != '.').ToArray()).ToLowerInvariant(), out newID))
                {
                    result[entry.ID] = newID;
                }
            }
            return result;
        }

        private TerrainComponent CreateTerrain(XmlHouseData model)
        {
            var terrain = new TerrainComponent(new Rectangle(1, 1, model.Size - 2, model.Size - 2));
            this.InitWorldComponent(terrain);
            return terrain;
        }

        private TerrainComponent CreateTerrain(short Size)
        {
            var terrain = new TerrainComponent(new Rectangle(1, 1, Size - 2, Size - 2));
            this.InitWorldComponent(terrain);
            return terrain;
        }

        public VMAvatar CreateAvatar()
        {
            return (VMAvatar)VM.Context.CreateObjectInstance(VMAvatar.TEMPLATE_PERSON, LotTilePos.OUT_OF_WORLD, Direction.NORTH).Objects[0];
        }


        public VMAvatar CreateAvatar(uint guid, XmlCharacter xml, bool visitor, short id)
        {

            VMAvatar avatar = (VMAvatar)VM.Context.CreateObjectInstance(guid, LotTilePos.OUT_OF_WORLD, Direction.NORTH).Objects[0];
            avatar.Visitor = visitor;
            var mailbox = VM.Entities.First(x => (x.Object.OBJ.GUID == 0xEF121974 || x.Object.OBJ.GUID == 0x1D95C9B0));


            avatar.SetAvatarData(xml);

            LotTilePos pos = mailbox.Position;
            pos.x = (short)(mailbox.Position.x + 1);
            pos.y = (short)( mailbox.Position.y + id);

            avatar.SetPosition(pos, Direction.WEST, VM.Context);
            //VMFindLocationFor.FindLocationFor(avatar, mailbox, VM.Context);
                       

            return avatar;
        }

        public void CreateObject(XmlHouseDataObject obj){
            LotTilePos pos = (obj.Level == 0) ? LotTilePos.OUT_OF_WORLD : LotTilePos.FromBigTile((short)obj.X, (short)obj.Y, (sbyte)obj.Level);
            
            var mojb = VM.Context.CreateObjectInstance(obj.GUIDInt, pos, obj.Direction);

            if (mojb == null) return;

            var nobj = mojb.Objects[0];

            if (obj.Group != 0)
            {
                foreach (var sub in nobj.MultitileGroup.Objects)
                {
                    sub.SetValue(VMStackObjectVariable.GroupID, (short)obj.Group);
                }
            }

            for (int i = 0; i < nobj.MultitileGroup.Objects.Count; i++) nobj.MultitileGroup.Objects[i].ExecuteEntryPoint(11, VM.Context, true);

            //return nobj;
            
        }


        private void InitWorldComponent(WorldComponent component)
        {
            component.Initialize(this.World.State.Device, this.World.State);
        }

    }
}
