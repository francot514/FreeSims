using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Files.HIT;

namespace TSO.HIT.model
{
    /// <summary>
    /// Groups related HIT resources, like the tsov2 series or newmain.
    /// </summary>
    public class HITResourceGroup
    {
        public EVT evt;
        public HITFile hit;
        public HSM hsm;
    }
}
