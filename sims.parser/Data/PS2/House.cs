using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Files.Formats.IFF.Chunks;

namespace FileParser.Data.Sims
{
   public class House
    {
        public int ID, Offset, Chunks;
        public string Name;
        public ARRY[] Arrys;
        public OBJM ObjectMap;
        public OBJT ObjectTable;
        public SIMI SimInfo;
        public HOUS HousesScore;

        public House(int id)
        {

            ID = id;

        }
    }

}
