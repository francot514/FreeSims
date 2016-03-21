using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimsLib.IFF
{


   public class ARRY: IffChunk
    {
       private int m_Version;

       public int data1, data2, val1, val2;
       public ushort value1, value2, value3;

       public string Header;

       public int Version
       {
           get { return m_Version; }
       }


        public ARRY(IffChunk Chunk) : base(Chunk)
        {

        MemoryStream MemStream = new MemoryStream(Chunk.Data);
            BinaryReader Reader = new BinaryReader(MemStream);

            Reader.ReadInt32();
            m_Version = Reader.ReadInt32();
            Header = Encoding.ASCII.GetString(Reader.ReadBytes(4));
            data1 = Reader.ReadInt32();
            data2 = Reader.ReadInt32();
            value1 = Reader.ReadUInt16();
            value2 = Reader.ReadUInt16();

        }
    }
}
