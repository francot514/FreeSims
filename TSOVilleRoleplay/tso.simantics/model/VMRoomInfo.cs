using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.SimAntics.Routing;
using Microsoft.Xna.Framework;
using tso.world.model;

namespace TSO.Simantics.model
{
    public struct VMRoomInfo
    {
        public List<VMRoomPortal> Portals;
        public List<VMEntity> Entities;
        public VMRoom Room;

    }

    public struct VMRoom
    {
        public ushort RoomID;
        public ushort AmbientLight;
        public bool IsOutside;
        public ushort Area;
        public bool IsPool;
        public bool Unroutable;

        public List<VMObstacle> WallObs;
        public List<VMObstacle> RoomObs;
        public Rectangle Bounds;
    }

    public class VMRoomPortal {
        public short ObjectID;
        public ushort TargetRoom;

        public VMRoomPortal(short obj, ushort target)
        {
            ObjectID = obj;
            TargetRoom = target;
        }
    }
}
