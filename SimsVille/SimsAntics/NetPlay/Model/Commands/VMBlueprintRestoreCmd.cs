﻿/*
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
using FSO.Vitaboy;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    public class VMBlueprintRestoreCmd : VMNetCommandBodyAbstract
    {
        public byte[] XMLData;
        public short JobLevel = -1;
        public List<XmlCharacter> Characters;


        public override bool Execute(VM vm)
        {

            string[] CharacterInfos = new string[9];
            
            XmlHouseData lotInfo;
            using (var stream = new MemoryStream(XMLData))
            {
                lotInfo = XmlHouseData.Parse(stream);
            }

            VMWorldActivator activator = new VMWorldActivator(vm, vm.Context.World);

            vm.Activator = activator;
            
            var blueprint = activator.LoadFromXML(lotInfo);

            if (VM.UseWorld)
            {
                vm.Context.World.InitBlueprint(blueprint);
                vm.Context.Blueprint = blueprint;
            }
            vm.SetGlobalValue(11, JobLevel);

            AppearanceType type;

            foreach (XmlCharacter Char in Characters)
            {
                uint vsimID = (uint)(new Random()).Next();
                Enum.TryParse(Char.Appearance, out type);

                var vheadPurchasable = Content.Content.Get().AvatarPurchasables.Get(Convert.ToUInt64(Char.Head, 16));
                var vbodyPurchasable = Content.Content.Get().AvatarPurchasables.Get(Convert.ToUInt64(Char.Body, 16));
                var vHeadID = vheadPurchasable != null ? vheadPurchasable.OutfitID :
                    Convert.ToUInt64(Char.Head, 16);
                var vBodyID = vbodyPurchasable != null ? vbodyPurchasable.OutfitID :
                    Convert.ToUInt64(Char.Body, 16);

                VMAvatar visitor = vm.Activator.CreateAvatar
                    (Convert.ToUInt32(Char.ObjID, 16), Char, true, Convert.ToInt16(Char.Id));

                if (!vm.Entities.Contains(visitor))
                    vm.SendCommand(new VMNetVisitorCmd
                    {
                        ActorUID = vsimID,
                        HeadID = vHeadID,
                        BodyID = vBodyID,
                        SkinTone = (byte)type,
                        Gender = Char.Gender == "male" ? true : false,
                        Name = Char.Name

                    });
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
