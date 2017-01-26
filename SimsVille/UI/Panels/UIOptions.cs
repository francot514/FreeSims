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
using SimsHomeMaker;

namespace TSOVille.Code.UI.Panels
{

    /// <summary>
    /// Options Panel
    /// </summary>
    public class UIOptions : UIDestroyablePanel
    {
        public UIImage Background;
        public UIImage Divider;

        public UIButton ExitButton { get; set; }
        public UIButton GraphicsButton { get; set; }
        public UIButton ProfanityButton { get; set; }
        public UIButton SelectSimButton { get; set; }
        public UIButton SoundButton { get; set; }

        public Texture2D BackgroundGameImage { get; set; }
        public Texture2D DividerImage { get; set; }

        private UIContainer Panel;
        private int CurrentPanel;

        public UIOptions()
        {
            var script = this.RenderScript("optionspanel.uis");

            /*var bgimage = new TSOVille.Code.UI.Framework.Parser.UINode();
            var imageAtts = new Dictionary<string,string>();
            imageAtts.Add("assetID", (GlobalSettings.GraphicsWidth < 1024)?"0x000000D800000002":"0x0000018300000002");
            bgimage.ID = "BackgroundGameImage";

            bgimage.AddAtts(imageAtts);
            script.DefineImage(bgimage);*/

            //we really need to figure out how graphics reset works to see what and how we need to reload things

            Background = new UIImage(GetTexture((GlobalSettings.GraphicsWidth < 1024) ? (ulong)0x000000D800000002 : (ulong)0x0000018300000002));
            this.AddAt(0, Background);
            Background.BlockInput();

            Divider = new UIImage(DividerImage);
            Divider.X = 227;
            Divider.Y = 17;
            this.Add(Divider);

            ExitButton.OnButtonClick += new ButtonClickDelegate(ExitButton_OnButtonClick);
            SelectSimButton.OnButtonClick += new ButtonClickDelegate(SelectSimButton_OnButtonClick);

            GraphicsButton.OnButtonClick += new ButtonClickDelegate(GraphicsButton_OnButtonClick);
            ProfanityButton.OnButtonClick += new ButtonClickDelegate(ProfanityButton_OnButtonClick);
            SoundButton.OnButtonClick += new ButtonClickDelegate(SoundButton_OnButtonClick);

            CurrentPanel = -1;
        }

        public override void Destroy()
        {
            //nothing to detach from here
        }

        public void SetPanel(int newPanel)
        {
            GraphicsButton.Selected = false;
            ProfanityButton.Selected = false;
            SoundButton.Selected = false;

            if (CurrentPanel != -1) this.Remove(Panel);
            if (newPanel != CurrentPanel)
            {
                switch (newPanel)
                {
                    case 0:
                        Panel = new UIGraphicOptions();
                        GraphicsButton.Selected = true;
                        break;
                    case 1:
                        Panel = new UIProfanityOptions();
                        ProfanityButton.Selected = true;
                        break;
                    case 2:
                        Panel = new UISoundOptions();
                        SoundButton.Selected = true;
                        break;
                    default:
                        break;
                }
                Panel.X = 240;
                Panel.Y = 0;
                this.Add(Panel);
                CurrentPanel = newPanel;
            }
            else
            {
                CurrentPanel = -1;
            }

        }

        private void ExitButton_OnButtonClick(UIElement button)
        {
            GameFacade.Kill();
            //UIScreen.ShowDialog(new UIExitDialog(), true);
        }

        private void GraphicsButton_OnButtonClick(UIElement button)
        {
            SetPanel(0);
        }

        private void ProfanityButton_OnButtonClick(UIElement button)
        {
            SetPanel(1);
        }

        private void SoundButton_OnButtonClick(UIElement button)
        {
            SetPanel(2);
        }

        private void SelectSimButton_OnButtonClick(UIElement button)
        {
            var alert = UIScreen.ShowAlert(new UIAlertOptions { Title = "Not Implemented", Message = "This feature is not implemented yet!" }, true);
        }
    }

    public class UIProfanityOptions : UIContainer
    {
        public UIProfanityOptions()
        {
            var alert = UIScreen.ShowAlert(new UIAlertOptions { Title = "Not Implemented", Message = "This feature is not implemented yet!" }, true);
            //this.RenderScript("profanitypanel.uis");
            //don't draw, this currently breaks the uis parser
        }
    }

