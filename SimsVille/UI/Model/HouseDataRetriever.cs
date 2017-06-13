/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
Rhys Simpson. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using TSO.Files;
using tso.world.Model;

namespace SimsVille.UI.Model
{
    public class HouseDataRetriever
    {
        public List<LotTileEntry>  LotTileData; //the renderer requests this on entry, so it must be populated before initilizing the CityRenderer!
        public List<Texture2D> HousesImages;
        public XmlCity CityData;
        public GraphicsDevice GfxDevice;

        public HouseDataRetriever(GraphicsDevice GfxDevice)
        {
            this.GfxDevice = GfxDevice;
            LotTileData = new List<LotTileEntry>();
            HousesImages = new List<Texture2D>();

        }


        public void GetCityLots()
        {

            CityData = XmlCity.Parse("Content/City.xml");


            foreach (XmlLot lot in CityData.Lots)
                LotTileData.Add(new LotTileEntry(lot.Id, lot.Name, 
                    (short)lot.X, (short)lot.Y, (byte)lot.Flags, lot.Cost));

             if (CityData.Lots.Count > 0) 
            foreach (LotTileEntry lotentry in LotTileData)
                HousesImages.Add(RetrieveHouseGFX(lotentry.id, lotentry.name));

        }

        public Texture2D RetrieveHouseGFX(int id, string name) {

            Texture2D HouseImg = null;
            
            try
            {
                    HouseImg = Texture2D.FromStream(GfxDevice, 
                    new FileStream(@"Houses/" + name.ToString() + ".png", FileMode.Open, FileAccess.Read, FileShare.Read)); 
                

            } catch (Exception) {
            }

            return HouseImg;
        }

        
    }


    public class LotTileEntry
    {
        public int id;
        public short x;
        public short y;
        public byte flags; //bit 0 = online, bit 1 = spotlight, bit 2 = locked, bit 3 = occupied
        public int cost;
        public string name;

        public LotTileEntry(int Lotid, string Name, short X, short Y, byte Flags, int Cost)
        {
            this.id = Lotid;
            this.x = X;
            this.y = Y;
            this.flags = Flags;
            this.cost = Cost;
            this.name = Name;
        }
    }

}
