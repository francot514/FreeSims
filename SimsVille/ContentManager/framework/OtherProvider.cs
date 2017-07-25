using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using TSO.Common.content;
using TSO.Content.framework;
using TSO.Content.codecs;


namespace TSO.Content
{
    public class TS1Provider
    {
        private FAR1Provider<object> FarProvider;

        public TS1Provider(Content contentManager, GraphicsDevice device)
        {
            
            FarProvider = new FAR1Provider<object>(contentManager, null, new Regex(@".*\.far"));
            //todo: files provider?
        }
        
        public void Init()
        {
            FarProvider.Init(0);
        }

        public Dictionary<string, Far1ProviderEntry<object>> BuildDictionary(string ext, string exclude)
        {

            Init();
            var entries = FarProvider.GetEntriesForExtension(ext);
            var result = new Dictionary<string, Far1ProviderEntry<object>>();
            if (entries == null) return result;
            foreach (var entry in entries)
            {
                var name = Path.GetFileName(entry.FarEntry.Filename.ToLowerInvariant());
                if (name.Contains(exclude)) continue;
                result[name] = entry;
            }
            return result;
        }

        public object Get(string item) {
            return FarProvider.Get(item);
        }
    }

    public class TS1SubProvider<T> : IContentProvider<T>
    {
        private TS1Provider BaseProvider;
        private Dictionary<string, Far1ProviderEntry<object>> Entries;
        private string Extension;
        private Func<object, T> Converter;

        public TS1SubProvider(TS1Provider baseProvider, string extension, Func<object, T> converter) : this(baseProvider, extension)
        {
            Converter = converter;
        }

        public TS1SubProvider(TS1Provider baseProvider, string extension)
        {
            BaseProvider = baseProvider;
            Extension = extension;
            Converter = x => (T)x;
        }

        public void Init()
        {
            Entries = BaseProvider.BuildDictionary(Extension, "globals");
        }

        public T Get(ulong id)
        {
            throw new NotImplementedException();
        }

        public virtual T Get(string name)
        {
            Far1ProviderEntry<object> result = null;

            if (Entries.TryGetValue(name, out result))
            {
                return Converter(result.Get());
            }

            return default(T);
        }

        public T Get(uint type, uint fileID)
        {
            throw new NotImplementedException();
        }

        public List<IContentReference<T>> List()
        {
            throw new NotImplementedException();
        }

        public List<Far1ProviderEntry<object>> ListGeneric()
        {
            return new List<Far1ProviderEntry<object>>(Entries.Values);
        }

        public T Get(ContentID id)
        {
            if (id.Name != null) return Get(id.Name);
            throw new NotImplementedException();
        }
    }
}
