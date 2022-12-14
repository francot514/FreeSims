/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Content.Framework;
using FSO.Common.Content;
using System.Xml;
using FSO.Content.Codecs;
using System.Text.RegularExpressions;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using FSO.Files.Formats.OTF;
using System.Collections.Concurrent;
using System.IO;
using FSO.Common;
using FSO.SimAntics.Engine;

namespace FSO.Content
{
    /// <summary>
    /// Provides access to binding (*.iff, *.spf, *.otf) data in FAR3 archives.
    /// </summary>
    public class WorldObjectProvider : IContentProvider<GameObject>
    {
        private ConcurrentDictionary<ulong, GameObject> Cache = new ConcurrentDictionary<ulong, GameObject>();
        private FAR1Provider<Files.Formats.IFF.IffFile> Iffs;
        private FAR1Provider<Files.Formats.IFF.IffFile> Sprites;
        private FAR1Provider<OTFFile> TuningTables;
        private Content ContentManager;

        public Dictionary<ulong, GameObjectReference> Entries;

        public WorldObjectProvider(Content contentManager)
        {
            this.ContentManager = contentManager;
        }

        private bool WithSprites;

        /// <summary>
        /// Initiates loading of world objects.
        /// </summary>
        public void Init(bool withSprites)
        {
            List<string>FarFiles = new List<string>();
            List<string> SpriteFiles = new List<string>();

            WithSprites = withSprites;

            for (int i = 1; i <= 9; i++)
                SpriteFiles.Add("objectdata/objects/objspf" + i + ".far");

            FarFiles.Add("objectdata/objects/objiff.far");

            /** Load packingslip **/
            Entries = new Dictionary<ulong, GameObjectReference>();
            Cache = new ConcurrentDictionary<ulong, GameObject>();

            var packingslip = new XmlDocument();
            packingslip.Load("Content/objects.xml");
            var objectInfos = packingslip.GetElementsByTagName("I");

            foreach (XmlNode objectInfo in objectInfos)
            {
                ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);
                Entries.Add(FileID, new GameObjectReference(this)
                {
                    ID = FileID,
                    FileName = objectInfo.Attributes["n"].Value,
                    Source = GameObjectSource.Far,
                    Name = objectInfo.Attributes["o"].Value,
                    Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                    SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                    EpObject = false
                });
            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/GameData/Objects"))
            {

                string GFile = FSOEnvironment.SimsCompleteDir + "/GameData/Objects/Objects.far";
                FarFiles.Add(GFile);
                SpriteFiles.Add(GFile);

                var gobjects = new XmlDocument();
                gobjects.Load("Content/npc.xml");
                var nobjectInfos = gobjects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in nobjectInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }


            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack"))
            {

                string EpFile = FSOEnvironment.SimsCompleteDir + "/ExpansionPack/ExpansionPack.far";
                FarFiles.Add(EpFile);
                SpriteFiles.Add(EpFile);



                var ep1objects = new XmlDocument();
                ep1objects.Load("Content/ep1.xml");
                var objectInfos1 = ep1objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos1)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack2"))
            {

                string Ep2File = FSOEnvironment.SimsCompleteDir + "/ExpansionPack2/ExpansionPack2.far";
                FarFiles.Add(Ep2File);
                SpriteFiles.Add(Ep2File);



                var ep2objects = new XmlDocument();
                ep2objects.Load("Content/ep2.xml");
                var objectInfos2 = ep2objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos2)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack3"))
            {

                string Ep3File = FSOEnvironment.SimsCompleteDir + "/ExpansionPack3/ExpansionPack3.far";
                FarFiles.Add(Ep3File);
                SpriteFiles.Add(Ep3File);



                var ep3objects = new XmlDocument();
                ep3objects.Load("Content/ep3.xml");
                var objectInfos3 = ep3objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos3)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }


            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack4"))
            {

                string Ep4File = FSOEnvironment.SimsCompleteDir + "/ExpansionPack4/ExpansionPack4.far";
                FarFiles.Add(Ep4File);
                SpriteFiles.Add(Ep4File);

              

                var ep4objects = new XmlDocument();
                ep4objects.Load("Content/ep4.xml");
                var objectInfos4 = ep4objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos4)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                    Entries.Add(FileID, new GameObjectReference(this)
                    {
                        ID = FileID,
                        FileName = objectInfo.Attributes["n"].Value,
                        Source = GameObjectSource.Far,
                        Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                        SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                        EpObject = true
                        
                    });
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack5"))
            {

                string Ep5File = FSOEnvironment.SimsCompleteDir + "/ExpansionPack5/ExpansionPack5.far";
                FarFiles.Add(Ep5File);
                SpriteFiles.Add(Ep5File);



                var ep5objects = new XmlDocument();
                ep5objects.Load("Content/ep5.xml");
                var objectInfos5 = ep5objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos5)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }


            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack6"))
            {

                string Ep6File = FSOEnvironment.SimsCompleteDir + "/ExpansionPack6/ExpansionPack6.far";
                FarFiles.Add(Ep6File);
                SpriteFiles.Add(Ep6File);



                var ep6objects = new XmlDocument();
                ep6objects.Load("Content/ep6.xml");
                var objectInfos6 = ep6objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos6)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/ExpansionPack7"))
            {

                string Ep7File = FSOEnvironment.SimsCompleteDir + "/ExpansionPack7/ExpansionPack7.far";
                FarFiles.Add(Ep7File);
                SpriteFiles.Add(Ep7File);



                var ep7objects = new XmlDocument();
                ep7objects.Load("Content/ep7.xml");
                var objectInfos7 = ep7objects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos7)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/Deluxe"))
            {

                string DeluxeFile = FSOEnvironment.SimsCompleteDir + "/Deluxe/Deluxe.far";
                FarFiles.Add(DeluxeFile);
                SpriteFiles.Add(DeluxeFile);



                var deluxeobjects = new XmlDocument();
                deluxeobjects.Load("Content/deluxe.xml");
                var deluxeInfos = deluxeobjects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in deluxeInfos)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }

            if (Directory.Exists(FSOEnvironment.SimsCompleteDir + "/Downloads"))
            {
                DirectoryInfo downloadsDir = new DirectoryInfo(FSOEnvironment.SimsCompleteDir + "/Downloads");

                foreach (DirectoryInfo dir in downloadsDir.GetDirectories())
                {

                    if (dir.GetFiles().Count() > 0)
                    {

                        foreach (FileInfo file in dir.GetFiles())
                            if (file.Extension == ".far")
                                FarFiles.Add(file.FullName);

                    }

                   
                }

                var dobjects = new XmlDocument();
                dobjects.Load("Content/downloads.xml");
                var objectInfos8 = dobjects.GetElementsByTagName("P");

                foreach (XmlNode objectInfo in objectInfos8)
                {
                    ulong FileID = Convert.ToUInt32(objectInfo.Attributes["g"].Value, 16);

                    if (!Entries.ContainsKey(FileID))
                        Entries.Add(FileID, new GameObjectReference(this)
                        {
                            ID = FileID,
                            FileName = objectInfo.Attributes["n"].Value,
                            Source = GameObjectSource.Far,
                            Group = Convert.ToInt16(objectInfo.Attributes["m"].Value),
                            SubIndex = Convert.ToInt16(objectInfo.Attributes["i"].Value),
                            EpObject = true

                        });
                }

            }

            Iffs = new FAR1Provider<Files.Formats.IFF.IffFile>(ContentManager, new IffCodec(), FarFiles.ToArray());

            TuningTables = new FAR1Provider<OTFFile>(ContentManager, new OTFCodec(), new Regex(".*/objotf.*\\.far"));

            Iffs.Init();
            TuningTables.Init();

            if (withSprites)
            {
                Sprites = new FAR1Provider<Files.Formats.IFF.IffFile>(ContentManager, new IffCodec(), SpriteFiles.ToArray());
                Sprites.Init();
            }


            //init local objects, piff clones
            string[] paths = Directory.GetFiles(Path.Combine(FSOEnvironment.ContentDir, "Objects"), "*.iff", SearchOption.AllDirectories);
            for (int i = 0; i < paths.Length; i++)
            {
                string entry = paths[i];
                string filename = Path.GetFileName(entry);
                IffFile iffFile = new IffFile(entry);

                var objs = iffFile.List<OBJD>();
                foreach (var obj in objs)
                {
                    Entries.Add(obj.GUID, new GameObjectReference(this)
                    {
                        ID = obj.GUID,
                        FileName = entry,
                        Source = GameObjectSource.Standalone,
                        Name = obj.ChunkLabel,
                        Group = (short)obj.MasterID,
                        SubIndex = obj.SubIndex
                    });
                }
            }

            
        }

        private GameObjectResource GenerateResource(GameObjectReference reference)
        {
            
                /** Better set this up! **/
                IffFile sprites = null, iff = null;
                OTFFile tuning = null;

                if (reference.Source == GameObjectSource.Far)
                {
                    iff = this.Iffs.Get(reference.FileName + ".iff");
                    iff.InitHash();
                    iff.RuntimeInfo.Path = reference.FileName;
                    if (WithSprites) sprites = this.Sprites.Get(reference.FileName + ".spf");
                    var rewrite = PIFFRegistry.GetOTFRewrite(reference.FileName + ".otf");
                    try
                    {
                        tuning = (rewrite != null) ? new OTFFile(rewrite) : this.TuningTables.Get(reference.FileName + ".otf");
                    }
                    catch (Exception)
                    {
                        //if any issues occur loading an otf, just silently ignore it.
                    }
                }
                else
                {
                    iff = new IffFile(reference.FileName);
                    iff.InitHash();
                    iff.RuntimeInfo.Path = reference.FileName;
                    iff.RuntimeInfo.State = IffRuntimeState.Standalone;
                }

                if (iff.RuntimeInfo.State == IffRuntimeState.PIFFPatch)
                {
                    //OBJDs may have changed due to patch. Remove all file references
                    ResetFile(iff);
                }

                iff.RuntimeInfo.UseCase = IffUseCase.Object;
                if (sprites != null) sprites.RuntimeInfo.UseCase = IffUseCase.ObjectSprites;

                return new GameObjectResource(iff, sprites, tuning, reference.FileName, reference.EpObject);
            
        }
    

        private ConcurrentDictionary<string, GameObjectResource> ProcessedFiles = new ConcurrentDictionary<string, GameObjectResource>();

        #region IContentProvider<GameObject> Members

        public GameObject Get(uint id, bool ts1)
        {
            return Get((ulong)id, ts1);
        }

        public GameObject Get(ulong id, bool ts1)
        {
            if (Cache.ContainsKey(id))
            {
                return Cache[id];
            }

            lock (Cache)
            {
                if (!Cache.ContainsKey(id))
                {
                    GameObjectReference reference;
                    GameObjectResource resource = null;

                    lock (Entries)
                    {
                        Entries.TryGetValue(id, out reference);
                        if (reference == null)
                        {
                            //Console.WriteLine("Failed to get Object ID: " + id.ToString() + " (no resource)");
                            return null;
                        }
                        lock (ProcessedFiles)
                        {
                            //if a file is processed but an object in it is not in the cache, it may have changed.
                            //check for it again!
                            ProcessedFiles.TryGetValue(reference.FileName, out resource);
                        }
                    }

                    if (resource == null)
                    {
                        /** Better set this up! **/
                        Files.Formats.IFF.IffFile sprites = null, iff = null;
                        OTFFile tuning = null;

                        if (reference.Source == GameObjectSource.Far)
                        {
                            iff = this.Iffs.Get(reference.FileName + ".iff");
                            iff.RuntimeInfo.Path = reference.FileName;
                            if (WithSprites)
                                if (reference.EpObject)
                                    sprites = this.Sprites.Get(reference.FileName + ".iff");
                                else
                                    sprites = this.Sprites.Get(reference.FileName + ".spf");
                            

                            tuning = this.TuningTables.Get(reference.FileName + ".otf");
                        }
                        else
                        {
                            iff = new Files.Formats.IFF.IffFile(reference.FileName);
                            iff.RuntimeInfo.Path = reference.FileName;
                            iff.RuntimeInfo.State = IffRuntimeState.Standalone;
                        }

                        if (iff.RuntimeInfo.State == IffRuntimeState.PIFFPatch)
                        {
                            //OBJDs may have changed due to patch. Remove all file references
                            ResetFile(iff);

                        }

                        iff.RuntimeInfo.UseCase = IffUseCase.Object;
                        if (sprites != null) sprites.RuntimeInfo.UseCase = IffUseCase.ObjectSprites;

                        resource = new GameObjectResource(iff, sprites, tuning, reference.FileName, ts1);


                        lock (ProcessedFiles)
                        {
                            ProcessedFiles.GetOrAdd(reference.FileName, resource);
                        }
                        
                        var piffModified = PIFFRegistry.GetOBJDRewriteNames();
                        foreach (var name in piffModified)
                        {
                            ProcessedFiles.GetOrAdd(name, GenerateResource(new GameObjectReference(this) { FileName = name.Substring(0, name.Length - 4), Source = GameObjectSource.Far, EpObject = ts1 }));
                        }

                    }

                    foreach (var objd in resource.MainIff.List<OBJD>())
                    {
                        var item = new GameObject
                        {
                            GUID = objd.GUID,
                            OBJ = objd,
                            Resource = resource
                        };
                        Cache.GetOrAdd(item.GUID, item);
                    }
                    //0x3BAA9787
                    if (!Cache.ContainsKey(id))
                    {
                       // Console.WriteLine("Failed to get Object ID: " + id.ToString() + " from resource " + resource.Name);
                        return null;
                    }
                    return Cache[id];
                }
                return Cache[id];
            }
        }

        public GameObject Get(uint type, uint fileID, bool ts1)
        {
            return Get(fileID, ts1);
        }

        public List<IContentReference<GameObject>> List()
        {
            return null;
        }

        #endregion

        /* 
        EXTERNAL MODIFICATION API
        Lets user add/remove/modify object references. (master id/guid/group info)
        */

        public void AddObject(GameObject obj)
        {
            lock (Entries)
            {
                var iff = obj.Resource.MainIff;
                AddObject(iff, obj.OBJ);
            }
        }

        public void AddObject(Files.Formats.IFF.IffFile iff, OBJD obj)
        {
            lock (Entries)
            {
                GameObjectSource source;
                switch (iff.RuntimeInfo.State)
                {
                    case IffRuntimeState.PIFFClone:
                        source = GameObjectSource.PIFFClone;
                        break;
                    case IffRuntimeState.Standalone:
                        source = GameObjectSource.Standalone;
                        break;
                    default:
                        source = GameObjectSource.Far;
                        break;
                }

                Entries.Add(obj.GUID, new GameObjectReference(this)
                {
                    ID = obj.GUID,
                    FileName = iff.RuntimeInfo.Path,
                    Source = source,
                    Name = obj.ChunkLabel,
                    Group = (short)obj.MasterID,
                    SubIndex = obj.SubIndex
                });
            }
        }

        public void RemoveObject(uint GUID)
        {
            lock (Entries)
            {
                Entries.Remove(GUID);
            }
            lock (Cache)
            {
                GameObject removed;
                Cache.TryRemove(GUID, out removed);
            }
        }

        public void ResetFile(Files.Formats.IFF.IffFile iff)
        {
            lock (Entries)
            {
                var ToRemove = new List<uint>();
                foreach (var objt in Entries)
                {
                    var obj = objt.Value;
                    if (obj.FileName == iff.RuntimeInfo.Path) ToRemove.Add((uint)objt.Key);
                }
                foreach (var guid in ToRemove)
                {
                    Entries.Remove(guid);
                }

                //add all OBJDs
                var list = iff.List<OBJD>();
                if (list != null)
                {
                    foreach (var obj in list) AddObject(iff, obj);
                }
            }
        }

        public void ModifyMeta(GameObject obj, uint oldGUID)
        {
            lock (Entries)
            {
                RemoveObject(oldGUID);
                AddObject(obj);
            }
        }
    }

    public class GameObjectReference : IContentReference<GameObject>
    {
        public ulong ID;
        public string FileName;
        public GameObjectSource Source;

        public string Name;
        public short Group;
        public short SubIndex;
        public bool EpObject;

        private WorldObjectProvider Provider;

        public GameObjectReference(WorldObjectProvider provider)
        {
            this.Provider = provider;
        }

        #region IContentReference<GameObject> Members

        public GameObject Get(bool ts1)
        {
            return Provider.Get(ID, ts1);
        }

        #endregion
    }

    public enum GameObjectSource
    {
        Far,
        PIFFClone,
        Standalone
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
        public abstract IffFile MainIff { get; }
        public abstract T Get<T>(ushort id);
        public abstract List<T> List<T>();
        public Dictionary<uint, short> TuningCache;
        public Dictionary<ushort, object> RoutineCache;

        public T[] ListArray<T>()
        {
            List<T> result = List<T>();
            if (result == null) result = new List<T>();
            return result.ToArray();
        }
        public GameGlobalResource SemiGlobal;

        public static Func<BHAV, object> BHAVAssembler;

        public virtual void Recache()
        {
            RoutineCache = new Dictionary<ushort, object>();
            var bhavs = MainIff.List<BHAV>();
            if (bhavs != null)
            {
                foreach (var bhav in bhavs)
                {
                    RoutineCache[bhav.ChunkID] = BHAVAssembler(bhav);
                }
            }

            TuningCache = new Dictionary<uint, short>();

            var bcons = MainIff.List<BCON>();
            if (bcons != null)
            {
                foreach (var table in bcons)
                {
                    uint i = ((uint)table.ChunkID << 16);
                    foreach (var item in table.Constants)
                    {
                        TuningCache[i++] = (short)item;
                    }
                }
            }
        }

        public object GetRoutine(ushort id)
        {
            object result;
            if (RoutineCache.TryGetValue(id, out result))
                return result;
            else
                return null;
        }

    }

    /// <summary>
    /// The resource for an object in the game world.
    /// </summary>
    public class GameObjectResource : GameIffResource
    {
        //DO NOT USE THESE, THEY ARE ONLY PUBLIC FOR DEBUG UTILITIES
        public Files.Formats.IFF.IffFile Iff;
        public Files.Formats.IFF.IffFile Sprites;
        public OTFFile Tuning;

        //use this tho
        public string Name;
        public Dictionary<string, VMTreeByNameTableEntry> TreeByName;
        public override IffFile MainIff
        {
            get { return Iff; }
        }

        public GameObjectResource(Files.Formats.IFF.IffFile iff, Files.Formats.IFF.IffFile sprites, OTFFile tuning, string iname, bool ts1)
        {
            this.Iff = iff;
            this.Sprites = sprites;
            this.Tuning = tuning;
            this.Name = iname;

            if (iff == null) return;
            var GLOBChunks = iff.List<GLOB>();
            if (GLOBChunks != null && GLOBChunks[0].Name != "")
            {
                var sg = FSO.Content.Content.Get().WorldObjectGlobals.Get(GLOBChunks[0].Name, ts1);
                if (sg != null) SemiGlobal = sg.Resource; //used for tuning constant fetching.
            }

            TreeByName = new Dictionary<string, VMTreeByNameTableEntry>();
            var bhavs = List<BHAV>();
            if (bhavs != null)
            {
                foreach (var bhav in bhavs)
                {
                    string name = bhav.ChunkLabel;
                    for (var i = 0; i < name.Length; i++)
                    {
                        if (name[i] == 0)
                        {
                            name = name.Substring(0, i);
                            break;
                        }
                    }
                    if (!TreeByName.ContainsKey(name)) TreeByName.Add(name, new VMTreeByNameTableEntry(bhav));
                }
            }
            //also add semiglobals

            if (SemiGlobal != null)
            {
                bhavs = SemiGlobal.List<BHAV>();
                if (bhavs != null)
                {
                    foreach (var bhav in bhavs)
                    {
                        string name = bhav.ChunkLabel;
                        for (var i = 0; i < name.Length; i++)
                        {
                            if (name[i] == 0)
                            {
                                name = name.Substring(0, i);
                                break;
                            }
                        }
                        if (!TreeByName.ContainsKey(name)) TreeByName.Add(name, new VMTreeByNameTableEntry(bhav));
                    }
                }
            }

        }


        public override void Recache()
        {
            base.Recache();

           

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
                if (Tuning != null)
                {
                    return (T)(object)Tuning.GetTable(id);
                }
                else
                {
                    return default(T);
                }
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
            if ((type == typeof(SPR2) || type == typeof(SPR) || type == typeof(DGRP)) && this.Sprites != null)
            {
                return this.Sprites.List<T>();
            }
            return this.Iff.List<T>();
        }
    }

    public class VMTreeByNameTableEntry
    {
        public BHAV bhav;

        public VMTreeByNameTableEntry(BHAV bhav)
        {
            this.bhav = bhav;
        }
    }

}
