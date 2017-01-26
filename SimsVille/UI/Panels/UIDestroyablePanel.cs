using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Framework;

namespace TSOVille.Code.UI.Panels
{
    public abstract class UIDestroyablePanel : UIContainer
    {
        //just a panel with a destroy function, so that any hooks can be detached.
        public abstract void Destroy();
    }
}
