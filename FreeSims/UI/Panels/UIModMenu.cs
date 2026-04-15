using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common;


namespace FSO.Client.UI.Panels
{
    public class UIModMenu : UIDialog
    {
        private UIImage Background;
        private UIButton IPBanButton;

        public uint AvatarID;

        public UIModMenu() : base(UIDialogStyle.Tall | UIDialogStyle.Close, true)
        {
            SetSize(380, 300);
            Caption = "Do what to this user?";

            Position = new Microsoft.Xna.Framework.Vector2(
                (GlobalSettings.Default.GraphicsWidth / 2.0f) - (480/2),
                (GlobalSettings.Default.GraphicsHeight / 2.0f) - 150
            );

            IPBanButton = new UIButton();
            IPBanButton.Caption = "IP Ban";
            IPBanButton.Position = new Microsoft.Xna.Framework.Vector2(40, 50);
            IPBanButton.Width = 300;
            IPBanButton.OnButtonClick += x =>
            {
             
                UIScreen.RemoveDialog(this);
            };
            Add(IPBanButton);

            var BanButton = new UIButton();
            BanButton.Caption = "Ban User";
            BanButton.Position = new Microsoft.Xna.Framework.Vector2(40, 90);
            BanButton.Width = 300;
            BanButton.OnButtonClick += x =>
            {
            
            };
            Add(BanButton);

            var kickButton = new UIButton();
            kickButton.Caption = "Kick Avatar";
            kickButton.Position = new Microsoft.Xna.Framework.Vector2(40, 130);
            kickButton.Width = 300;
            kickButton.OnButtonClick += x =>
            {
               
            };
            Add(kickButton);

            var nhoodBanButton = new UIButton();
            nhoodBanButton.Caption = "Nhood Ban";
            nhoodBanButton.Position = new Microsoft.Xna.Framework.Vector2(40, 170);
            nhoodBanButton.Width = 300;
            nhoodBanButton.OnButtonClick += x =>
            {
                
            };
            Add(nhoodBanButton);

            CloseButton.OnButtonClick += CloseButton_OnButtonClick;
        }

        private void CloseButton_OnButtonClick(UIElement button)
        {
            UIScreen.RemoveDialog(this);
        }
    }
}
