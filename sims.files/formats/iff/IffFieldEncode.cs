﻿using FSO.Files.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Files.Formats.IFF.Chunks
{
    /// <summary>
    /// Used to read values from field encoded stream.
    /// </summary>
    public class IffFieldEncode : IOProxy
    {
        private byte bitPos = 0;
        private byte curByte = 0;
        private bool odd = false;
        public byte[] widths = { 5, 8, 13, 16 };
        public byte[] widths2 = { 6, 11, 21, 32 };
        public byte[] widthsByte = { 2, 4, 6, 8 };

        public void setBytePos(int n)
        {
            io.Seek(SeekOrigin.Begin, n);
            curByte = io.ReadByte();
            bitPos = 0;
        }

        public override ushort ReadUInt16()
        {
            return (ushort)ReadField(false);
        }

        public override short ReadInt16()
        {
            return (short)ReadField(false);
        }

        public override int ReadInt32()
        {
            return (int)ReadField(true);
        }

        public override uint ReadUInt32()
        {
            return (uint)ReadField(true);
        }

        public override float ReadFloat()
        {
            return (float)ReadField(true);
            //this is incredibly wrong
        }

        public byte ReadByte()
        {
            return (byte)ReadField(widthsByte);
        }

        public void Interrupt()
        {
            long targetPos = io.Position;

            if (bitPos == 0)
            {
                targetPos--;
            }

            io.Seek(SeekOrigin.Begin, targetPos);
        }

        public string BitDebug(int count)
        {
            string result = "";

            for (int i = 0; i < count; i++)
            {
                var bit = ReadBit();

                result += bit == 1 ? "1" : "0";

                if (bitPos == 0)
                {
                    result += "|";
                }
            }

            return result;
        }

        public string BitDebugTil(long skipPosition)
        {
            long currentPos = bitPos == 0 ? io.Position : io.Position - 1;

            int diff = (int)(skipPosition - currentPos) * 8 - bitPos;

            if (diff < 0)
            {
                return "oob";
            }

            return BitDebug(diff);
        }

        private long ReadField(byte[] widths)
        {
            if (ReadBit() == 0) return 0;

            uint code = ReadBits(2);
            byte width = widths[code];
            long value = ReadBits(width);
            value |= -(value & (1 << (width - 1)));

            if (value == 0)
            {
                // not valid
            }

            return value;
        }

        private long ReadField(bool big)
        {
            if (ReadBit() == 0) return 0;

            uint code = ReadBits(2);
            byte width = (big) ? widths2[code] : widths[code];
            long value = ReadBits(width);
            value |= -(value & (1 << (width - 1)));

            return value;
        }

        private uint ReadBits(int n)
        {
            uint total = 0;
            for (int i = 0; i < n; i++)
            {
                total += (uint)(ReadBit() << ((n - i) - 1));
            }
            return total;
        }

        private byte ReadBit()
        {
            byte result = (byte)((curByte & (1 << (7 - bitPos))) >> (7 - bitPos));
            if (++bitPos > 7)
            {
                bitPos = 0;
                try
                {
                    curByte = io.ReadByte();
                    odd = !odd;
                }
                catch (Exception)
                {
                    curByte = 0; //no more data, read 0
                }
            }
            return result;
        }

        public string ReadString(bool nextField)
        {
            if (bitPos == 0)
            {
                io.Seek(SeekOrigin.Current, -1);
                odd = !odd;
            }
            var str = io.ReadNullTerminatedString();
            if ((str.Length % 2 == 0) == !odd) io.ReadByte(); //2 byte pad

            bitPos = 8;
            if (nextField && io.HasMore)
            {
                curByte = io.ReadByte();
                odd = true;
                bitPos = 0;
            } else
            {
                odd = false;
            }

            return str;
        }

        public IffFieldEncode(IoBuffer io) : base(io)
        {
            curByte = io.ReadByte();
            odd = !odd;
            bitPos = 0;
        }
    }
}
