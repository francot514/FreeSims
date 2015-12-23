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
using TSOVille.Code.UI.Controls;
using TSOVille.LUI;
using TSOVille.Code.UI.Framework;
using TSOVille.Code.UI.Screens;

namespace TSOVille.Code.UI.Panels
{
    public class UILoginDialog : UIDialog
    {
        private UITextEdit m_TxtAccName, m_TxtPass;
        private LoginScreen m_LoginScreen;

        public UILoginDialog(LoginScreen loginScreen)
            : base(UIDialogStyle.Standard, true)
        {
            this.m_LoginScreen = loginScreen;
            this.Caption = GameFacade.Strings.GetString("UIText", "209", "1");

            SetSize(350, 225);

            m_TxtAccName = UITextEdit.CreateTextBox();
            m_TxtAccName.X = 20;
            m_TxtAccName.Y = 72;
            m_TxtAccName.MaxChars = 16;
            m_TxtAccName.SetSize(310, 27);
            m_TxtAccName.CurrentText = GlobalSettings.Default.LastUser;
            m_TxtAccName.OnTabPress += new KeyPressDelegate(m_TxtAccName_OnTabPress);
            this.Add(m_TxtAccName);

            m_TxtPass = UITextEdit.CreateTextBox();
            m_TxtPass.X = 20;
            m_TxtPass.Y = 128;
            m_TxtPass.MaxChars = 16;
            m_TxtPass.CurrentText = "password";
            m_TxtPass.SetSize(310, 27);
            //m_TxtPass.OnTabPress += new KeyPressDelegate(m_TxtPass_OnTabPress);
            m_TxtPass.OnEnterPress += new KeyPressDelegate(loginBtn_OnButtonClick);
            this.Add(m_TxtPass);

            /** Login button **/
            var loginBtn = new UIButton
            {
                X = 126,
                Y = 170,
                Width = 100,
                ID = "LoginButton",
                Caption = GameFacade.Strings.GetString("UIText", "209", "2")
            };
            this.Add(loginBtn);
            loginBtn.OnButtonClick += new ButtonClickDelegate(loginBtn_OnButtonClick);

            var exitBtn = new UIButton
            {
                X = 226,
                Y = 170,
                Width = 100,
                ID = "ExitButton",
                Caption = GameFacade.Strings.GetString("UIText", "209", "3")
            };
            this.Add(exitBtn);
            exitBtn.OnButtonClick += new ButtonClickDelegate(exitBtn_OnButtonClick);

            var RegisterBtn = new UIButton
            {
                X = 26,
                Y = 170,
                Width = 100,
                ID = "RegisterButton",
                Caption = "Register"
            };
            this.Add(RegisterBtn);
            RegisterBtn.OnButtonClick += new ButtonClickDelegate(RegisterBtn_OnButtonClick);

            
            this.Add(new UILabel
            {
                Caption = GameFacade.Strings.GetString("UIText", "209", "4"),
                X = 24,
                Y = 50
            });

            this.Add(new UILabel
            {
                Caption = GameFacade.Strings.GetString("UIText", "209", "5"),
                X = 24,
                Y = 106
            });

            GameFacade.Screens.inputManager.SetFocus(m_TxtAccName);
        }

        /*void m_TxtPass_OnTabPress(UIElement element)
        {
            GameFacade.Screens.inputManager.SetFocus(m_TxtAccName);
        }*/

        void m_TxtAccName_OnTabPress(UIElement element)
        {
            GameFacade.Screens.inputManager.SetFocus(m_TxtPass);
        }

        public string Username
        {
            get
            {
                return m_TxtAccName.CurrentText;
            }

            set
            {
                m_TxtAccName.CurrentText = value;
            }
        }

        public string Password
        {
            get
            {
                return m_TxtPass.CurrentText;
            }

            set
            {
                m_TxtPass.CurrentText = value;
            }
        }

        void loginBtn_OnButtonClick(UIElement button)
        {
            m_LoginScreen.Login();
            //GameFacade.Controller.ShowPersonSelection();
        }

        void RegisterBtn_OnButtonClick(UIElement button)
        {
            m_LoginScreen.Register();
            //GameFacade.Controller.ShowPersonSelection();
        }

        void exitBtn_OnButtonClick(UIElement button)
        {
            GameFacade.Kill();
            /*var exitDialog = new UIExitDialog();
            Parent.Add(exitDialog);*/
        }
    }
}
