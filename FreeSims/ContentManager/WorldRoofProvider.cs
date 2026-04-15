using FSO.Common;
using FSO.Content.Codecs;
using FSO.Content.Framework;
using FSO.Content.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FSO.Content
{
    public class WorldRoofProvider : FileProvider<ITextureRef>
    {
        public string RoofsPath = "housedata" + Path.DirectorySeparatorChar +
                "Roofs";

        public List<string> Roofs;

        public WorldRoofProvider(Content contentManager) : base(contentManager, new TextureCodec(), new string[2])
        {

            Roofs = new List<string>();
            
            if (Directory.Exists(Path.Combine(contentManager.BasePath, RoofsPath)))
            {
               DirectoryInfo roofsDir = new DirectoryInfo(Path.Combine(contentManager.BasePath, RoofsPath));

                if (roofsDir.GetFiles().Count() > 0)
                {

                foreach (var file in roofsDir.GetFiles())
                   Roofs.Add(Path.Combine(RoofsPath, file.Name));

                }

            }


            base.Files = Roofs.ToArray();
        }

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }



public string IDToName(int id)
        {

            if (id < 0)
                id = 0;

            return Items[id].Name;
        }

        public int NameToID(string name)
        {
            return Items.FindIndex(x => x.Name == name);
        }
    }
}
