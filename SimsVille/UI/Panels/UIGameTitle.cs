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
using TSOVille.Code.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.LUI;
using TSOVille.Code.UI.Controls;
using TSOVille.Code.UI.Screens;
using Microsoft.Xna.Framework;
using SimsHomeMaker;

namespace TSOVille.Code.UI.Panels
{
    //in matchmaker displays title of city. in lot displays lot name.

    public class UIGameTitle : UIContainer
    {
        public UIImage Background;
        public UILabel Label;

        public UIGameTitle()
        {
            Background = new UIImage(GetTexture((ulong)0x000001A700000002));
            Background.With9Slice(40, 40, 0, 0);
            this.AddAt(0, Background);
            Background.BlockInput();

            Label = new UILabel();
            Label.CaptionStyle = TextStyle.DefaultLabel.Clone();
            Label.CaptionStyle.Size = 11;
            Label.Alignment = TextAlignment.Middle;
            this.Add(Label);

            SetTitle("Not Blazing Falls");
        }

        public void SetTitle(string title)
        {
            Label.Caption = title;

            var style = Label.CaptionStyle;

            var width = style.MeasureString(title).X;
            var ScreenWidth = GlobalSettings.GraphicsWidth/2;

            Background.X = ScreenWidth-(width / 2 + 40);
            Background.SetSize(width + 80, 24);

            Label.X = ScreenWidth-width/2;
            Label.Size = new Vector2(width, 20);

        }
    }
}
