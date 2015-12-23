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
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TSOVille.Code.UI.Controls;
using TSOVille.Code.UI.Framework;

namespace TSOVille.Code.UI.Model
{
    public class SlicedTextureRef : ITextureRef
    {
        public Texture2D Texture;
        public Rectangle Margins;

        private NineSliceMargins NineSlice;

        public SlicedTextureRef(Texture2D texture, Rectangle margins)
        {
            this.Texture = texture;
            this.Margins = margins;

            NineSlice = new NineSliceMargins {
                Left = margins.Left,
                Top = margins.Top,
                Right = margins.Width,
                Bottom = margins.Height
            };
            NineSlice.CalculateOrigins(texture);
        }

        public void Draw(SpriteBatch SBatch, UIElement element, float x, float y, float width, float height)
        {
            //TODO: Cache scales for various sizes?
            NineSlice.CalculateScales(width, height);
            NineSlice.DrawOntoPosition(SBatch, element, Texture, width, height, new Vector2(x, y));
        }
    }


    public interface ITextureRef
    {
        void Draw(SpriteBatch SBatch, UIElement element, float x, float y, float width, float height);
    }
}
