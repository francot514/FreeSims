using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Common.content;
using SimsHomeMaker.ContentManager.model;
using TSO.Content;
using TSO.Content.framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.IO;
using TSOVille.Code.Utils;
using TSOVille.Code;

namespace SimsHomeMaker.ContentManager
{
    public class WorldRoofProvider : IContentProvider<Roof>
    {

        private Content ContentManager;
        private Dictionary<ushort, Roof> ById;
        private GraphicsDevice GraphicsDevice;
        public Dictionary<ushort, RoofReference> Entries;
        private Dictionary<ulong, Roof> Cache = new Dictionary<ulong, Roof>();


        public WorldRoofProvider(Content contentManager, GraphicsDevice graphics)
        {
            this.ContentManager = contentManager;
            this.GraphicsDevice = graphics;
        }

         public void Init()
        {

            this.Entries = new Dictionary<ushort, RoofReference>();
            this.ById = new Dictionary<ushort, Roof>();

            var packingslip = new XmlDocument();
            packingslip.Load(ContentManager.GetPath("Content\\roofs.xml"));
            var objectInfos = packingslip.GetElementsByTagName("R");

            

            foreach (XmlNode objectInfo in objectInfos)
            {
                ushort FileID = Convert.ToUInt16(objectInfo.Attributes["id"].Value);
                string path = "GameData/Roofs/" + objectInfo.Attributes["n"].Value + ".bmp";
                    
                FileStream fs = File.Open(path, FileMode.Open);
                Texture2D sprite = Texture2D.FromStream(GraphicsDevice, fs);
                 
                Entries.Add(FileID, new RoofReference(this)
                {
                    ID = FileID,
                    FileName = objectInfo.Attributes["n"].Value,
                    Sprite = sprite
                });

                this.AddRoof(new Roof
                {
                    ID = FileID,
                    Sprite = sprite
                });
               
            }
             

        }

         private void AddRoof(Roof roof)
         {
             ById.Add(roof.ID, roof);
         }

         public Texture2D GetRoofThumb(ushort id, GraphicsDevice device)
         {
             if (id < 28)
             {
                 return ById[id].Sprite;
             }
             else return null;
         }


         public Roof Get(ulong id)
        {

            ushort ids = Convert.ToUInt16(id);

            if (ById.ContainsKey((ushort)id))
            {
                return ById[(ushort)id];
            }



              if (this.Entries[ids] != null)
                {
                    var reference = this.Entries[ids];


                    var item = new Roof
                    {
                        ID = ids,
                        Sprite = reference.Sprite
                    };

                    if (!Cache.ContainsKey(item.ID))
                    {
                        Cache.Add(id, item);

                    }


                    if (!Cache.ContainsKey(id))
                    {
                        return null;
                    }

                }
              return Cache[id];
        }



         public Roof Get(uint type, uint fileID)
         {
             return null;
         }


         public List<IContentReference<Roof>> List()
         {
             return new List<IContentReference<Roof>>(Entries.Values);
         }


        }


         public class RoofReference : IContentReference<Roof>
        {
            public ulong ID;
            public string FileName;

            public int Price; //remember these, just in place of a catalog
            public string Name;
            public string Description;
            public Texture2D Sprite;
            private WorldRoofProvider Provider;

            public RoofReference(WorldRoofProvider provider)
            {
                this.Provider = provider;
            }

            #region IContentReference<Roof> Members

            public Roof Get()
            {
                return Provider.Get(ID);
            }

            #endregion

        }
}
