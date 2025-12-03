/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using FSO.Content;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using FSO.LotView;
using FSO.LotView.Components;
using FSO.LotView.Model;
using FSO.SimAntics;
using FSO.SimAntics.Entities;
using FSO.SimAntics.Model;
using Microsoft.Xna.Framework;
using tso.world.Model;

public class VMWorldActivator
{
	public static HashSet<uint> ControllerObjects = new HashSet<uint> { 2933422533u, 3323953410u, 3936853382u, 2800949331u, 2880036682u, 2568581908u };

	public static HashSet<ushort> ValidWallStyles = new HashSet<ushort> { 2, 12, 14, 13 };

	private VM VM;

	private World World;

	private Blueprint Blueprint;

	private bool FlipRoad;

	public VMWorldActivator(VM vm, World world)
	{
		VM = vm;
		World = world;
	}

	public Blueprint LoadFromXML(XmlHouseData model)
	{

        var size = 0;
        if (size == 0) size = model.Size;

        Blueprint = new Blueprint(model.Size, model.Size);
		VM.Context.Blueprint = Blueprint;
		VM.Context.Architecture = new VMArchitecture(model.Size, model.Size, Blueprint, VM.Context);
		VMArchitecture arch = VM.Context.Architecture;
		foreach (XmlHouseDataFloor floor in model.World.Floors)
		{
			arch.SetFloor(floor.X, floor.Y, (sbyte)(floor.Level + 1), new FloorTile
			{
				Pattern = (ushort)floor.Value
			}, force: true);
		}
		foreach (XmlHouseDataPool pool in model.World.Pools)
		{
			arch.SetFloor(pool.X, pool.Y, 1, new FloorTile
			{
				Pattern = ushort.MaxValue
			}, force: true);
		}
		foreach (XmlHouseDataWall wall in model.World.Walls)
		{
			arch.SetWall((short)wall.X, (short)wall.Y, (sbyte)(wall.Level + 1), new WallTile
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
		foreach (XmlHouseDataObject obj2 in model.Objects)
		{
			CreateObject(obj2);
		}
		if (VM.UseWorld)
		{
			foreach (XmlSoundData obj in model.Sounds)
			{
				VM.Context.Ambience.SetAmbience(VM.Context.Ambience.GetAmbienceFromGUID(obj.ID), obj.On == 1);
				
			}
			Blueprint.Terrain = CreateTerrain(model);
		}
		XmlHouseDataObject testObject = new XmlHouseDataObject();
		testObject.GUID = "0x70F69082";
		testObject.X = 0;
		testObject.Y = 0;
		testObject.Level = 1;
		testObject.Dir = 0;

        if (VM.UseWorld) World.State.WorldSize = size;

        CreateObject(testObject);
		arch.Tick();
		return Blueprint;
	}

	public Blueprint LoadFromIff(IffFile iff)
	{
		SIMI simi = iff.Get<SIMI>(1);
		HOUS hous = iff.Get<HOUS>(0);
		short size = simi.GlobalData[23];
		short type = simi.GlobalData[35];
		Blueprint = new Blueprint(size, size);
		VM.Context.Blueprint = Blueprint;
		VM.Context.Architecture = new VMArchitecture(size, size, Blueprint, VM.Context);
		FlipRoad = (hous.CameraDir & 1) > 0;
		VM.GlobalState = simi.GlobalData;
		VM.GlobalState[20] = 255;
		VM.GlobalState[25] = 4;
		VM.GlobalState[17] = 4;
		VM.SetGlobalValue(10, 1);
		VM.SetGlobalValue(32, 0);
		VM.Context.Clock.Hours = VM.GlobalState[0];
		VM.Context.Clock.DayOfMonth = VM.GlobalState[1];
		VM.Context.Clock.Minutes = VM.GlobalState[5];
		VM.Context.Clock.MinuteFractions = VM.GlobalState[6] * VM.Context.Clock.TicksPerMinute;
		VM.Context.Clock.Month = VM.GlobalState[7];
		VM.Context.Clock.Year = VM.GlobalState[8];
		List<WALmEntry> floorM = iff.Get<FLRm>(1)?.Entries ?? iff.Get<FLRm>(0)?.Entries ?? new List<WALmEntry>();
		List<WALmEntry> wallM = iff.Get<WALm>(1)?.Entries ?? iff.Get<WALm>(0)?.Entries ?? new List<WALmEntry>();
		Dictionary<byte, ushort> floorDict = BuildFloorDict(floorM);
		Dictionary<byte, ushort> wallDict = BuildWallDict(wallM);
		VMArchitecture arch = VM.Context.Architecture;
		ARRY advFloors = iff.Get<ARRY>(11);
		byte[] flags = iff.Get<ARRY>(8).TransposeData;
		if (advFloors != null)
		{
			arch.Floors[0] = RemapFloors(DecodeAdvFloors(advFloors.TransposeData), floorDict, flags);
			arch.Floors[1] = RemapFloors(DecodeAdvFloors(iff.Get<ARRY>(111).TransposeData), floorDict, flags);
			arch.Walls[0] = RemapWalls(DecodeAdvWalls(iff.Get<ARRY>(12).TransposeData), wallDict, floorDict);
			arch.Walls[1] = RemapWalls(DecodeAdvWalls(iff.Get<ARRY>(112).TransposeData), wallDict, floorDict);
		}
		else
		{
			arch.Floors[0] = RemapFloors(DecodeFloors(iff.Get<ARRY>(1).TransposeData), floorDict, flags);
			arch.Floors[1] = RemapFloors(DecodeFloors(iff.Get<ARRY>(101).TransposeData), floorDict, flags);
			arch.Walls[0] = RemapWalls(DecodeWalls(iff.Get<ARRY>(2).TransposeData), wallDict, floorDict);
			arch.Walls[1] = RemapWalls(DecodeWalls(iff.Get<ARRY>(102).TransposeData), wallDict, floorDict);
		}
		arch.SignalRedraw();
		byte[] pools = iff.Get<ARRY>(9).TransposeData;
		byte[] water = iff.Get<ARRY>(10).TransposeData;
		for (int j = 0; j < pools.Length; j++)
		{
			if (pools[j] != byte.MaxValue && pools[j] != 0)
			{
				arch.Floors[0][j].Pattern = ushort.MaxValue;
			}
			if (water[j] != byte.MaxValue && water[j] != 0)
			{
				arch.Floors[0][j].Pattern = 65534;
			}
		}
		arch.Floors[0] = ResizeFloors(arch.Floors[0], size);
		arch.Floors[1] = ResizeFloors(arch.Floors[1], size);
		arch.Walls[0] = ResizeWalls(arch.Walls[0], size);
		arch.Walls[1] = ResizeWalls(arch.Walls[1], size);
		arch.RoofStyle = (uint)Content.Get().WorldRoofs.NameToID(hous.RoofName.ToLowerInvariant() + ".bmp");
		if (VM.UseWorld)
		{
			World.State.WorldSize = size;
			Blueprint.Terrain = CreateTerrain(size);
		}
		arch.RegenWallsAt();
		arch.RegenRoomMap();
		VM.Context.RegeneratePortalInfo();
		OBJM objm = iff.Get<OBJM>(1);
		OBJT objt = iff.Get<OBJT>(0);
		int l = 0;
		for (ushort k = 0; k < objm.IDToOBJT.Count(); k += 2)
		{
			if (objm.IDToOBJT[k] != 0 && objm.ObjectData.TryGetValue(objm.IDToOBJT[k], out var target))
			{
				OBJTEntry entry = objt.Entries[objm.IDToOBJT[(ushort)(k + 1)] - 1];
				target.Name = entry.Name;
				target.GUID = entry.GUID;
			}
		}
		ushort[][] objFlrs = new ushort[2][]
		{
			DecodeObjID(iff.Get<ARRY>(3)?.TransposeData),
			DecodeObjID(iff.Get<ARRY>(103)?.TransposeData)
		};
		for (int flr = 0; flr < 2; flr++)
		{
			ushort[] objs = objFlrs[flr];
			if (objs == null)
			{
				continue;
			}
			for (int i = 0; i < objs.Length; i++)
			{
				ushort obj = objs[i];
				if (obj != 0)
				{
					int x2 = i % 64;
					int y = i / 64;
					if (objm.ObjectData.TryGetValue(obj, out var targ))
					{
						targ.ArryX = x2;
						targ.ArryY = y;
						targ.ArryLevel = flr + 1;
					}
				}
			}
		}
		Content content = Content.Get();
		foreach (uint controller in ControllerObjects)
		{
			VM.Context.CreateObjectInstance(controller, LotTilePos.OUT_OF_WORLD, Direction.NORTH, ts1: false);
		}
		List<Tuple<VMEntity, OBJM.MappedObject>> ents = new List<Tuple<VMEntity, OBJM.MappedObject>>();
		foreach (OBJM.MappedObject obj3 in objm.ObjectData.Values)
		{
			if (ControllerObjects.Contains(obj3.GUID))
			{
				continue;
			}
			GameObject res = content.WorldObjects.Get(obj3.GUID, ts1: false);
			if (res == null)
			{
				continue;
			}
			OBJD objd = res.OBJ;
			if (res.OBJ.MasterID != 0)
			{
				IEnumerable<OBJD> allObjs = from x in res.Resource.List<OBJD>()
					where x.MasterID == res.OBJ.MasterID
					select x;
				bool hasLead = allObjs.Any((OBJD x) => x.MyLeadObject != 0);
				if ((!hasLead || res.OBJ.MyLeadObject == 0) && (hasLead || res.OBJ.SubIndex != 0))
				{
					continue;
				}
				OBJD master = allObjs.FirstOrDefault((OBJD x) => x.SubIndex < 0);
				if (master == null)
				{
					continue;
				}
				objd = master;
				obj3.GUID = master.GUID;
			}
			OBJM.MappedObject src = obj3;
			while (src != null && src.ParentID != 0)
			{
				if (objm.ObjectData.TryGetValue(src.ParentID, out src) && src.ParentID == 0)
				{
					obj3.ArryX = src.ArryX;
					obj3.ArryY = src.ArryY;
					obj3.ArryLevel = src.ArryLevel;
				}
			}
			LotTilePos pos = LotTilePos.OUT_OF_WORLD;
			Direction dir2 = (Direction)(1 << obj3.Direction);
			VMMultitileGroup nobj2 = VM.Context.CreateObjectInstance(obj3.GUID, pos, dir2, ts1: true);
			if (nobj2.Objects.Count != 0)
			{
				if (obj3.ContainerID == 0 && obj3.ArryX != 0 && obj3.ArryY != 0)
				{
					nobj2.BaseObject.SetPosition(LotTilePos.FromBigTile((short)obj3.ArryX, (short)obj3.ArryY, (sbyte)obj3.ArryLevel), dir2, VM.Context);
				}
				ents.Add(new Tuple<VMEntity, OBJM.MappedObject>(nobj2.BaseObject, obj3));
			}
		}
		foreach (Tuple<VMEntity, OBJM.MappedObject> ent in ents)
		{
			OBJM.MappedObject obj2 = ent.Item2;
			if (ent.Item1.Position == LotTilePos.OUT_OF_WORLD && obj2.ContainerID != 0 && obj2.ArryX != 0 && obj2.ArryY != 0)
			{
				Direction dir = (Direction)(1 << obj2.Direction);
				ent.Item1.SetPosition(LotTilePos.FromBigTile((short)obj2.ArryX, (short)obj2.ArryY, (sbyte)obj2.ArryLevel), dir, VM.Context);
			}
		}
		List<VMEntity> entClone = new List<VMEntity>(VM.Entities);
		foreach (VMEntity nobj in entClone)
		{
			nobj.ExecuteEntryPoint(11, VM.Context, runImmediately: true);
		}
		arch.SignalRedraw();
		VM.Context.World?.InitBlueprint(Blueprint);
		arch.Tick();
		return Blueprint;
	}

	private bool[] ResizeFlags(byte[] flags, int size)
	{
		if (size >= 64)
		{
			return flags.Select((byte x) => (x & 0x20) == 0).ToArray();
		}
		bool[] result = new bool[size * size];
		int iS = 0;
		int iD = 0;
		for (int y = 0; y < 64; y++)
		{
			if (y >= size)
			{
				return result;
			}
			for (int x2 = 0; x2 < 64; x2++)
			{
				if (x2 < size)
				{
					result[iD++] = (flags[iS] & 0x20) == 0;
				}
				iS++;
			}
		}
		return result;
	}

	private FloorTile[] ResizeFloors(FloorTile[] floors, int size)
	{
		if (size >= 64)
		{
			return floors;
		}
		FloorTile[] result = new FloorTile[size * size];
		int iS = 0;
		int iD = 0;
		for (int y = 0; y < 64; y++)
		{
			if (y >= size)
			{
				return result;
			}
			for (int x = 0; x < 64; x++)
			{
				if (x < size)
				{
					result[iD++] = floors[iS];
				}
				iS++;
			}
		}
		return result;
	}

	private WallTile[] ResizeWalls(WallTile[] walls, int size)
	{
		if (size >= 64)
		{
			return walls;
		}
		WallTile[] result = new WallTile[size * size];
		int iS = 0;
		int iD = 0;
		for (int y = 0; y < 64; y++)
		{
			if (y >= size)
			{
				return result;
			}
			for (int x = 0; x < 64; x++)
			{
				if (x < size)
				{
					result[iD++] = walls[iS];
				}
				iS++;
			}
		}
		return result;
	}

	private FloorTile[] RemapFloors(FloorTile[] floors, Dictionary<byte, ushort> dict, byte[] flags)
	{
		for (int i = 0; i < floors.Length; i++)
		{
			FloorTile floor = floors[i];
			if (FlipRoad)
			{
				if (floor.Pattern == 10)
				{
					floors[i].Pattern = 11;
				}
				else if (floor.Pattern == 11)
				{
					floors[i].Pattern = 10;
				}
			}
			if (((floor.Pattern != 0 && (flags[i] & 0x20) == 0) || floor.Pattern > 30) && dict.TryGetValue((byte)floor.Pattern, out var newID))
			{
				floors[i].Pattern = newID;
			}
		}
		return floors;
	}

	private WallTile[] RemapWalls(WallTile[] walls, Dictionary<byte, ushort> dict, Dictionary<byte, ushort> floorDict)
	{
		for (int i = 0; i < walls.Length; i++)
		{
			WallTile wall = walls[i];
			if (wall.BottomLeftPattern != 0 && dict.TryGetValue((byte)wall.BottomLeftPattern, out var newID))
			{
				walls[i].BottomLeftPattern = newID;
			}
			if (wall.BottomRightPattern != 0 && dict.TryGetValue((byte)wall.BottomRightPattern, out newID))
			{
				walls[i].BottomRightPattern = newID;
			}
			if ((wall.Segments & WallSegments.AnyDiag) > (WallSegments)0)
			{
				if (wall.TopLeftPattern != 0 && floorDict.TryGetValue((byte)wall.TopLeftPattern, out newID))
				{
					walls[i].TopLeftPattern = newID;
				}
				if (wall.TopLeftStyle != 0 && floorDict.TryGetValue((byte)wall.TopLeftStyle, out newID))
				{
					walls[i].TopLeftStyle = newID;
				}
			}
			else
			{
				if (wall.TopLeftPattern != 0 && dict.TryGetValue((byte)wall.TopLeftPattern, out newID))
				{
					walls[i].TopLeftPattern = newID;
				}
				if (wall.TopRightPattern != 0 && dict.TryGetValue((byte)wall.TopRightPattern, out newID))
				{
					walls[i].TopRightPattern = newID;
				}
			}
		}
		return walls;
	}

	public FloorTile[] DecodeFloors(byte[] data)
	{
		return data.Select(delegate(byte x)
		{
			FloorTile result = default(FloorTile);
			result.Pattern = x;
			return result;
		}).ToArray();
	}

	public FloorTile[] DecodeAdvFloors(byte[] data)
	{
		int j = 0;
		FloorTile[] result = new FloorTile[data.Length / 2];
		for (int i = 0; i < data.Length; i += 2)
		{
			result[j++] = new FloorTile
			{
				Pattern = (ushort)(data[i] | (data[i + 1] << 8))
			};
		}
		return result;
	}

	public ushort[] DecodeObjID(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		int j = 0;
		ushort[] result = new ushort[data.Length / 2];
		for (int i = 0; i < data.Length; i += 2)
		{
			result[j++] = (ushort)(data[i] | (data[i + 1] << 8));
		}
		return result;
	}

	public WallTile[] DecodeWalls(byte[] data)
	{
		int j = 0;
		WallTile[] result = new WallTile[data.Length / 8];
		for (int i = 0; i < data.Length; i += 8)
		{
			WallTile wallTile = default(WallTile);
			wallTile.Segments = (WallSegments)data[i];
			wallTile.TopLeftStyle = data[i + 2];
			wallTile.TopRightStyle = data[i + 3];
			wallTile.TopLeftPattern = data[i + 4];
			wallTile.TopRightPattern = data[i + 5];
			wallTile.BottomLeftPattern = data[i + 6];
			wallTile.BottomRightPattern = data[i + 7];
			WallTile tile = wallTile;
			if ((tile.Segments & WallSegments.AnyDiag) == 0)
			{
				if (!ValidWallStyles.Contains(tile.TopLeftStyle))
				{
					tile.TopLeftStyle = 1;
				}
				if (!ValidWallStyles.Contains(tile.TopRightStyle))
				{
					tile.TopRightStyle = 1;
				}
				if ((tile.Segments & WallSegments.TopLeft) == 0)
				{
					tile.TopLeftStyle = 0;
				}
				if ((tile.Segments & WallSegments.TopRight) == 0)
				{
					tile.TopRightStyle = 0;
				}
			}
			result[j++] = tile;
		}
		return result;
	}

	public WallTile[] DecodeAdvWalls(byte[] data)
	{
		int j = 0;
		WallTile[] result = new WallTile[data.Length / 14];
		for (int i = 0; i < data.Length; i += 14)
		{
			WallTile wallTile = default(WallTile);
			wallTile.Segments = (WallSegments)data[i];
			wallTile.TopLeftStyle = (ushort)(data[i + 2] | (data[i + 3] << 8));
			wallTile.TopRightStyle = (ushort)(data[i + 4] | (data[i + 5] << 8));
			wallTile.TopLeftPattern = (ushort)(data[i + 6] | (data[i + 7] << 8));
			wallTile.TopRightPattern = (ushort)(data[i + 8] | (data[i + 9] << 8));
			wallTile.BottomLeftPattern = (ushort)(data[i + 10] | (data[i + 11] << 8));
			wallTile.BottomRightPattern = (ushort)(data[i + 12] | (data[i + 13] << 8));
			WallTile tile = wallTile;
			if ((tile.Segments & WallSegments.AnyDiag) == 0)
			{
				if (!ValidWallStyles.Contains(tile.TopLeftStyle))
				{
					tile.TopLeftStyle = 1;
				}
				if (!ValidWallStyles.Contains(tile.TopRightStyle))
				{
					tile.TopRightStyle = 1;
				}
				if ((tile.Segments & WallSegments.TopLeft) == 0)
				{
					tile.TopLeftStyle = 0;
				}
				if ((tile.Segments & WallSegments.TopRight) == 0)
				{
					tile.TopRightStyle = 0;
				}
			}
			result[j++] = tile;
		}
		return result;
	}

	private Dictionary<byte, ushort> BuildFloorDict(List<WALmEntry> entries)
	{
		Dictionary<string, ushort> c = Content.Get().WorldFloors.DynamicFloorFromID;
		Dictionary<byte, ushort> result = new Dictionary<byte, ushort>();
		foreach (WALmEntry entry in entries)
		{
			if (c.TryGetValue(new string(entry.Name.TakeWhile((char x) => x != '.').ToArray()).ToLowerInvariant(), out var newID))
			{
				result[entry.ID] = newID;
			}
		}
		return result;
	}

	private Dictionary<byte, ushort> BuildWallDict(List<WALmEntry> entries)
	{
		Dictionary<string, ushort> c = Content.Get().WorldWalls.DynamicWallFromID;
		Dictionary<byte, ushort> result = new Dictionary<byte, ushort>();
		foreach (WALmEntry entry in entries)
		{
			if (c.TryGetValue(new string(entry.Name.TakeWhile((char x) => x != '.').ToArray()).ToLowerInvariant(), out var newID))
			{
				result[entry.ID] = newID;
			}
		}
		return result;
	}

	private TerrainComponent CreateTerrain(XmlHouseData model)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		TerrainComponent terrain = new TerrainComponent(new Rectangle(1, 1, model.Size - 2, model.Size - 2), Blueprint);
		InitWorldComponent(terrain);
		return terrain;
	}

	private TerrainComponent CreateTerrain(short Size)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		TerrainComponent terrain = new TerrainComponent(new Rectangle(1, 1, Size - 2, Size - 2), Blueprint);
		InitWorldComponent(terrain);
		return terrain;
	}

