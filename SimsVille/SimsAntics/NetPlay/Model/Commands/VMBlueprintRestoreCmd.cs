/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FSO.LotView.Model;
using FSO.SimAntics.Utils;
using tso.world.Model;
using FSO.Common;
using FSO.SimAntics.Primitives;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    public class VMBlueprintRestoreCmd : VMNetCommandBodyAbstract
    {
        public byte[] XMLData;
        public short JobLevel = -1;
        public List<XmlCharacter> Characters;
        public XmlCharacter ActiveChar;

        public override bool Execute(VM vm)
        {

            string[] CharacterInfos = new string[9];
            
            XmlHouseData lotInfo;
            using (var stream = new MemoryStream(XMLData))
            {
                lotInfo = XmlHouseData.Parse(stream);
            }

            vm.Activator = new VMWorldActivator(vm, vm.Context.World);
            
            var blueprint = vm.Activator.LoadFromXML(lotInfo);

            if (VM.UseWorld)
            {
                vm.Context.World.InitBlueprint(blueprint);
                vm.Context.Blueprint = blueprint;
            }
            vm.SetGlobalValue(11, JobLevel);


            if (vm.IsServer)
            {



                foreach (XmlCharacter Char in Characters)
                {
                    VMAvatar visitor = vm.Activator.CreateAvatar
                        (Convert.ToUInt32(Char.ObjID, 16), Char, true, Convert.ToInt16(Char.Id));

                    if (!vm.Entities.Contains(visitor))
                        vm.Entities.Add(visitor);

                }
            }
            


            return true;
        }

        #region VMSerializable Members

        public override void SerializeInto(BinaryWriter writer)
        {
            if (XMLData == null) writer.Write(0);
            else
            {
                writer.Write(XMLData.Length);
                writer.Write(XMLData);
                writer.Write(JobLevel);
            }
        }

        public override void Deserialize(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            XMLData = reader.ReadBytes(length);
            JobLevel = reader.ReadInt16();
        }

        #endregion
    }
}
