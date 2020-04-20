using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSO.SimAntics.Model.TSOPlatform
{
    public class VMTSOObjectState : VMTSOEntityState
    {
        //TODO: repair
        public uint OwnerID;
        public VMTSOObjectFlags ObjectFlags;

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            OwnerID = reader.ReadUInt32();
        }

        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write(OwnerID);
        }

        public override void Tick(VM vm, object owner)
        {
            base.Tick(vm, owner);
        }

        public void Donate(VM vm, VMEntity owner)
        {
            //remove all sellback value and set it as donated.
            owner.MultitileGroup.Price = 0;
            foreach (var obj in owner.MultitileGroup.Objects)
            {
                (obj.TSOState as VMTSOObjectState).ObjectFlags |= VMTSOObjectFlags.FSODonated;
            }
            VMBuildableAreaInfo.UpdateOverbudgetObjects(vm);
        }
    }

    public enum VMTSOObjectFlags : byte
    {
        FSODonated = 1
    }
}
