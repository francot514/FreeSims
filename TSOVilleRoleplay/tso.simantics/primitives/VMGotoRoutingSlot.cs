using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Files.utils;
using TSO.Simantics.engine.utils;
using TSO.Simantics.engine.scopes;
using Microsoft.Xna.Framework;
using tso.world.model;

namespace TSO.Simantics.engine.primitives
{
    public class VMGotoRoutingSlot : VMPrimitiveHandler {
        public override VMPrimitiveExitCode Execute(VMStackFrame context)
        {
            var operand = context.GetCurrentOperand<VMGotoRoutingSlotOperand>();
            
            var slot = VMMemory.GetSlot(context, operand.Type, operand.Data);
            var obj = context.StackObject;
            var avatar = context.Caller;

            //Routing slots must be type 3.
            if (slot.Type == 3)
            {
                var pathFinder = context.Thread.PushNewRoutingFrame(context, !operand.NoFailureTrees);
                var success = pathFinder.InitRoutes(slot, context.StackObject);

                return VMPrimitiveExitCode.CONTINUE;
            }
            else return VMPrimitiveExitCode.GOTO_FALSE;
        }
    }

    public class VMGotoRoutingSlotOperand : VMPrimitiveOperand
    {
        public ushort Data;
        public VMSlotScope Type;
        public byte Flags;
        public bool NoFailureTrees
        {
            get
            {
                return (Flags & 1) > 0;
            }
        }

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes){
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN)){
                Data = io.ReadUInt16();
                Type = (VMSlotScope)io.ReadUInt16();
                Flags = io.ReadByte();
            }
        }
        #endregion

        public override string ToString()
        {
            return "Go To Routing Slot (" + Type.ToString() + ": #" + Data + ")";
        }
    }

}