    public class UISoundOptions : UIContainer
    {
        public UISlider FXSlider { get; set; }
        public UISlider MusicSlider { get; set; }
        public UISlider VoxSlider { get; set; }
        public UISlider AmbienceSlider { get; set; }

        public UISoundOptions()
        {
            this.RenderScript("soundpanel.uis");

            FXSlider.OnChange += new ChangeDelegate(ChangeVolume);
            MusicSlider.OnChange += new ChangeDelegate(ChangeVolume);
            VoxSlider.OnChange += new ChangeDelegate(ChangeVolume);
            AmbienceSlider.OnChange += new ChangeDelegate(ChangeVolume);
        }

        void ChangeVolume(UIElement slider)
        {
            UISlider elm = (UISlider)slider;

            //if (elm == FXSlider) GlobalSettings.FXVolume = (byte)elm.Value;
            //else if (elm == MusicSlider) GlobalSettings.MusicVolume = (byte)elm.Value;
            //else if (elm == VoxSlider) GlobalSettings.VoxVolume = (byte)elm.Value;
            //else if (elm == AmbienceSlider) GlobalSettings.AmbienceVolume = (byte)elm.Value;

            //GlobalSettings.Save();
            //GameFacade.SoundManager.UpdateMusicVolume();
        }
    }

    public class UIGraphicOptions : UIContainer
    {

        public UIButton AntiAliasCheckButton { get; set; }
        public UIButton ShadowsCheckButton { get; set; }
        public UIButton LightingCheckButton { get; set; }
        public UIButton UIEffectsCheckButton { get; set; }
        public UIButton EdgeScrollingCheckButton { get; set; }
        public UIButton FullScreenCheckButton;

        // High-Medium-Low detail buttons:

        public UIButton TerrainDetailLowButton { get; set; }
        public UIButton TerrainDetailMedButton { get; set; }
        public UIButton TerrainDetailHighButton { get; set; }

        public UIButton CharacterDetailLowButton { get; set; }
        public UIButton CharacterDetailMedButton { get; set; }
        public UIButton CharacterDetailHighButton { get; set; }

        public UILabel UIEffectsLabel { get; set; }
        public UILabel CharacterDetailLabel { get; set; }

        public UIGraphicOptions()
        {
            var script = this.RenderScript("graphicspanel.uis");
            UIEffectsLabel.Caption = "City Shadows";
            UIEffectsLabel.Alignment = TextAlignment.Middle;
            CharacterDetailLabel.Caption = "Shadow Detail";

            FullScreenCheckButton = new UIButton();
            FullScreenCheckButton.Caption = "Full Screen";
            FullScreenCheckButton.Position = new Vector2(245, 281);

            AntiAliasCheckButton.OnButtonClick += new ButtonClickDelegate(FlipSetting);
            ShadowsCheckButton.OnButtonClick += new ButtonClickDelegate(FlipSetting);
            LightingCheckButton.OnButtonClick += new ButtonClickDelegate(FlipSetting);
            UIEffectsCheckButton.OnButtonClick += new ButtonClickDelegate(FlipSetting);
            EdgeScrollingCheckButton.OnButtonClick += new ButtonClickDelegate(FlipSetting);
            FullScreenCheckButton.OnButtonClick += new ButtonClickDelegate(FlipSetting);

            CharacterDetailLowButton.OnButtonClick += new ButtonClickDelegate(ChangeShadowDetail);
            CharacterDetailMedButton.OnButtonClick += new ButtonClickDelegate(ChangeShadowDetail);
            CharacterDetailHighButton.OnButtonClick += new ButtonClickDelegate(ChangeShadowDetail);

            SettingsChanged();
        }

        private void FlipSetting(UIElement button)
        {
            
            SettingsChanged();
        }

        private void ChangeShadowDetail(UIElement button)
        {
           
            //GlobalSettings.Save();
            SettingsChanged();
        }

        private void SettingsChanged()
        {
            

            //not used right now! We need to determine if this should be ingame or not... It affects the density of grass blades on the simulation terrain.
            TerrainDetailLowButton.Disabled = true;
            TerrainDetailMedButton.Disabled = true;
            TerrainDetailHighButton.Disabled = true;
        }
    }
}
