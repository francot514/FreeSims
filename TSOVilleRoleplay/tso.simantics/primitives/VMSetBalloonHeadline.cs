using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Simantics.engine;
using TSO.Files.utils;
using System.IO;
using TSO.Simantics.model;

namespace TSO.Simantics.primitives
{
    public class VMSetBalloonHeadline : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context)
        {
            var operand = context.GetCurrentOperand<VMSetBalloonHeadlineOperand>();
            var obj = (operand.OfStackOBJ) ? context.StackObject : context.Caller;

            if (obj.HeadlineRenderer != null) obj.HeadlineRenderer.Dispose();

            if (operand.Index == -1 || operand.Duration == 0)
            {
                obj.Headline = null;
                obj.HeadlineRenderer = null;
            }
            else
            {
                VMEntity icon = null;
                int index = operand.Index;
                if (operand.Group == VMSetBalloonHeadlineOperandGroup.Algorithmic)
                    icon = (index < 2) ? context.StackObject : context.VM.GetObjectById((short)context.Locals[operand.Algorithmic]);
                else if (operand.Indexed)
                    index += context.Thread.TempRegisters[0];
                obj.Headline = new VMRuntimeHeadline(operand, obj, icon, (sbyte)index);
                obj.HeadlineRenderer = context.VM.Headline.Get(obj.Headline);
            }
            return VMPrimitiveExitCode.GOTO_TRUE;
        }
    }

    public class VMSetBalloonHeadlineOperand : VMPrimitiveOperand
    {
        public ushort Flags2;
        public sbyte Index;
        public VMSetBalloonHeadlineOperandGroup Group;
        public short Duration;
        public byte Type;
        public byte Flags;

        public int Algorithmic
        {
            get
            {
                return Flags2 >> 1;
            }
        }

        public bool OfStackOBJ
        {
            get
            {
                return (Flags2 & 0x1) > 0;
            }
        }

        public bool Inactive
        {
            get
            {
                return (Flags & 0x1) > 0;
            }
        }

        public bool Crossed
        {
            get
            {
                return (Flags & 0x2) > 0;
            }
        }

        public bool Backwards
        {
            get
            {
                return (Flags & 0x4) > 0;
            }
        }

        public bool DurationInLoops
        {
            get
            {
                return (Flags & 0x8) > 0;
            }
        }

        public bool Indexed
        {
            get
            {
                return (Flags & 0x10) > 0;
            }
        }

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes)
        {
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN))
            {
                Flags2 = io.ReadUInt16();
                Index = (sbyte)io.ReadByte();
                Group = (VMSetBalloonHeadlineOperandGroup)(sbyte)io.ReadByte();
                Duration = io.ReadInt16();
                Type = io.ReadByte();
                Flags = io.ReadByte();
            }
        }

        public void Write(byte[] bytes)
        {
            using (var io = new BinaryWriter(new MemoryStream(bytes)))
            {
                io.Write(Flags2);
                io.Write(Index);
                io.Write((byte)Group);
                io.Write(Duration);
                io.Write(Type);
                io.Write(Flags);
            }
        }
        #endregion
    }

    public enum VMSetBalloonHeadlineOperandGroup
    {
        OldStyle = 0,
        Balloon = 1,
        Conversation = 2,
        Motive = 3,
        Relationship = 4,
        Headline = 5,
        Debug = 6,
        Algorithmic = 7,
        RouteFailure = 8,
        Progress = 9,

    }
}
