using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using TSO.Content.codecs;
using System.Text.RegularExpressions;
using TSO.Content.framework;


namespace TSO.Content
{
    /// <summary>
    /// Provides access to avatar thumbnail data in FAR3 archives.
    /// </summary>
    public class AvatarThumbnailProvider : PackingslipProvider<Texture2D>
    {
        public AvatarThumbnailProvider(Content contentManager, GraphicsDevice device)
            : base(contentManager, "packingslips/thumbnails.xml", new TextureCodec(device))
        {
        }
    }
}
