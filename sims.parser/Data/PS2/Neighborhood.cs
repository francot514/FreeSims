
using System;
using System.Collections.Generic;
using FSO.Files.Formats.IFF.Chunks;

namespace FileParser.Data.Sims
{
   public class Neighborhood
    {
        public int Offset, Chunks;
        public NGBH Main;
        public NBRS Scores;
        public TATT Tables;
        public List<FAMI> Families;

        public Neighborhood()
        {
            Families = new List<FAMI>();
            Main = new NGBH();
            Scores = new NBRS();
            Tables = new TATT();

        }

    }
}