	public VMAvatar CreateAvatar()
	{
		return (VMAvatar)VM.Context.CreateObjectInstance(VMAvatar.TEMPLATE_PERSON, LotTilePos.OUT_OF_WORLD, Direction.NORTH, ts1: false).Objects[0];
	}

	public VMAvatar CreateAvatar(uint guid, XmlCharacter xml, bool visitor, short id)
	{
		VMAvatar avatar = (VMAvatar)VM.Context.CreateObjectInstance(guid, LotTilePos.OUT_OF_WORLD, Direction.NORTH, ts1: false).Objects[0];
		avatar.Visitor = visitor;
		VMEntity mailbox = VM.Entities.First((VMEntity x) => x.Object.OBJ.GUID == 4010940788u || x.Object.OBJ.GUID == 496355760 || x.Object.OBJ.GUID == 1961361321 || x.Object.OBJ.GUID == 2254071826u);
		avatar.SetAvatarData(xml);
		LotTilePos pos = mailbox.Position;
		pos.x = (short)(mailbox.Position.x + 1);
		pos.y = (short)(mailbox.Position.y + id);
		avatar.SetPosition(pos, Direction.WEST, VM.Context);
		return avatar;
	}

	public void CreateObject(XmlHouseDataObject obj)
	{
		LotTilePos pos = ((obj.Level == 0) ? LotTilePos.OUT_OF_WORLD : LotTilePos.FromBigTile((short)obj.X, (short)obj.Y, (sbyte)obj.Level));
		VMMultitileGroup mojb = VM.Context.CreateObjectInstance(obj.GUIDInt, pos, obj.Direction, ts1: false);
		if (mojb == null)
		{
			return;
		}
		VMEntity nobj = mojb.Objects[0];
		if (obj.Group != 0)
		{
			foreach (VMEntity sub in nobj.MultitileGroup.Objects)
			{
				sub.SetValue(VMStackObjectVariable.GroupID, (short)obj.Group);
			}
		}
		for (int i = 0; i < nobj.MultitileGroup.Objects.Count; i++)
		{
			nobj.MultitileGroup.Objects[i].ExecuteEntryPoint(11, VM.Context, runImmediately: true);
		}
	}

	private void InitWorldComponent(WorldComponent component)
	{
		component.Initialize(World.State.Device, World.State);
	}
}
