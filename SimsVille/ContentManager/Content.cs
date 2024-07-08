/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using FSO.Files.FAR3;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common.Content;
using FSO.Files;
using FSO.Files.Formats.IFF;
using System.Threading;
using FSO.Common;
using FSO.Content.Framework;
using FSO.Content.TS1;
using FSO.Content.Interfaces;
using FSO.Vitaboy;

namespace FSO.Content
{
    /// <summary>
    /// Content is a singleton responsible for loading data.
    /// </summary>
    public class Content
    {
        public static void Init(string basepath, GraphicsDevice device){
            INSTANCE = new Content(basepath, device);
        }
        private static Content INSTANCE;
        public static Content Get()
        {
            return INSTANCE;
        }

        /**
         * Content Manager
         */
        public string BasePath;
        public string[] AllFiles;
        public string[] TS1AllFiles;
        public string[] ContentFiles;
        public bool TS1;
        private GraphicsDevice Device;

        public ChangeManager Changes;

        /// <summary>
        /// Creates a new instance of Content.
        /// </summary>
        /// <param name="basePath">Path to client directory.</param>
        /// <param name="device">A GraphicsDevice instance.</param>
        private Content(string basePath, GraphicsDevice device)
        {
            this.BasePath = basePath;
            this.Device = device;

            Changes = new ChangeManager();

            UIGraphics = new UIGraphicsProvider(this);
            AvatarMeshes = new AvatarMeshProvider(this, Device);
            AvatarBindings = new AvatarBindingProvider(this);
             AvatarTextures = new AvatarTextureProvider(this, Device);
            // AvatarSkeletons = new AvatarSkeletonProvider(this);
            // AvatarAppearances = new AvatarAppearanceProvider(this);
            AvatarOutfits = new AvatarOutfitProvider(this);
            //AvatarAnimations = new AvatarAnimationProvider(this);
            AvatarPurchasables = new AvatarPurchasables(this);
            AvatarHandgroups = new HandgroupProvider(this, Device);
            AvatarCollections = new AvatarCollectionsProvider(this);
            AvatarThumbnails = new AvatarThumbnailProvider(this, Device);

            TS1Global = new TS1Provider(this);
            TS1AvatarTextures = new TS1AvatarTextureProvider(TS1Global);
            TS1AvatarMeshes = new TS1BMFProvider(TS1Global);
            //Jobs = new TS1JobProvider(TS1Global);
            Neighborhood = new TS1NeighborhoodProvider(this);

            WorldObjects = new WorldObjectProvider(this);
            WorldFloors = new WorldFloorProvider(this);
            WorldWalls = new WorldWallProvider(this);
            WorldObjectGlobals = new WorldGlobalProvider(this);
            WorldCatalog = new WorldObjectCatalog();
            WorldRoofs = new WorldRoofProvider(this);


            BCFGlobal = new TS1BCFProvider(this, TS1Global);
            AvatarAnimations = new TS1BCFAnimationProvider(BCFGlobal);
            AvatarSkeletons = new TS1BCFSkeletonProvider(BCFGlobal);
            AvatarAppearances = new TS1BCFAppearanceProvider(BCFGlobal);
            Audio = new TS1Audio(this);
            //Audio = new Audio(this);
            GlobalTuning = new Tuning(Path.Combine("Content", "tuning.dat"));

            Init();
        }

        /// <summary>
        /// Initiates loading for world.
        /// </summary>
        public void InitWorld()
        {
            WorldObjects.Init((Device != null));
            WorldObjectGlobals.Init();
            WorldWalls.InitTS1();
            WorldFloors.InitTS1();
            WorldCatalog.Init(this);
            WorldRoofs.Init();
        }

