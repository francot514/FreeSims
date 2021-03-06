﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FSO.LotView.Model;

namespace FSO.SimAntics.Marshals
{
    public class VMGameObjectMarshal : VMEntityMarshal
    {
        public Direction Direction;
        public bool Disabled;

        public VMGameObjectMarshal() { }
        public VMGameObjectMarshal(int version) : base(version) { }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Direction = (Direction)reader.ReadByte();
        }
        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write((byte)Direction);
        }
    }
}
