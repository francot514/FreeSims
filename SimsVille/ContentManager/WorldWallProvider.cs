using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSO.Common.content;
using TSO.Content.model;
using TSO.Files.formats.iff;
using TSO.Files.formats.iff.chunks;
using TSO.Files.FAR1;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using TSO.Content.framework;
using TSO.Content.codecs;
using System.Text.RegularExpressions;
using TSO.Files.FAR3;
using TSOVille.Code.Utils;

namespace TSO.Content
{
    /// <summary>
    /// Provides access to wall (*.wll) data in FAR3 archives.
    /// </summary>
    public class WorldWallProvider : IContentProvider<Wall>
    {
        private Content ContentManager;
        public Wall Junctions;
        private List<WallStyle> WallStyles;
        private Dictionary<ushort, Wall> ById;
        private Dictionary<ushort, WallStyle> StyleById;
        private Iff WallGlobals;

        public ushort[] WallStyleIDs =
        {
            0x1, //wall
            0x2, //picket fence
            0xD, //iron fence
            0xC, //privacy fence
            0xE //banisters
        };

        public Dictionary<ushort, WallReference> Entries;

        public IFFProvider<Iff> Walls;

        public Dictionary<ushort, int> WallStyleToIndex;

        public int NumWalls;

        public WorldWallProvider(Content contentManager)
        {
            this.ContentManager = contentManager;
        }

        /// <summary>
        /// Initiates loading of walls.
        /// </summary>
        public void Init()
        {
            WallStyleToIndex = WallStyleIDs.ToDictionary(x => x, x => Array.IndexOf(WallStyleIDs, x));

            this.Entries = new Dictionary<ushort, WallReference>();
            this.ById = new Dictionary<ushort, Wall>();
            this.StyleById = new Dictionary<ushort, WallStyle>();
            this.WallStyles = new List<WallStyle>();

            var wallGlobalsPath = ContentManager.GetPath("GameData/walls.iff");
            WallGlobals = new Iff(wallGlobalsPath);

            var buildGlobalsPath = ContentManager.GetPath("GameData/Build.iff");
            var buildGlobals = new Iff(buildGlobalsPath); //todo: centralize?

            /** Get wall styles from globals file **/
            ushort wallID = 1;
            for (ushort i = 2; i < 512; i+=2)
            {
                var far = WallGlobals.Get<SPR>((ushort)(i));
                var medium = WallGlobals.Get<SPR>((ushort)(i + 512));
                var near = WallGlobals.Get<SPR>((ushort)(i + 1024));

                var fard = WallGlobals.Get<SPR>((ushort)(i + 1));
                var mediumd = WallGlobals.Get<SPR>((ushort)(i + 513));
                var neard = WallGlobals.Get<SPR>((ushort)(i + 1025));

                if (fard == null)
                { //no walls down, just render exactly the same
                    fard = far;
                    mediumd = medium;
                    neard = near;
                }

                var styleStrs = buildGlobals.Get<STR>(0x81);

                string name = null, description = null;
                int price = -1;
                int buyIndex = -1;
                WallStyleToIndex.TryGetValue(wallID, out buyIndex);
                
                if (buyIndex != -1)
                {
                    price = int.Parse(styleStrs.GetString(buyIndex * 3));
                    name = styleStrs.GetString(buyIndex * 3 + 1);
                    description = styleStrs.GetString(buyIndex * 3 + 2);
                }

                this.AddWallStyle(new WallStyle
                {
                    ID = wallID,
                    WallsUpFar = far,
                    WallsUpMedium = medium,
                    WallsUpNear = near,
                    WallsDownFar = fard,
                    WallsDownMedium = mediumd,
                    WallsDownNear = neard,

                    Price = price,
                    Name = name,
                    Description = description
                });

                wallID++;
            }

            DynamicStyleID = 256; //styles loaded from objects start at 256. The objd reference is dynamically altered to reference this new id, 
            //so only refresh wall cache at same time as obj cache! (do this on lot unload)

            /** Get wall patterns from globals file **/
            var wallStrs = buildGlobals.Get<STR>(129);

            wallID = 0;

            

            for (ushort i = 0; i < 29; i++)
            {
                var far = WallGlobals.Get<SPR>((ushort)(i+1536));
                var medium = WallGlobals.Get<SPR>((ushort)(i + 1536 + 256));
                var near = WallGlobals.Get<SPR>((ushort)(i + 1536 + 512));

                this.AddWall(new Wall
                {
                    ID = wallID,
                    Far = far,
                    Medium = medium,
                    Near = near,
                });



                if (i > 0 && i < (wallStrs.Length / 3) + 1)
                {
                    Entries.Add(wallID, new WallReference(this)
                    {
                        ID = wallID,
                        FileName = "global",

                        Name = wallStrs.GetString((i-1)*3+1),
                        Price = int.Parse(wallStrs.GetString((i - 1) * 3 + 0)),
                        Description = wallStrs.GetString((i - 1) * 3 + 2)
                    });

                    
                }

                wallID++;
            }

            Junctions = new Wall
                {
                    ID = wallID,
                    Far = WallGlobals.Get<SPR>(4096),
                    Medium = WallGlobals.Get<SPR>(4097),
                    Near = WallGlobals.Get<SPR>(4098),
                };


            this.Walls = new IFFProvider<Iff>(ContentManager, new IffCodec(), wallGlobalsPath);
            Walls.Init();
            NumWalls = wallID;
        }

