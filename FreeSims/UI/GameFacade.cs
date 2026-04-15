/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Model;
using System.IO;
using System.Threading;
using FSO.Client.Utils;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework;
using FSO.Client.UI.Panels;
using FSO.Client.GameContent;
using FSO.Client.UI;
using Microsoft.Xna.Framework;
using FSO.Client.Rendering.City;
using SimsVille.UI.Model;

namespace FSO.Client
{
    /// <summary>
    /// Central point for accessing game objects
    /// </summary>
    public class GameFacade
    {
        public static ContentStrings Strings;
        public static GameController Controller;
        public static UILayer Screens;
        public static _3DLayer Scenes;
        public static GraphicsDevice GraphicsDevice;
        public static GraphicsDeviceManager GraphicsDeviceManager;
        public static TSOGame Game;
        //public static TSOClientTools DebugWindow;
        public static Font MainFont;
        public static Font EdithFont;
        public static UpdateState LastUpdateState;
        public static Thread GameThread;
        public static bool Focus = true;

        public static bool Linux;
        public static bool DirectX;

        public static CursorManager Cursor;
        public static UIMessageController MessageController = new UIMessageController();

        //Entries received from city server, see UIPacketHandlers.OnCityTokenResponse()
        public static CityDataRetriever CDataRetriever = new CityDataRetriever();
        public static HouseDataRetriever HousesDataRetriever;
        /// <summary>
        /// Place where the game can store cached values, e.g. pre modified textures to improve
        /// 2nd load speed, etc.
        /// </summary>
        public static string CacheDirectory;
        public static string CacheRoot = @"TSOCache/";

        public static void Init()
        {

            HousesDataRetriever = new HouseDataRetriever(GraphicsDevice);
            HousesDataRetriever.GetCityLots();

        }

        /**
         * Important top level events
         */
        public static event BasicEventHandler OnContentLoaderReady;

        public static string GameFilePath(string relativePath)
        {
            return Path.Combine(GlobalSettings.Default.StartupPath, relativePath);
        }

        /// <summary>
        /// This gets the number of a city when provided with a name.
        /// </summary>
        /// <param name="CityName">Name of the city.</param>
        /// <returns>Number of the city.</returns>
        public static int GetCityNumber(string CityName)
        {

            return 1;
        }


        /// <summary>
        /// Kills the application.
        /// </summary>
        public static void Kill()
        {
            //TODO: Add any needed deconstruction here.
            Game.Exit();
        }

        public static void TriggerContentLoaderReady()
        {
            if (OnContentLoaderReady != null)
            {
                OnContentLoaderReady();
            }
        }

        public static TimeSpan GameRunTime
        {
            get
            {
                if (LastUpdateState != null && LastUpdateState.Time != null)
                {
                    return LastUpdateState.Time.TotalGameTime;
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }
    }

    public delegate void BasicEventHandler();

}
