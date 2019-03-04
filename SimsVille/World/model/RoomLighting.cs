using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FSO.LotView.Model
{
    public class RoomLighting
    {
        //TODO: point lights

        public ushort OutsideLight;
        public ushort AmbientLight;
        public short RoomScore;
        public Rectangle Bounds;
    }
}
