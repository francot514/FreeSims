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
using System.Linq;
using System.Text;
using TSOVille.Code.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.LUI;
using TSOVille.Code.UI.Controls;
using Microsoft.Xna.Framework;
using TSO.SimsAntics;
using TSO.SimsAntics.Model;
using SimsHomeMaker;

namespace TSOVille.Code.UI.Panels
{
    /// <summary>
    /// Live Mode Panel
    /// </summary>
    public class UILiveMode : UIDestroyablePanel
    {
        public UIImage Thumbnail;
        public UIImage Background;
        public UIImage Divider;
        public UIMotiveDisplay MotiveDisplay;
        public UISkillDisplay SkillDisplay;
        public Texture2D DividerImg { get; set; }
        public Texture2D PeopleListBackgroundImg { get; set; }
        public Texture2D EODButtonLayoutNoneImg { get; set; }
        public Texture2D MoodPositiveImg { get; set; }
        public Texture2D MoodNegativeImg { get; set; }

        //EOD buttons
        public UIButton EODHelpButton { get; set; }
        public UIButton EODCloseButton { get; set; }
        public UIButton EODExpandButton { get; set; }
        public UIButton EODContractButton { get; set; }

        public UIButton MoodPanelButton;
 
        //public VMAvatar SelectedAvatar;
        //public List<VMAvatar> Avatars;


        public UILiveMode () {
            var script = this.RenderScript("livepanel"+((GlobalSettings.GraphicsWidth < 1024)?"":"1024")+".uis");

            Background = new UIImage(GetTexture((GlobalSettings.GraphicsWidth < 1024) ? (ulong)0x000000D800000002 : (ulong)0x0000018300000002));
            Background.Y = 33;
            Background.BlockInput();
            this.AddAt(0, Background);

            MoodPanelButton = new UIButton();
            MoodPanelButton.Texture = GetTexture((ulong)FileIDs.UIFileIDs.lpanel_moodpanelbtn);
            MoodPanelButton.ImageStates = 4;
            MoodPanelButton.Position = new Vector2(31, 63);
            this.Add(MoodPanelButton);

            
            Thumbnail = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.thumbtemplate1frame));
            Thumbnail.Size = new Point(33, 33);
            Thumbnail.Position = new Vector2(63, 73);
            Thumbnail.BlockInput();
            this.Add(Thumbnail);


            var PeopleListBg = new UIImage(PeopleListBackgroundImg);
            PeopleListBg.Position = new Microsoft.Xna.Framework.Vector2(375, 38);
            this.AddAt(1, PeopleListBg);

            Divider = new UIImage(DividerImg);
            Divider.Position = new Microsoft.Xna.Framework.Vector2(140, 49);
            this.AddAt(1, Divider);


            EODHelpButton.Visible = false;
            EODCloseButton.Visible = false;
            EODExpandButton.Visible = false;
            EODContractButton.Visible = false;

        }

        public override void Destroy()
        {
            //nothing to detach from here
        }

        public override void Update(TSO.Common.rendering.framework.model.UpdateState state)
        {
            base.Update(state);

            

        }

        public override void Draw(UISpriteBatch batch)
        {
            base.Draw(batch);
            
        }


    }
}
