using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tso.world.model;
using Microsoft.Xna.Framework;
using tso.world.components;
using TSO.Simantics.model;

namespace TSO.Simantics.entities
{
    /// <summary>
    /// Ties multiple entities together with a common name and set of repositioning functions.
    /// </summary>
    public class VMMultitileGroup
    {
        public bool MultiTile;
        public List<VMEntity> Objects = new List<VMEntity>();
        public List<LotTilePos> Offsets = new List<LotTilePos>();

        public VMEntity BaseObject
        {
            get
            {
                for (int i = 0; i < Objects.Count(); i++)
                {
                    var sub = Objects[i];
                    if (sub.Object.OBJ.MyLeadObject > 0) return sub;
                }

                for (int i = 0; i < Objects.Count(); i++)
                {
                    var sub = Objects[i];
                    if ((((ushort)sub.Object.OBJ.SubIndex) >> 8) == 0 && (((ushort)sub.Object.OBJ.SubIndex) & 0xFF) == 0 && sub.Object.OBJ.LevelOffset == 0) return sub;
                }
                return Objects[0];
            }
        }

        public Vector3[] GetBasePositions()
        {
            Vector3[] positions = new Vector3[Objects.Count];
            for (int i = 0; i < Objects.Count(); i++)
            {
                ushort sub = (ushort)Objects[i].Object.OBJ.SubIndex;
                positions[i] = new Vector3(Offsets[i].x / 16, Offsets[i].y / 16, 0);
            }
            return positions;
        }

        public void AddDynamicObject(VMEntity obj, LotTilePos offset)
        {
            Objects.Add(obj);
            Offsets.Add(offset);
        }

        public void AddObject(VMEntity obj)
        {
            AddDynamicObject(obj,
                new LotTilePos((short)((sbyte)(((ushort)obj.Object.OBJ.SubIndex) >> 8) * 16),
                (short)((sbyte)(((ushort)obj.Object.OBJ.SubIndex) & 0xFF) * 16),
                (sbyte)obj.Object.OBJ.LevelOffset));
        }

        public VMPlacementResult ChangePosition(LotTilePos pos, Direction direction, VMContext context)
        {
            if (pos.Level > context.Architecture.Stories) return new VMPlacementResult(VMPlacementError.NotAllowedOnFloor);

            VMEntity[] OldContainers = new VMEntity[Objects.Count];
            short[] OldSlotNum = new short[Objects.Count];
            for (int i = 0; i < Objects.Count(); i++)
            {
                OldContainers[i] = Objects[i].Container;
                OldSlotNum[i] = Objects[i].ContainerSlot;
                Objects[i].PrePositionChange(context);
            }

            int Dir = 0;
            switch (direction)
            {
                case Direction.NORTH:
                    Dir = 0; break;
                case Direction.EAST:
                    Dir = 2; break;
                case Direction.SOUTH:
                    Dir = 4; break;
                case Direction.WEST:
                    Dir = 6; break;
            }

            Matrix rotMat = Matrix.CreateRotationZ((float)(Dir * Math.PI / 4.0));
            VMPlacementResult[] places = new VMPlacementResult[Objects.Count];

            var bObj = BaseObject;
            var bOff = Offsets[Objects.IndexOf(BaseObject)];
            var leadOff = new Vector3(bOff.x, bOff.y, 0);

            //TODO: optimize so we don't have to recalculate all of this
            if (pos != LotTilePos.OUT_OF_WORLD)
            {
                for (int i = 0; i < Objects.Count(); i++)
                {
                    var sub = Objects[i];
                    var off = new Vector3(Offsets[i].x, Offsets[i].y, 0);
                    off = Vector3.Transform(off - leadOff, rotMat);

                    var offPos = new LotTilePos((short)Math.Round(pos.x + off.X), (short)Math.Round(pos.y + off.Y), (sbyte)(pos.Level + Offsets[i].Level));
                    places[i] = sub.PositionValid(offPos, direction, context);
                    if (places[i].Status != VMPlacementError.Success)
                    {
                        //go back to where we started: we're no longer out of world.
                        for (int j = 0; j < Objects.Count(); j++)
                        {
                            //need to restore slot we were in
                            if (OldContainers[j] != null)
                            {
                                OldContainers[j].PlaceInSlot(Objects[j], OldSlotNum[j], false, context);
                            }
                            Objects[j].PositionChange(context);
                        }
                        return places[i];
                    }
                }
            }

            //verification success

            for (int i = 0; i < Objects.Count(); i++)
            {
                var sub = Objects[i];
                var off = new Vector3(Offsets[i].x, Offsets[i].y, 0);
                off = Vector3.Transform(off - leadOff, rotMat);

                var offPos = (pos == LotTilePos.OUT_OF_WORLD) ?
                    LotTilePos.OUT_OF_WORLD :
                    new LotTilePos((short)Math.Round(pos.x + off.X), (short)Math.Round(pos.y + off.Y), (sbyte)(pos.Level + Offsets[i].Level));

                sub.SetIndivPosition(offPos, direction, context, places[i]);
            }
            for (int i = 0; i < Objects.Count(); i++) Objects[i].PositionChange(context);
            return new VMPlacementResult(VMPlacementError.Success);
        }

        public void SetVisualPosition(Vector3 pos, Direction direction, VMContext context)
        {
            int Dir = 0;
            switch (direction)
            {
                case Direction.NORTH:
                    Dir = 0; break;
                case Direction.EAST:
                    Dir = 2; break;
                case Direction.SOUTH:
                    Dir = 4; break;
                case Direction.WEST:
                    Dir = 6; break;
            }

            Matrix rotMat = Matrix.CreateRotationZ((float)(Dir * Math.PI / 4.0));
            var bObj = BaseObject;
            var bOff = Offsets[Objects.IndexOf(BaseObject)];
            var leadOff = new Vector3(bOff.x, bOff.y, 0);

            for (int i = 0; i < Objects.Count(); i++)
            {
                var sub = Objects[i];
                var off = new Vector3(Offsets[i].x / 16f, Offsets[i].y / 16f, sub.Object.OBJ.LevelOffset * 2.95f);
                off = Vector3.Transform(off - leadOff, rotMat);

                sub.Direction = direction;
                sub.VisualPosition = pos + off;
            }
        }

        public void Delete(VMContext context)
        {
            for (int i = 0; i < Objects.Count(); i++)
            {
                var obj = Objects[i];
                obj.PrePositionChange(context);
                context.RemoveObjectInstance(obj);
            }
        }

        public void Init(VMContext context)
        {
            for (int i = 0; i < Objects.Count(); i++)
            {
                Objects[i].Init(context);
            }
        }
    }
}