        private ushort DynamicStyleID;


        /// <summary>
        /// Adds a dynamic wall style to WorldWallProvider.
        /// </summary>
        /// <param name="input">Wallstyle to add.</param>
        /// <returns>The ID of the wallstyle.</returns>
        public ushort AddDynamicWallStyle(WallStyle input) //adds a new wall and returns its id
        {
            input.ID = DynamicStyleID++;
            AddWallStyle(input);
            return (ushort)(DynamicStyleID - 1);
        }

        private void AddWall(Wall wall)
        {
           
            ById.Add(wall.ID, wall);
        }

        private void AddWallStyle(WallStyle wall)
        {
            WallStyles.Add(wall);
            StyleById.Add(wall.ID, wall);
        }

        /// <summary>
        /// Gets a wallstyle instance from WorldWallProvider.
        /// </summary>
        /// <param name="id">The ID of the wallstyle.</param>
        /// <returns>A WallStyle instance.</returns>
        public WallStyle GetWallStyle(ulong id)
        {
            if (StyleById.ContainsKey((ushort)id))
            {
                return StyleById[(ushort)id];
            }
            return null;
        }

        public Texture2D GetWallThumb(ushort id, GraphicsDevice device)
        {
            if (id < 29)
            {
                var spr = ById[id].Medium;
                return (spr == null)?null:spr.Frames[2].GetTexture(device);
                
            }
            else
            {
                var iff = this.Walls.MainIff;
                var spr = iff.Get<SPR>(1793);
                return (spr == null)?null:spr.Frames[2].GetTexture(device);
            }
        }


        public BMP GetWallStyleIcon(ushort id)
        {
            return WallGlobals.Get<BMP>(id);
        }

        #region IContentProvider<Wall> Members

        public Wall Get(string id)
        {


            return new Wall();
        }

        public Wall Get(ulong id)
        {
            if (ById.ContainsKey((ushort)id))
            {
                return ById[(ushort)id];
            }
            else
            {
                //get from iff

                Iff iff = this.Walls.MainIff;
                var far = iff.Get<SPR>(1);
                var medium = iff.Get<SPR>(1793);
                var near = iff.Get<SPR>(2049);

                ById[(ushort)id] = new Wall
                {
                    ID = (ushort)id,
                    Near = near,
                    Medium = medium,
                    Far = far
                };
                
                }

                return ById[(ushort)id];

            
        }

        public Wall Get(uint type, uint fileID)
        {
            return null;
        }

        public List<IContentReference<Wall>> List()
        {
            return new List<IContentReference<Wall>>(Entries.Values);
        }

        #endregion
    }

    public class WallReference : IContentReference<Wall>
    {
        public ulong ID;
        public string FileName;

        public int Price; //remember these, just in place of a catalog
        public string Name;
        public string Description;

        private WorldWallProvider Provider;

        public WallReference(WorldWallProvider provider)
        {
            this.Provider = provider;
        }

        #region IContentReference<Wall> Members

        public Wall Get()
        {
            return Provider.Get(ID);
        }

        #endregion
    }
}
