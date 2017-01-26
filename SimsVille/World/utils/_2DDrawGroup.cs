using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tso.world.Utils
{
    public class _2DDrawGroup
    {
        public _2DSpriteVertex[] Vertices;
        public short[] Indices;

        public Texture2D Pixel;
        public Texture2D Depth;
        public Texture2D Mask;
        public EffectTechnique Technique;
    }
}
