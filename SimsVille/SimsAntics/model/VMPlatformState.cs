using FSO.SimAntics.NetPlay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FSO.SimAntics.Model.Platform;

namespace FSO.SimAntics.Model
{
    public abstract class VMPlatformState : VMSerializable
    {
        public VMAbstractValidator Validator;
        public abstract bool LimitExceeded { get; set; }

        public virtual bool CanPlaceNewUserObject(VM vm)
        {
            return true;
        }

        public virtual bool CanPlaceNewDonatedObject(VM vm)
        {
            return false;
        }

        public abstract void ActivateValidator(VM vm);

        public abstract void Deserialize(BinaryReader reader);
        public abstract void SerializeInto(BinaryWriter writer);
        public abstract void Tick(VM vm, object owner);

       
    }
}
