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

        public override bool Execute(VM vm)
        {

            string[] CharacterInfos = new string[9];
            List<XmlCharacter> Characters = new List<XmlCharacter>();
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


            var DirectoryInfo = new DirectoryInfo(Path.Combine(FSOEnvironment.UserDir, "Characters/"));
            

            if (vm.IsServer)
               {  
                

                for (int i = 0; i <= DirectoryInfo.GetFiles().Count() - 1; i++)
                {

                    var file = DirectoryInfo.GetFiles()[i];
                    CharacterInfos[i] = file.FullName;
                    Characters.Add(XmlCharacter.Parse(file.FullName));

                    VMAvatar visitor = vm.Activator.CreateAvatar(Convert.ToUInt32(Characters[i].ObjID, 16), Characters[i], true, (short)i);

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
