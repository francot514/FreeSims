/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the TSOVille.

The Initial Developer of the Original Code is
RHY3756547. All Rights Reserved.

Contributor(s): ______________________________________.
*/


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.UI.Framework.Parser;
using TSOVille.Code.UI.Model;
using TSO.Common.rendering.framework.model;
using TSO.Common.rendering.framework.io;
using TSOVille.Code.Utils;

namespace TSOVille.Code.UI.Controls
{
    /// <summary>
    /// The motive display used in live mode. Labels, values and increment rate indicators can be custom set.
    /// </summary>
    public class UISkillDisplay : UIElement
    {
        public short[] SkillValues;
        public string[] SkillNames = {
        "Cooking",
        "Charisma",
        "Mechanical",
        "Creativity",
        "Body",
        "Logic"
        };
        private Texture2D Filler;

        public UISkillDisplay()
        {
            SkillValues = new short[8];
            Filler = TextureUtils.TextureFromColor(GameFacade.GraphicsDevice, Color.White);
        }

        private void DrawSkill(UISpriteBatch batch, int x, int y, int skill)
        {
            double p = (SkillValues[skill] + 100) / 200.0;
            Color barcol = new Color((byte)(57 * (1 - p)), (byte)(213 * p + 97 * (1 - p)), (byte)(49 * p + 90 * (1 - p)));
            Color bgcol = new Color((byte)(57 * p + 214*(1-p)), (byte)(97 * p), (byte)(90 * p));

            batch.Draw(Filler, LocalRect(x, y, 60, 5), bgcol);
            batch.Draw(Filler, LocalRect(x, y, (int)(60*p), 5), barcol);
            batch.Draw(Filler, LocalRect(x+(int)(60 * p), y, 1, 5), Color.Black); 
            var style = TextStyle.DefaultLabel.Clone();
            style.Size = 8;

            var temp = style.Color;
            style.Color = Color.Black;
            DrawLocalString(batch, SkillNames[skill], new Vector2(x + 1, y - 12), style, new Rectangle(0, 0, 60, 12), TextAlignment.Center); //shadow

            style.Color = temp;
            DrawLocalString(batch, SkillNames[skill], new Vector2(x, y - 13), style, new Rectangle(0, 0, 60, 12), TextAlignment.Center);
        }

        public override void Draw(UISpriteBatch batch)
        {
            for (int i = 0; i < 4; i++)
            {
                DrawSkill(batch, 200, 13 + 20 * i, i); //left side
                DrawSkill(batch, 300, 13 + 20 * i, i + 1); //right side
            }
        }
    }
}
