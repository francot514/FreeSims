/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FSO.Common;

namespace FSO.LotView
{
    /// <summary>
    /// Handles XNA content for the world.
    /// </summary>
    public class WorldContent
    {
        public static ContentManager ContentManager;

        public static void Init(GameServiceContainer serviceContainer, string rootDir)
        {
            ContentManager = new ContentManager(serviceContainer);
            ContentManager.RootDirectory = rootDir;
        }

        public static string EffectSuffix
        {
            get { return ((FSOEnvironment.SoftwareDepth)?"iOS":""); }
        }

        public static Effect _2DWorldBatchEffect
        {
            get{

                if (ContentManager != null)
                return ContentManager.Load<Effect>("Effects/2DWorldBatch"+EffectSuffix);

                return null;
            }
        }

        public static Effect GrassEffect
        {
            get
            {
                if (ContentManager != null)
                 return ContentManager.Load<Effect>("Effects/GrassShader"+EffectSuffix);

                return null;
               
            }
        }

        public static Texture2D GridTexture
        {
            get
            {
                if (ContentManager != null)
                return ContentManager.Load<Texture2D>("Textures/gridTexture");


                return null;
            }
        }
    }
}
