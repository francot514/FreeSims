using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileParser.Data.Sims;
using System.IO;
using FSO.Files.Formats.IFF.Chunks;

namespace FileParser.Data
{
  public class Game
    {
        
        public string Name;
        public Disc Disc;
        public NGHFile DefaultNgh;

        public Game(string name)
        {

            Name = name;

            GetDisc();

        }

        public void GetDisc()
        {

        }

        public void ParseNGH(string file)
        {

            DefaultNgh = new NGHFile(file);

            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {

                DefaultNgh.Header = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.StringsOffset = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.Id = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.ExtraOffset = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.Neighborhood = new Neighborhood();
                DefaultNgh.Neighborhood.Offset = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.Neighborhood.Chunks = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                for (int i = 0; i < 9; i++)
                {

                    DefaultNgh.Houses.Add(new House(i)
                    {
                        Offset = BitConverter.ToInt32(reader.ReadBytes(4), 0),
                        Chunks = BitConverter.ToInt32(reader.ReadBytes(4), 0)

                });

                }

                int count = DefaultNgh.Neighborhood.Chunks - 3;

                DefaultNgh.Neighborhood.Main.ChunkLabel = Encoding.ASCII.GetString(reader.ReadBytes(4));
                DefaultNgh.Neighborhood.Scores.ChunkLabel = Encoding.ASCII.GetString(reader.ReadBytes(4));
                DefaultNgh.Neighborhood.Tables.ChunkLabel = Encoding.ASCII.GetString(reader.ReadBytes(4));

                if (count > 0)
                        for (int i = 0; i < count; i++)
                        {
                            DefaultNgh.Neighborhood.Families.Add(new FAMI()
                            {
                                ChunkLabel = Encoding.ASCII.GetString(reader.ReadBytes(4))
                                


                            });

                        }

                DefaultNgh.Neighborhood.Main.Offset = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.Neighborhood.Scores.Offset = BitConverter.ToInt32(reader.ReadBytes(4), 0);
                DefaultNgh.Neighborhood.Tables.Offset = BitConverter.ToInt32(reader.ReadBytes(4), 0);

                if (count > 0)
                    for (int i = 0; i < count; i++)
                    {
                       DefaultNgh.Neighborhood.Families[i].Offset = BitConverter.ToInt32(reader.ReadBytes(4), 0);


                    }

            }

        }

    }
}