        /// <summary>
        /// Setup the content manager so it knows where to find various files.
        /// </summary>
        private void Init()
        {
            /** Scan system for files **/
            var allFiles = new List<string>();
            _ScanFiles(BasePath, allFiles);
            AllFiles = allFiles.ToArray();

            var ts1AllFiles = new List<string>();
            var oldBase = BasePath;
            
                _ScanFiles(BasePath, ts1AllFiles);
                TS1AllFiles = ts1AllFiles.ToArray();

            PIFFRegistry.Init(Path.Combine(FSOEnvironment.ContentDir, "Patch/"));
            Archives = new Dictionary<string, FAR3Archive>();
            UIGraphics.Init();

            TS1Global.Init();
            //BCFGlobal.Init();
            TS1AvatarTextures.Init();
            TS1AvatarMeshes.Init();
            AvatarMeshes.Init();
            AvatarBindings.Init();
            AvatarTextures.Init();
            //AvatarSkeletons.Init();
            //((AvatarAppearanceProvider)AvatarAppearances).Init();
            AvatarOutfits.Init();
            //((AvatarAnimationProvider)AvatarAnimations).Init();
            Audio.Init();
            AvatarPurchasables.Init();
            AvatarCollections.Init();
            AvatarHandgroups.Init();
            AvatarThumbnails.Init();


            InitWorld();
        }

        /// <summary>
        /// Scans a directory for a list of files.
        /// </summary>
        /// <param name="dir">The directory to scan.</param>
        /// <param name="fileList">The list of files to scan for.</param>
        private void _ScanFiles(string dir, List<string> fileList)
        {
            var fullPath = dir;
            var files = Directory.GetFiles(fullPath);
            foreach (var file in files)
            {
                fileList.Add(file.Substring(BasePath.Length));
            }

            var dirs = Directory.GetDirectories(fullPath);
            foreach (var subDir in dirs)
            {
                _ScanFiles(subDir, fileList);
            }
        }

        /// <summary>
        /// Gets a path relative to the client's directory.
        /// </summary>
        /// <param name="path">The path to combine with the client's directory.</param>
        /// <returns>The path combined with the client's directory.</returns>
        public string GetPath(string path)
        {
            return Path.Combine(BasePath, path);
        }

        private Dictionary<string, FAR3Archive> Archives;

        /// <summary>
        /// Gets a resource using a path and ID.
        /// </summary>
        /// <param name="path">The path to the file. If this path is to an archive, assetID can be null.</param>
        /// <param name="assetID">The ID for the resource. Can be null if path doesn't point to an archive.</param>
        /// <returns></returns>
        public Stream GetResource(string path, ulong assetID)
        {
            if (path.EndsWith(".dat"))
            {
                /** Archive **/
                if (!Archives.ContainsKey(path))
                {
                    FAR3Archive newArchive = new FAR3Archive(GetPath(path));
                    Archives.Add(path, newArchive);
                }

                var archive = Archives[path];
                var bytes = archive.GetItemByID(assetID);
                return new MemoryStream(bytes, false);
            }

            if (path.EndsWith(".bmp") || path.EndsWith(".png") || path.EndsWith(".tga")) path = "uigraphics/" + path;

            return File.OpenRead(GetPath(path));
        }

        /** World **/
        public WorldObjectProvider WorldObjects;
        public WorldGlobalProvider WorldObjectGlobals;
        public WorldFloorProvider WorldFloors;
        public WorldWallProvider WorldWalls;
        public WorldObjectCatalog WorldCatalog;
        public WorldRoofProvider WorldRoofs;

        public UIGraphicsProvider UIGraphics;
        
        /** Avatar **/
        public AvatarMeshProvider AvatarMeshes;
        public AvatarBindingProvider AvatarBindings;
        public AvatarTextureProvider AvatarTextures;
        public TS1BCFSkeletonProvider AvatarSkeletons;
        public IContentProvider<Appearance> AvatarAppearances;
        public AvatarOutfitProvider AvatarOutfits;
        public IContentProvider<Animation> AvatarAnimations;
        public AvatarPurchasables AvatarPurchasables;
        public HandgroupProvider AvatarHandgroups;
        public AvatarCollectionsProvider AvatarCollections;
        public AvatarThumbnailProvider AvatarThumbnails;

        public TS1Provider TS1Global;
        public TS1BCFProvider BCFGlobal;
        public TS1AvatarTextureProvider TS1AvatarTextures;
        public TS1BMFProvider TS1AvatarMeshes;
        TS1JobProvider Jobs;
        TS1NeighborhoodProvider Neighborhood;

        /** Audio **/
        public IAudioProvider Audio;

        /** GlobalTuning **/
        public Tuning GlobalTuning;
    }
}
