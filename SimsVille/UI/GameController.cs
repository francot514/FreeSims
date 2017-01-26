/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Screens;
using System.IO;



namespace TSOVille.Code
{
    /// <summary>
    /// Handles the game flow between various game modes, e.g. login => city view
    /// </summary>
    public class GameController
    {
        

        /// <summary>
        /// Start the preloading process
        /// </summary>
        public void StartLoading()
        {
            var screen = new MaxisLogo();

            GameFacade.Screens.AddScreen(screen);
            ContentManager.InitLoading();
        }


        public void ShowCity()
        {
            var screen = new CoreGameScreen();
            GameFacade.Screens.RemoveCurrent();
            GameFacade.Screens.AddScreen(screen);
        }

       

        public void ShowLot()
        {
            var screen = new CoreGameScreen(); //new LotDebugScreen();
            GameFacade.Screens.RemoveCurrent();
            GameFacade.Screens.AddScreen(screen);

            string house = "Content/Empty.xml";

            if (File.Exists("Houses/New.xml"))
                house = "Houses/New.xml";

          

            screen.InitTestLot(house);

            screen.ZoomLevel = 1;
        }


    }
}
