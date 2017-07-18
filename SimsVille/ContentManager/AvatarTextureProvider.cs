using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TSO.Content.framework;
using TSOVille.Code.UI.Model;


namespace TSO.Content
{

       public class TexturesProvider : TS1SubProvider<ITextureRef>
       {
           public TexturesProvider(TS1Provider baseProvider)
               : base(baseProvider, ".bmp")
           {
           }

           public override ITextureRef Get(string name)
           {
               return base.Get(name.Replace(".jpg", "").ToLowerInvariant() + ".bmp");
           }
       }

}
