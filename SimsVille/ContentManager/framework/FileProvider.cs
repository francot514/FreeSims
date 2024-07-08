/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Common.Content;
using System.Text.RegularExpressions;
using System.IO;
using FSO.Content.Codecs;

namespace FSO.Content.Framework
{
    /// <summary>
    /// Provides access to files.
    /// </summary>
    /// <typeparam name="T">The type of file to provide access to.</typeparam>
    public class FileProvider<T> : IContentProvider<T>
    {
        protected Content ContentManager;
        protected Dictionary<string, string> EntriesByName;
        protected Dictionary<ulong, string> EntryNamesById;
        protected IContentCodec<T> Codec;
        protected Dictionary<string, T> Cache;
        protected List<FileContentReference<T>> Items;
        private Regex FilePattern;
        public string[] Files;
        public bool UseTS1;
        public bool UseContent;

        /// <summary>
        /// Creates a new instance of FileProvider.
        /// </summary>
        /// <param name="contentManager">A Content instance.</param>
        /// <param name="codec">The codec of the filetype of which to provide access.</param>
        /// <param name="filePattern">Filepattern used to search for files of this type.</param>
        public FileProvider(Content contentManager, IContentCodec<T> codec, Regex filePattern)
        {
            this.ContentManager = contentManager;
            this.Codec = codec;
            this.FilePattern = filePattern;
        }

        public FileProvider(Content contentManager, IContentCodec<T> codec, string[] files)
        {
            this.ContentManager = contentManager;
            this.Codec = codec;
            this.Files = files;
        }

        /// <summary>
        /// Initiates loading of files of this type.
        /// </summary>
        public void Init()
        {
            this.Items = new List<FileContentReference<T>>();
            this.Cache = new Dictionary<string, T>();
            this.EntriesByName = new Dictionary<string, string>();
            this.EntryNamesById = new Dictionary<ulong, string>();

            lock (Cache)
            {
                List<string> matchedFiles = new List<string>();
                var files = UseContent ? ContentManager.ContentFiles : (UseTS1 ? ContentManager.TS1AllFiles : ContentManager.AllFiles);
                var basePath = UseTS1 ? ContentManager.BasePath : ContentManager.BasePath;
                if (FilePattern != null)
                foreach (var file in ContentManager.AllFiles)
                {
                    if (FilePattern.IsMatch(file))
                    {
                        matchedFiles.Add(file);
                    }
                }
                else
                {
                    foreach (var file in Files)
                        matchedFiles.Add(file);

                }

                foreach (var file in matchedFiles)
                {
                    var name = Path.GetFileName(file).ToLowerInvariant();
                    EntriesByName.Add(name, file);
                    Items.Add(new FileContentReference<T>(name, this));
                }
            }
        }

        /// <summary>
        /// Gets a file.
        /// </summary>
        /// <param name="name">The name of the file to get.</param>
        /// <returns>The file.</returns>
        public T Get(string name)
        {
            name = name.ToLowerInvariant();
            lock (Cache)
            {
                if (Cache.ContainsKey(name))
                {
                    return Cache[name];
                }

                if (EntriesByName.ContainsKey(name))
                {
                    var fullPath = ContentManager.GetPath(EntriesByName[name]);
                    using (var reader = File.OpenRead(fullPath))
                    {
                        var item = Codec.Decode(reader);
                        Cache.Add(name, item);
                        return item;
                    }
                }
                return default(T);
            }
        }


        public T ThrowawayGet(string name)
        {
            if (EntriesByName.ContainsKey(name))
            {
                var fullPath = UseContent ? ("Content/" + EntriesByName[name]) : (UseTS1 ? Path.Combine(ContentManager.BasePath, EntriesByName[name]) : ContentManager.GetPath(EntriesByName[name]));
                using (var reader = File.OpenRead(fullPath))
                {
                    T item;
                    if (Codec == null) item = (T)SmartCodec.Decode(reader, Path.GetExtension(fullPath));
                    else item = Codec.Decode(reader);
                    return item;
                }
            }
            return default(T);
        }

        #region IContentProvider<T> Members



        public T Get(ContentID id)
        {
            throw new NotImplementedException();
        }
        protected virtual T ResolveById(ulong id)
        {
            string entry = null;
            if (EntryNamesById.TryGetValue(id, out entry))
            {
                return Get(entry);
            }
            return default(T);
        }


        public T Get(ulong id, bool ts1)
        {
            return ResolveById(id);
        }

        public T Get(uint type, uint fileID, bool ts1)
        {
            var fileIDLong = ((ulong)fileID) << 32;
            return Get(fileIDLong | type, true);

        }


        public List<IContentReference<T>> List()
        {
            return new List<IContentReference<T>>(Items);
        }

        #endregion
    }

    /// <summary>
    /// Reference to a file's contents.
    /// </summary>
    /// <typeparam name="T">The type of file to reference.</typeparam>
    public class FileContentReference<T> : IContentReference<T>
    {
        public string Name;
        private FileProvider<T> Provider;

        public FileContentReference(string name, FileProvider<T> provider){
            this.Name = name;
            this.Provider = provider;
        }

        #region IContentReference<T> Members

        public T Get(bool ts1){
            return this.Provider.Get(Name);
        }


        public object GetThrowawayGeneric()
        {
            throw new NotImplementedException();
        }

        public object GetGeneric()
        {
            return Get(true);
        }
        #endregion
    }
}
