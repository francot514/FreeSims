using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Controls;
using tso.world.Model;
using TSO.Content;
using TSO.Vitaboy;

namespace TSOVille.Network
{
    public class DataController
    {

        public UISim SelectCharacter(string file)
            {

            UISim sim;

            XmlCharacter charInfo;

            AppearanceType type;

            charInfo = XmlCharacter.Parse(file);

            Enum.TryParse(charInfo.Appearance, out type);

            var headPurchasable = Content.Get().AvatarPurchasables.Get(Convert.ToUInt64(charInfo.Head, 16));
            var bodyPurchasable = Content.Get().AvatarPurchasables.Get(Convert.ToUInt64(charInfo.Body, 16));


            sim = new UISim(charInfo.ObjID, charInfo.Id){
            Name = charInfo.Name,
            Head = Content.Get().AvatarOutfits.Get(headPurchasable.OutfitID),
            Body = Content.Get().AvatarOutfits.Get(bodyPurchasable.OutfitID),
            HeadOutfitID = headPurchasable.OutfitID,
            BodyOutfitID = bodyPurchasable.OutfitID,
            Handgroup = Content.Get().AvatarOutfits.Get(bodyPurchasable.OutfitID),
           
                
                };

            sim.Avatar.Appearance = type;

            
            return sim;

            }

    }
}
