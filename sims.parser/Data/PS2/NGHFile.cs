using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileParser.Data.Sims
{
   public class NGHFile
    {
        public int Header, StringsOffset, Id, ExtraOffset;

        public string Name;

        public Neighborhood Neighborhood;

        public List<House> Houses;

        public NGHFile(string name)
        {

            Name = name;
            Houses = new List<House>();
            Neighborhood = new Neighborhood();


        }
    }

}
