using FSO.LotView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSO.Client
{
    /// <summary>
    /// To avoid dynamically linking monogame from Program.cs (where we have to choose the correct version for the OS),
    /// we use this mediator class.
    /// </summary>
    public class GameStartProxy
    {
        public TSOGame Game;

        public void Start(bool useDX)
        {
			Game = new TSOGame();
            GameFacade.DirectX = useDX;
			World.DirectX = useDX;
            Game.Run();
            Game.Dispose();
        }

		public void SetPath(string path)
		{
			GlobalSettings.Default.StartupPath = path;
            GlobalSettings.Default.Windowed = false;
		}
	}
}
