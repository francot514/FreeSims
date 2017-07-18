using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TSO.Content.framework;
using TSOVille.Code.UI.Model;
using TSO.Common.rendering.vitaboy;


namespace TSO.Content
{

       public class MeshesProvider : TS1SubProvider<Mesh>
       {
           public MeshesProvider(TS1Provider baseProvider)
               : base(baseProvider, ".bmf")
           {
           }

           public override Mesh Get(string name)
           {
               return base.Get(name.Replace(".mesh", "").ToLowerInvariant() + ".bmf");
           }
       }

    
}
