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

namespace TSOVille.Code.UI.Panels
{
    /// <summary>
    /// Live Mode Panel
    /// </summary>
    public class UILiveMode : UIDestroyablePanel
    {
        public UIImage Thumbnail;
        public UIImage[] Thumbnails = new UIImage[5];
        public UIImage Background;
        public UIImage Divider;
        public UIImage Icon;
        public UIMotiveDisplay MotiveDisplay;
        public UISkillDisplay SkillDisplay;
        public Texture2D DividerImg { get; set; }
        public Texture2D PeopleListBackgroundImg { get; set; }
        public Texture2D EODButtonLayoutNoneImg { get; set; }

        //EOD buttons
        public UIButton EODHelpButton { get; set; }
        public UIButton EODCloseButton { get; set; }
        public UIButton EODExpandButton { get; set; }
        public UIButton EODContractButton { get; set; }

        public UIButton MoodPanelButton;
 
        public VMAvatar SelectedAvatar;
        public List<VMAvatar> Avatars = new List<VMAvatar>();


        public UILiveMode () {
            var script = this.RenderScript("livepanel"+((GlobalSettings.Default.GraphicsWidth < 1024)?"":"1024")+".uis");

            Background = new UIImage(GetTexture((GlobalSettings.Default.GraphicsWidth < 1024) ? (ulong)0x000000D800000002 : (ulong)0x0000018300000002));
            Background.Y = 33;
            this.AddAt(0, Background);

            MoodPanelButton = new UIButton();
            MoodPanelButton.Texture = GetTexture((ulong)FileIDs.UIFileIDs.lpanel_moodpanelbtn);
            MoodPanelButton.ImageStates = 4;
            MoodPanelButton.Position = new Vector2(31, 63);
            this.Add(MoodPanelButton);

            
            Thumbnail = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.thumbtemplate1frame));
            Thumbnail.Size = new Point(33, 33);
            Thumbnail.Position = new Vector2(63, 73);
            this.Add(Thumbnail);


            Thumbnails[0] = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.thumbtemplate1frame));
            Thumbnails[0].Size = new Point(33, 33);
            Thumbnails[0].Position = new Vector2(600, 59);
            this.Add(Thumbnails[0]);

            Thumbnails[1] = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.thumbtemplate1frame));
            Thumbnails[1].Size = new Point(33, 33);
            Thumbnails[1].Position = new Vector2(640, 59);
            this.Add(Thumbnails[1]);

            Thumbnails[2] = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.thumbtemplate1frame));
            Thumbnails[2].Size = new Point(33, 33);
            Thumbnails[2].Position = new Vector2(600, 99);
            this.Add(Thumbnails[2]);

            Thumbnails[3] = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.thumbtemplate1frame));
            Thumbnails[3].Size = new Point(33, 33);
            Thumbnails[3].Position = new Vector2(640, 99);
            this.Add(Thumbnails[3]);

            var PeopleListBg = new UIImage(PeopleListBackgroundImg);
            PeopleListBg.Position = new Microsoft.Xna.Framework.Vector2(375, 38);
            this.AddAt(1, PeopleListBg);

            Divider = new UIImage(DividerImg);
            Divider.Position = new Microsoft.Xna.Framework.Vector2(140, 49);
            this.AddAt(1, Divider);

            MotiveDisplay = new UIMotiveDisplay();
            MotiveDisplay.Position = new Vector2(165, 59);
            this.Add(MotiveDisplay);

            SkillDisplay = new UISkillDisplay();
            SkillDisplay.Position = new Vector2(365, 59);
            this.Add(SkillDisplay);


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

            if (SelectedAvatar != null)
            {

                Thumbnail.Texture = SelectedAvatar.GetIcon(GameFacade.GraphicsDevice, 0);
                Thumbnail.Tooltip = SelectedAvatar.Name;

                
                if (Avatars.Count > 0 )
                    {

                Thumbnails[0].Texture = Avatars[0].GetIcon(GameFacade.GraphicsDevice, 0);
                Thumbnails[0].Tooltip = Avatars[0].Name;

                Thumbnails[1].Texture = Avatars[1].GetIcon(GameFacade.GraphicsDevice, 0);
                Thumbnails[1].Tooltip = Avatars[1].Name;


                Thumbnails[2].Texture = Avatars[2].GetIcon(GameFacade.GraphicsDevice, 0);
                Thumbnails[2].Tooltip = Avatars[2].Name;


                Thumbnails[3].Texture = Avatars[3].GetIcon(GameFacade.GraphicsDevice, 0);
                Thumbnails[3].Tooltip = Avatars[3].Name;
                    }

                UpdateMotives();
                UpdateSkills();

            }


        }

        private void UpdateMotives()
        {
            MotiveDisplay.MotiveValues[0] = SelectedAvatar.GetMotiveData(VMMotive.Hunger);
            MotiveDisplay.MotiveValues[1] = SelectedAvatar.GetMotiveData(VMMotive.Comfort);
            MotiveDisplay.MotiveValues[2] = SelectedAvatar.GetMotiveData(VMMotive.Hygiene);
            MotiveDisplay.MotiveValues[3] = SelectedAvatar.GetMotiveData(VMMotive.Bladder);
            MotiveDisplay.MotiveValues[4] = SelectedAvatar.GetMotiveData(VMMotive.Energy);
            MotiveDisplay.MotiveValues[5] = SelectedAvatar.GetMotiveData(VMMotive.Fun);
            MotiveDisplay.MotiveValues[6] = SelectedAvatar.GetMotiveData(VMMotive.Social);
            MotiveDisplay.MotiveValues[7] = SelectedAvatar.GetMotiveData(VMMotive.Room);
        }

        private void UpdateSkills()
        {

            SkillDisplay.SkillValues[0] = SelectedAvatar.GetPersonData(VMPersonDataVariable.CookingSkill);
            SkillDisplay.SkillValues[1] = SelectedAvatar.GetPersonData(VMPersonDataVariable.CharismaSkill);
            SkillDisplay.SkillValues[2] = SelectedAvatar.GetPersonData(VMPersonDataVariable.MechanicalSkill);
            SkillDisplay.SkillValues[3] = SelectedAvatar.GetPersonData(VMPersonDataVariable.CreativitySkill);
            SkillDisplay.SkillValues[4] = SelectedAvatar.GetPersonData(VMPersonDataVariable.BodySkill);
            SkillDisplay.SkillValues[5] = SelectedAvatar.GetPersonData(VMPersonDataVariable.LogicSkill);

        }
    }
}
