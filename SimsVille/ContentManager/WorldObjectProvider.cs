using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Content.framework;
using TSO.Common.content;
using System.Xml;
using TSO.Content.codecs;
using System.Text.RegularExpressions;
using TSO.Files.formats.iff;
using TSO.Files.formats.iff.chunks;
using TSO.Files.formats.otf;
using System.IO;
using TSO.Files.FAR1;

namespace TSO.Content
{
    /// <summary>
    /// Provides access to binding (*.iff, *.spf, *.otf) data in FAR3 archives.
    /// </summary>
    public class WorldObjectProvider : IContentProvider<GameObject>
    {
        private Dictionary<ulong, GameObject> Cache = new Dictionary<ulong, GameObject>();
        public FAR1Provider<Iff> Iffs;
        public FAR1Provider<Iff> Sprites;
        //private FAR1Provider<OTF> TuningTables;
        private Content ContentManager;
        public List<GameObjectResource> Resources;
        public List<string> FarFiles;

        private Dictionary<ulong, GameObjectReference> Entries;

        public WorldObjectProvider(Content contentManager)
        {
            this.ContentManager = contentManager;
        }

        /// <summary>
        /// Initiates loading of world objects.
        /// </summary>
        public void Init()
        {

            string dpath = "Downloads";
            FarFiles = new List<string>();

            /** Load packingslip **/
            Entries = new Dictionary<ulong, GameObjectReference>();
            Cache = new Dictionary<ulong, GameObject>();

            if (Directory.Exists("GameData"))
            {
                FarFiles.Add("GameData\\Objects\\Objects.far");

                var objects = new XmlDocument();
                objects.Load(ContentManager.GetPath("Content\\objects.xml"));
                var objectInfos = objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = objectInfo.Attributes["n"].Value
                    });
                }


            }

            if (Directory.Exists("ExpansionPack"))
            {

                FarFiles.Add("ExpansionPack\\ExpansionPack.far");

                var ep1objects = new XmlDocument();
                ep1objects.Load(ContentManager.GetPath("Content\\ep1.xml"));
                var ep1objectsInfos = ep1objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep1objectsInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = objectInfo.Attributes["n"].Value
                    });
                }

            }

            if (Directory.Exists("ExpansionPack2"))
            {
                FarFiles.Add("ExpansionPack\\ExpansionPack2.far");

                var ep2objects = new XmlDocument();
                ep2objects.Load(ContentManager.GetPath("Content\\ep2.xml"));
                var ep2objectsInfos = ep2objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep2objectsInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = objectInfo.Attributes["n"].Value
                    });
                }


             }

            if (Directory.Exists("ExpansionPack3"))
            {
                FarFiles.Add("ExpansionPack\\ExpansionPack3.far");

                var ep3objects = new XmlDocument();
                ep3objects.Load(ContentManager.GetPath("Content\\ep3.xml"));
                var ep3objectsInfos = ep3objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep3objectsInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = objectInfo.Attributes["n"].Value
                    });
                }


            }


            if (Directory.Exists("ExpansionPack4"))
            {
                FarFiles.Add("ExpansionPack\\ExpansionPack4.far");

                var ep4objects = new XmlDocument();
                ep4objects.Load(ContentManager.GetPath("Content\\ep4.xml"));
                var ep4objectsInfos = ep4objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in ep4objectsInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = objectInfo.Attributes["n"].Value
                    });
                }

            }



            if (!Directory.Exists(dpath))
                Directory.CreateDirectory(dpath);

            DirectoryInfo dir = new DirectoryInfo(dpath);

            var ToRemove = new List<ulong>();

            foreach (FileInfo file in dir.GetFiles())
                if (file.Extension == ".iff")
                {
                    var iff = new Iff(file.FullName);
                    ulong FileID = iff.List<OBJD>()[0].GUID;

                    foreach (var obj in Entries)
                        if (obj.Key == FileID)
                            ToRemove.Add(obj.Key);

                    foreach (var guid in ToRemove)
                        if (guid == FileID)
                            Entries.Remove(FileID);
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = Path.GetFileNameWithoutExtension(file.Name)
                    });


                }


            

            Iffs = new FAR1Provider<Iff>(ContentManager, new IffCodec(), FarFiles.ToArray());
            Sprites = new FAR1Provider<Iff>(ContentManager, new IffCodec(), FarFiles.ToArray());
            //TuningTables = new FAR1Provider<OTF>(ContentManager, new OTFCodec(), new Regex(".*\\\\objotf.*\\.far"));
            Resources = new List<GameObjectResource>();

            Iffs.Init(0);
            //TuningTables.Init();
            Sprites.Init(0);


                
         }


        public List<string> ProcessedFiles = new List<string>();

        #region IContentProvider<GameObject> Members

        public GameObject Get(uint id)
        {
            return Get((ulong)id);
        }

        public GameObject Get(ulong id)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(id))
                {
                    return Cache[id];
                }

                if (this.Entries[id] != null)
                {
                    var reference = this.Entries[id];
                    if (ProcessedFiles.Contains(reference.FileName))
                        return null;
                        

                /** Better set this up! **/
                var iff = this.Iffs.Get(reference.FileName + ".iff");

                if (iff == null)
                    iff = new Iff("Downloads/" + reference.FileName + ".iff");

                var sprites = this.Sprites.Get(reference.FileName + ".iff");
                //var tuning = this.TuningTables.Get(reference.FileName + ".otf");

                if (sprites == null)
                    sprites = new Iff("Downloads/" + reference.FileName + ".iff");

                ProcessedFiles.Add(reference.FileName);
                    
               
                var resource = new GameObjectResource(iff, sprites);
                Resources.Add(resource);


                    foreach (var objd in iff.List<OBJD>())
                    {
                        var item = new GameObject
                        {
                            GUID = objd.GUID,
                            OBJ = objd,
                            Resource = resource
                        };
                        if (!Cache.ContainsKey(item.GUID))
                        {
                            Cache.Add(item.GUID, item);
                            
                        }
                    }

                }
                //0x3BAA9787
                if (!Cache.ContainsKey(id))
                {
                    return null;
                }

                }
                return Cache[id];
                
            
        }

        public GameObject Get(uint type, uint fileID)
        {
            return Get(fileID);
        }

        public List<IContentReference<GameObject>> List()
        {
            return null;
        }

        #endregion
    }

    public class GameObjectReference : IContentReference<GameObject>
    {
        public ulong ID;
        public string FileName;

        private WorldObjectProvider Provider;

        public GameObjectReference(WorldObjectProvider provider)
        {
            this.Provider = provider;
        }

        #region IContentReference<GameObject> Members

        public GameObject Get()
        {
            return Provider.Get(ID);
        }

        #endregion
    }

    /// <summary>
    /// An object in the game world.
    /// </summary>
    public class GameObject
    {
        public ulong GUID;
        public OBJD OBJ;
        public GameObjectResource Resource;
    }

    public abstract class GameIffResource
    {
        public abstract Iff MainIff { get; }
        public abstract T Get<T>(ushort id);
        public abstract List<T> List<T>();
        public T[] ListArray<T>()
        {
            List<T> result = List<T>();
            if (result == null) result = new List<T>();
            return result.ToArray();
        }
        public GameGlobalResource SemiGlobal;
    }

    /// <summary>
    /// The resource for an object in the game world.
    /// </summary>
    public class GameObjectResource : GameIffResource
    {
        //DO NOT USE THESE, THEY ARE ONLY PUBLIC FOR DEBUG UTILITIES
        public Iff Iff;
        public Iff Sprites;

        public override Iff MainIff
        {
            get { return Iff; }
        }

        public GameObjectResource(Iff iff, Iff sprites)
        {
            this.Iff = iff;
            this.Sprites = sprites;
            //this.Tuning = tuning;
        }

        /// <summary>
        /// Gets a game object's resource based on the ID found in the object's OTF.
        /// </summary>
        /// <typeparam name="T">Type of object reource to load (IFF, SPF).</typeparam>
        /// <param name="id">ID of the resource to load.</param>
        /// <returns>An object's resource of the specified type.</returns>
        public override T Get<T>(ushort id)
        {
            var type = typeof(T);
            if (type == typeof(OTFTable))
            {
                
                
               return default(T);
                
            }

            T item1 = this.Iff.Get<T>(id);
            if (item1 != null)
            {
                return item1;
            }

            if (this.Sprites != null)
            {
                T item2 = this.Sprites.Get<T>(id);
                if (item2 != null)
                {
                    return item2;
                }
            }
            return default(T);
        }

        public override List<T> List<T>()
        {
            var type = typeof(T);
            if (type == typeof(SPR2) || type == typeof(SPR) || type == typeof(DGRP))
            {
                return this.Sprites.List<T>();
            }
            return this.Iff.List<T>();
        }
    }
}
