using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Simantics.engine;
using TSO.Files.utils;
using TSO.Files.formats.iff.chunks;

namespace TSO.Simantics.primitives
{
    public class VMChangeActionString : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context)
        {
            var operand = context.GetCurrentOperand<VMChangeActionStringOperand>();
            STR table = null;
            switch (operand.Scope)
            {
                case 0:
                    table = context.ScopeResource.Get<STR>(operand.StringTable);
                    break;
                case 1:
                    table = context.Callee.SemiGlobal.Resource.Get<STR>(operand.StringTable);
                    break;
                case 2:
                    table = context.Global.Resource.Get<STR>(operand.StringTable);
                    break;
            }

            if (table != null)
            {
                var newName = VMDialogHandler.ParseDialogString(context, table.GetString(operand.StringID - 1), table);
                if (context.Thread.IsCheck && context.Thread.ActionStrings != null)
                {
                    context.Thread.ActionStrings.Add(new VMPieMenuInteraction()
                    {
                        Name = newName,
                        Param0 = (context.StackObject == null) ? (short)0 : context.StackObject.ObjectID
                    });
                }
                else
                    context.Thread.Queue[0].Name = newName;
            }
            return VMPrimitiveExitCode.GOTO_TRUE;
        }
    }

    public class VMChangeActionStringOperand : VMPrimitiveOperand
    {
        public ushort StringTable;
        public ushort Scope;
        public byte StringID;

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes)
        {
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN))
            {
                StringTable = io.ReadUInt16();
                Scope = io.ReadUInt16();
                StringID = io.ReadByte();
            }
        }
        #endregion
    }
}
