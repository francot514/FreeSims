using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TSO.Common.rendering.vitaboy;
using FSO.Content.Framework;
using FSO.Vitaboy;


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
