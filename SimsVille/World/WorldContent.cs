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
using FSO.LotView.Effects;

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
            get
            {
                return ContentManager.Load<Effect>("Effects/2DWorldBatch" + EffectSuffix);
            }
            
        }

        public static LightMap2DEffect LightEffect
        {
            get
            {
                return new LightMap2DEffect(ContentManager.Load<Effect>("Effects/LightMap2D"));
            }

        }

        public static GrassEffect GrassEffect
        {
            get
            {
                return new GrassEffect(ContentManager.Load<Effect>("Effects/GrassShader"+EffectSuffix));
            }
            

        }
        public static Texture2D GridTexture
        {
            get
            {
                return ContentManager.Load<Texture2D>("Textures/gridTexture");
            }
        }
    }
}
