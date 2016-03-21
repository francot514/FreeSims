using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace SimsLib.IFF
{
    public class SIMI : IffChunk
    {

        
        public string Header, Simh;
        public int Offset, Simhour, Day, Zoomlevel, Personid, Timeday, Minute, Second, Month, Year;
        public int m_Version, Family, House, Speed, Paused, Mode, Lotsize, Daysrunning, Lotprice, Funds, LotType;

        public int Version
        {
            get { return m_Version; }
        }

        public SIMI(IffChunk Chunk) : base(Chunk)
        {

            MemoryStream MemStream = new MemoryStream(Chunk.Data);
            BinaryReader Reader = new BinaryReader(MemStream);

            m_Version = 0;
            Reader.ReadInt32();
            m_Version = Reader.ReadInt32();


            Header = Encoding.ASCII.GetString(Reader.ReadBytes(4));
            


            Simhour = Reader.ReadInt16();
            Day = Reader.ReadInt16();
            Zoomlevel = Reader.ReadInt16();
            Personid = Reader.ReadInt16();
            Timeday = Reader.ReadInt16();
            Minute = Reader.ReadInt16();
            Second = Reader.ReadInt16();
            Month = Reader.ReadInt16();
            Year = Reader.ReadInt16();
            Family = Reader.ReadInt16();
            House = Reader.ReadInt16();
            Reader.ReadInt16(); //Unused???
            Reader.ReadInt16(); //ButtonZID
            Reader.ReadInt16(); //BudgetMod
            Reader.ReadInt16(); //BudgetDiv
            Reader.ReadInt16(); //Language
            Speed = Reader.ReadInt16();
            Paused = Reader.ReadInt16();
            Reader.ReadInt16(); //Held Sim Speed
            Mode = Reader.ReadInt16();
            Reader.ReadInt16(); //GameEdition
            Reader.ReadInt16();//Inhibits
            Reader.ReadInt16(); //LotHasHouse
            Lotsize = Reader.ReadInt16();
            Reader.ReadInt16(); //Demo??
            Reader.ReadInt16(); //Debug Flags??
            Reader.ReadInt16();//Tutorial??
            Reader.ReadInt16();//Indoor tiles??
            Daysrunning = Reader.ReadInt16();
            Lotprice = Reader.ReadInt16(); //Max Day??
            Reader.ReadInt16(); //Free will
            Reader.ReadInt16(); //House station
            Reader.ReadInt16(); //Simless Build Mode
            Reader.ReadInt16(); //MachineLevel
             Reader.ReadInt16(); //LotTransit 
            LotType = Reader.ReadInt16();
             Reader.ReadInt16(); //Grass Simulation
             Reader.ReadInt16(); //Filter Flags
        }

    }
}
