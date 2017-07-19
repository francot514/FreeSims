using System;
using System.Collections.Generic;
using TSO.Common.content;
using TSO.Content;
using TSO.Content.framework;
using TSO.Files.formats.iff;
using TSO.Files.formats.iff.chunks;

namespace TSO.Content
{
    public class IFFProvider<T> : IContentProvider<T>
    {
        protected Content ContentManager;
        private string IffFile;
        protected Dictionary<ulong, IffEntry<T>> Entries;
        protected IContentCodec<T> Codec;
        protected Dictionary<ulong, T> Cache;
        public Iff MainIff;

        /// <summary>
        /// Creates a new instance of IFFProvider.
        /// </summary>
        /// <param name="contentManager">A Content instance.</param>
        /// <param name="packingslip">The name of a packingslip (xml) file.</param>
        /// <param name="codec">The codec of the file for which to provide access.</param>
        public IFFProvider(Content contentManager, IContentCodec<T> codec, string iffFile)
        {
            this.ContentManager = contentManager;
            this.IffFile = iffFile;
            this.Codec = codec;


        }


        /// <summary>
        /// Gets a file from an archive.
        /// </summary>
        /// <param name="type">The TypeID of the file to get.</param>
        /// <param name="fileID">The FileID of the file to get.</param>
        /// <returns>A file of the specified type.</returns>
        public T Get(uint type, uint fileID)
        {
            var fileIDLong = ((ulong)fileID) << 32;
            return Get(fileIDLong | type);
        }


        public T Get(string name)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get an asset by its ID.
        /// </summary>
        /// <param name="id">The ID of the asset.</param>
        /// <returns>A file of the specified type.</returns>
        public T Get(ulong id)
        {
            lock (Cache)
            {
                if (Cache.ContainsKey(id))
                {
                    return Cache[id];
                }

                var item = (Entries.ContainsKey(id))?Entries[id]:null;
                if(item == null)
                {
                    return default(T);
                }

                using (var dataStream = ContentManager.GetResource(item.FilePath, id))
                {
                    if (dataStream == null){
                        return default(T);
                    }

                    T value = this.Codec.Decode(dataStream);
                    Cache.Add(id, value);
                    return value;
                }
            }
        }

        #region IContentProvider Members
        public void Init()
        {
            Entries = new Dictionary<ulong, IffEntry<T>>();
            Cache = new Dictionary<ulong, T>();

            MainIff = new Iff(IffFile);
            
        }

        public List<IContentReference<T>> List()
        {
            var result = new List<IContentReference<T>>();
            foreach(var item in Entries.Values){
                result.Add(item);
            }
            return result;
        }
        #endregion
    }

    /// <summary>
    /// An entry of a file in a packingslip (*.xml).
    /// </summary>
    /// <typeparam name="T">Type of the file.</typeparam>
    public class IffEntry <T> : IContentReference <T>
    {
        public ushort ID;
        public string FilePath;
        private IFFProvider<T> Provider;

        public IffEntry(IFFProvider<T> provider)
        {
            this.Provider = provider;
        }

        public T Get()
        {
            return Provider.Get(ID);
        }



    }

}
