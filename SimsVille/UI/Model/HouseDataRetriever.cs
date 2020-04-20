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
using tso.world.Model;
using FSO.Client.Rendering.City;

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
                HousesImages.Add(RetrieveHouseGFX(lotentry.lotid, lotentry.name));

        }

        public Texture2D RetrieveHouseGFX(int id, string name) {

            Texture2D HouseImg = null;
            
            try
            {
                if (File.Exists("Content/Houses/" + name.ToString() + ".png"))
                HouseImg = Texture2D.FromStream(GfxDevice,
                new FileStream("Content/Houses/" + name.ToString() + ".png", FileMode.Open, FileAccess.Read, FileShare.Read)); 
                

            } catch (Exception) {
            }

            return HouseImg;
        }

        
    }



}
