using FSO.SimAntics.Model.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics.Primitives;
using FSO.SimAntics.Utils;

namespace FSO.SimAntics.Model.TS1Platform
{
    public class VMTS1LotState : VMPlatformState
    {
        public SIMI SimulationInfo;
        public FAMI CurrentFamily;

        public override bool LimitExceeded { get; set; }

        public VMTS1LotState() : base() { }
        public VMTS1LotState(int version) : base(version) { }

        public void ActivateFamily(VM vm, FAMI family)
        {
            if (family == null) return;
            vm.SetGlobalValue(9, (short)family.ChunkID);
            CurrentFamily = family;
        }

        /// <summary>
        /// Ensure all members of the family are present on the lot.
        /// Spawns missing family members at the mailbox.
        /// </summary>
        public void VerifyFamily(VM vm)
        {
            if (CurrentFamily == null)
            {
                vm.SetGlobalValue(32, 1);
                return;
            }
            vm.SetGlobalValue(32, 0);
            vm.SetGlobalValue(9, (short)CurrentFamily.ChunkID);
            var missingMembers = new HashSet<uint>(CurrentFamily.RuntimeSubset);
            foreach (var avatar in vm.Context.ObjectQueries.Avatars)
            {
                missingMembers.Remove(avatar.Object.OBJ.GUID);
            }


        }


        public override void Deserialize(BinaryReader reader)
        {
            if (reader.ReadBoolean())
            {
                SimulationInfo = new SIMI() { ChunkID = 1, ChunkLabel = "", ChunkType = "SIMI" };
                SimulationInfo.Read(null, reader.BaseStream);
            }

            //this is really only here for future networking. families should be activated (see abover) when joining lots for the first time
            var famID = reader.ReadUInt16(); 
            if (famID < 65535)
            {
                CurrentFamily = new FAMI() { ChunkID = famID, ChunkLabel = "", ChunkType = "FAMI" };
                CurrentFamily.Read(null, reader.BaseStream);
            }
        }

        public override void SerializeInto(BinaryWriter writer)
        {
            writer.Write(SimulationInfo != null);
            SimulationInfo?.Write(null, writer.BaseStream);
            writer.Write(CurrentFamily?.ChunkID ?? 65535);
            if (CurrentFamily != null) CurrentFamily.Write(null, writer.BaseStream);
        }

        public override void Tick(VM vm, object owner)
        {
        }

        public void UpdateSIMI(VM vm)
        {
            if (SimulationInfo == null) return;
            
        }

        public override void ActivateValidator(VM vm)
        {
            Validator = new VMDefaultValidator(vm);
        }
    }
}
