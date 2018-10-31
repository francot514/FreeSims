using FSO.Common;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Content.TS1
{
    /// <summary>
    /// Provides families, neighbors, neighborhood structure and more given a current userdata folder. Should also allow the game to save these things.
    /// </summary>
    public class TS1NeighborhoodProvider
    {
        public Files.Formats.IFF.IffFile MainResource;
        public Files.Formats.IFF.IffFile LotLocations;
        public Files.Formats.IFF.IffFile StreetNames;
        public Files.Formats.IFF.IffFile NeighbourhoodDesc;
        public Files.Formats.IFF.IffFile STDesc;
        public Files.Formats.IFF.IffFile MTDesc;
        public Dictionary<short, short> ZoningDictionary = new Dictionary<short, short>();
        public NBRS Neighbors;
        public NGBH Neighborhood;
        public TATT TypeAttributes;
        public Dictionary<short, FAMI> FamilyForHouse = new Dictionary<short, FAMI>();
        public Content ContentManager;
        public TS1GameState GameState = new TS1GameState();
        public string UserPath;

        public HashSet<uint> DirtyAvatars = new HashSet<uint>();

        public TS1NeighborhoodProvider(Content contentManager)
        {
            ContentManager = contentManager;
            InitSpecific(1);
        }

        /// <summary>
        /// Intializes a specific neighbourhood. Also counts as a save discard, since it unloads the current neighbourhood.
        /// </summary>
        /// <param name="id"></param>
        public void InitSpecific(int id)
        {
            DirtyAvatars.Clear();
            ZoningDictionary.Clear();
            FamilyForHouse.Clear();

            var udName = "UserData" + ((id == 0) ? "" : (id+1).ToString());
            //simitone shouldn't modify existing ts1 data, since our house saves are incompatible.
            //therefore we should copy to the simitone user data.

            var userPath = FSOEnvironment.ContentDir;

               
            UserPath = userPath;

            MainResource = new Files.Formats.IFF.IffFile(Path.Combine(UserPath, "Neighborhood.iff"));

            Neighbors = MainResource.List<NBRS>().FirstOrDefault();
            Neighborhood = MainResource.List<NGBH>().FirstOrDefault();


            //todo: manage avatar iffs here
        }

        public Neighbour GetNeighborByID(short ID)
        {
            Neighbour result = null;
            Neighbors.NeighbourByID.TryGetValue(ID, out result);
            return result;
        }

        public FAMI GetFamilyForHouse(short ID)
        {
            FAMI result = null;
            FamilyForHouse.TryGetValue(ID, out result);
            return result;
        }

        public short? GetNeighborIDForGUID(uint GUID)
        {
            short result = 0;
            if (Neighbors.DefaultNeighbourByGUID.TryGetValue(GUID, out result))
                return result;
            return null;
        }
        public short SetToNext(short current)
        {
            Neighbour neighbor = Neighbors.Entries.FirstOrDefault(x => x.NeighbourID > current);

            return (neighbor != null) ? neighbor.NeighbourID : (short)-1;
        }

        public short SetToNext(short current, uint guid)
        {

            Neighbour neighbor = Neighbors.Entries.FirstOrDefault(x => x.NeighbourID > current && x.GUID == guid);

            return (neighbor != null) ? neighbor.NeighbourID : (short)-1;
        }

        public bool SaveNeighbourhood(bool withSims)
        {
            //todo: save iffs for dirty avatars. 
            DirtyAvatars.Clear();

            using (var stream = new FileStream(Path.Combine(UserPath, "Neighborhood.iff"), FileMode.Create, FileAccess.Write, FileShare.None))
                MainResource.Write(stream);

            return true;
        }

        public bool SaveHouse(int houseID, Files.Formats.IFF.IffFile file)
        {
            using (var stream = new FileStream(GetHousePath(houseID), FileMode.Create, FileAccess.Write, FileShare.None))
                file.Write(stream);

            return true;
        }

        public IffFile GetHouse(int id)
        {
            return new Files.Formats.IFF.IffFile(Path.Combine(UserPath, "Houses/House"+ id.ToString().PadLeft(2, '0')+".iff"));
        }

        public string GetHousePath(int id)
        {
            return Path.Combine(UserPath, "Houses/House" + id.ToString().PadLeft(2, '0') + ".iff");
        }

        public BMP GetHouseThumb(int id)
        {
            return GetHouse(id).Get<BMP>(512); //roof on
        }

        public short GetTATT(uint guid, int index)
        {
            short[] dat = null;
            if (TypeAttributes.TypeAttributesByGUID.TryGetValue(guid, out dat))
            {
                if (index >= dat.Length) return 0;
                else return dat[index];
            }
            return 0;
        }

        public Tuple<string, string> GetHouseNameDesc(int houseID)
        {
            STR res;
            if (houseID < 80) res = NeighbourhoodDesc.Get<STR>((ushort)(houseID + 2000));
            else if (houseID < 90) res = STDesc.Get<STR>((ushort)(houseID + 2000));
            else res = MTDesc.Get<STR>((ushort)(houseID + 2000));

            if (res == null) return new Tuple<string, string>("", "");
            else return new Tuple<string, string>(res.GetString(0), res.GetString(1));
        }

        public void SetTATT(uint guid, int index, short value)
        {
            short[] dat = null;
            if (!TypeAttributes.TypeAttributesByGUID.TryGetValue(guid, out dat))
            {
                var obj = ContentManager.WorldObjects.Get(guid);
                if (obj == null) return;
                dat = new short[32];
                TypeAttributes.TypeAttributesByGUID[guid] = dat;
            }
            if (index >= dat.Length) return;
            else dat[index] = value;
        }
    }

    public class TS1GameState
    {
        public FAMI ActiveFamily;
        public uint DowntownSimGUID;
        public short LotTransitInfo;
    }
}
