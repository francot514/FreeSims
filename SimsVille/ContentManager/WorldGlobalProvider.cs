using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Content.framework;
using TSO.Common.content;
using System.Xml;
using TSO.Content.codecs;
using System.Text.RegularExpressions;
using System.IO;
using TSO.Files.formats.iff;
using TSO.Files.formats.iff.chunks;
using TSO.Files.formats.otf;

namespace TSO.Content
{
    /// <summary>
    /// Provides access to global (*.otf, *.iff) data in FAR3 archives.
    /// </summary>
    public class WorldGlobalProvider
    {
        private Dictionary<string, GameGlobal> Cache; //indexed by lowercase filename, minus directory and extension.
        public FAR1Provider<Iff> GlobalIffs;
        private Content ContentManager;
        private Dictionary<ulong, GameObjectReference> Entries;


        public WorldGlobalProvider(Content contentManager)
        {
            this.ContentManager = contentManager;
        }

        /// <summary>
        /// Creates a new cache for loading of globals.
        /// </summary>
        public void Init()
        {
            Cache = new Dictionary<string, GameGlobal>();

            GlobalIffs = new FAR1Provider<Iff>(ContentManager, new IffCodec(), "GameData\\Global\\Global.far");

            GlobalIffs.Init(0);


            Entries = new Dictionary<ulong, GameObjectReference>();


        }

        /// <summary>
        /// Gets a resource.
        /// </summary>
        /// <param name="filename">The filename of the resource to get.</param>
        /// <returns>A GameGlobal instance containing the resource.</returns>
        public GameGlobal Get(string filename)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(filename))
                {
                    return Cache[filename];
                }

                

                //if we can't load this let it throw an exception...
                //probably sanity check this when we add user objects.
                var iff = this.GlobalIffs.Get(filename + ".iff");
                //var iff = new Iff(Path.Combine(Content.Get().BasePath, "GameData\\Global\\" + filename + ".iff")); 
                
                var resource = new GameGlobalResource(iff);

                var item = new GameGlobal
                {
                    Resource = resource
                };

                Cache.Add(filename, item);

                return item;
            }
        }
    }

    public class GameGlobal
    {
        public GameGlobalResource Resource;
    }

    /// <summary>
    /// A global can be an OTF (Object Tuning File) or an IFF.
    /// </summary>
    public class GameGlobalResource : GameIffResource
    {
        public Iff Iff;
        public OTF Tuning;

        public override Iff MainIff
        {
            get { return Iff; }
        }

        public GameGlobalResource(Iff iff)
        {
            this.Iff = iff;
            //this.Tuning = tuning;
        }

        public override T Get<T>(ushort id)
        {
            var type = typeof(T);

            T item1 = this.Iff.Get<T>(id);
            if (item1 != null)
            {
                return item1;
            }

            if (type == typeof(OTFTable))
            {
                if (Tuning != null)
                {
                    return (T)(object)Tuning.GetTable(id);
                }
            }

            return default(T);
        }

        public override List<T> List<T>()
        {
            return this.Iff.List<T>();
        }
    }
}
