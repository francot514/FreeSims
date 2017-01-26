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
using TSO.Content.framework;
using System.Text.RegularExpressions;
using TSO.Content.codecs;
using Microsoft.Xna.Framework.Graphics;

namespace TSO.Content
{
    /// <summary>
    /// Provides access to floor (*.flr) data in FAR3 archives.
    /// </summary>
    public class WorldFloorProvider : IContentProvider<Floor>
    {
        private Content ContentManager;
        private Dictionary<ushort, Floor> ById;

        public Dictionary<ushort, FloorReference> Entries;
        public FAR1Provider<Iff> Floors;

        private Iff FloorGlobals;
        public int NumFloors;

        public WorldFloorProvider(Content contentManager)
        {
            this.ContentManager = contentManager;
        }

        /// <summary>
        /// Initiates loading of floors.
        /// </summary>
        public void Init()
        {

            this.Entries = new Dictionary<ushort, FloorReference>();
            this.ById = new Dictionary<ushort, Floor>();

            var floorGlobalsPath = ContentManager.GetPath("GameData/floors.iff");
            var floorGlobals = new Iff(floorGlobalsPath);
            FloorGlobals = floorGlobals;

            var buildGlobalsPath = ContentManager.GetPath("GameData/Build.iff");
            var buildGlobals = new Iff(buildGlobalsPath); //todo: centralize?

            /** There is a small handful of floors in a global file for some reason **/
            ushort floorID = 1;
            var floorStrs = buildGlobals.Get<STR>(130);
            for (ushort i = 1; i < 28; i++)
            {
                var far = floorGlobals.Get<SPR2>(i);
                var medium = floorGlobals.Get<SPR2>((ushort)(i + 256));
                var near = floorGlobals.Get<SPR2>((ushort)(i + 512)); //2048 is water tile

                this.AddFloor(new Floor
                {
                    ID = floorID,
                    Far = far,
                    Medium = medium,
                    Near = near
                });

                Entries.Add(floorID, new FloorReference(this)
                {
                    ID = floorID,
                    FileName = "global",

                    Name = floorStrs.GetString((i - 1) * 3 + 1),
                    Price = int.Parse(floorStrs.GetString((i - 1) * 3 + 0)),
                    Description = floorStrs.GetString((i - 1) * 3 + 2)
                });

                floorID++;
            }

            var waterStrs = buildGlobals.Get<STR>(133);
            //add pools for catalog logic
            Entries.Add(65535, new FloorReference(this)
            {
                ID = 65535,
                FileName = "global",

                Price = int.Parse(waterStrs.GetString(0)),
                Name = waterStrs.GetString(1),
                Description = waterStrs.GetString(2)
            });
            
            NumFloors = floorID;
            this.Floors = new FAR1Provider<Iff>(ContentManager, new IffCodec(), new Regex(".*\\\\floors.*\\.far"));
            Floors.Init(2);
        }

        private void AddFloor(Floor floor)
        {
            ById.Add(floor.ID, floor);
        }


        public Texture2D GetFloorThumb(ushort id, GraphicsDevice device)
        {
            if (id < 28)
            {
                return ById[id].Near.Frames[0].GetTexture(device);
            }
            else return null;
        }

        public SPR2 GetGlobalSPR(ushort id)
        {
            return FloorGlobals.Get<SPR2>(id);
        }

        #region IContentProvider<Floor> Members

        public Floor Get(ulong id)
        {
            if (ById.ContainsKey((ushort)id))
            {
                return ById[(ushort)id];
            }
            else
            {
                //get from iff
                if (!Entries.ContainsKey((ushort)id)) return null;
                Iff iff = this.Floors.Get(Entries[(ushort)id].FileName);
                if (iff == null) return null;

                var far = iff.Get<SPR2>(1);
                var medium = iff.Get<SPR2>(257);
                var near = iff.Get<SPR2>(513);

                ById[(ushort)id] = new Floor
                {
                    ID = (ushort)id,
                    Near = near,
                    Medium = medium,
                    Far = far
                };
                return ById[(ushort)id];
            }
        }

        public Floor Get(uint type, uint fileID)
        {
            return null;
        }

        public List<IContentReference<Floor>> List()
        {
            return new List<IContentReference<Floor>>(Entries.Values);
        }

        #endregion
    }

    public class FloorReference : IContentReference<Floor>
    {
        public ulong ID;
        public string FileName;

        public int Price; //remember these, just in place of a catalog
        public string Name;
        public string Description;

        private WorldFloorProvider Provider;

        public FloorReference(WorldFloorProvider provider)
        {
            this.Provider = provider;
        }

        #region IContentReference<Floor> Members

        public Floor Get()
        {
            return Provider.Get(ID);
        }

        #endregion
    }
}
