using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace tso.world.Model
{
    public class Room
    {
        public ushort RoomID;
        public bool IsOutside;
        public ushort Area;
        public bool IsPool;
        public Rectangle Bounds;

        public Room(ushort id, bool outside, Rectangle bounds, ushort area)
        {

            RoomID = id;
            IsOutside = outside;
            Bounds = bounds;
            Area = area;

        }
    }
}
