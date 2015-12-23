using System;
using System.Collections.Generic;
using System.Linq;
using TSOVille.Code.UI.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSOVille.LUI;
using TSOVille.Code.UI.Controls;
using TSOVille.Code.UI.Framework.Parser;
using tso.world.model;

namespace TSOVille.Code.UI.Screens
{
   public class HouseCreator: GameScreen
    {

        private UIContainer BackgroundCtnr;
        private UIImage Background, TextEditBg, DescBg;
        public UIButton CancelButton;
        public UIButton AcceptButton;
        public UIListBox SelectionBox;
        public UITextEdit NameTextEdit, SizeTextEdit;
        public UITextEdit DescriptionTextEdit;
        public CoreGameScreen MainScreen;
        public XmlHouseData HouseData;

        private UIButton m_ExitButton;

        public HouseCreator()
        {
           

            BackgroundCtnr = new UIContainer();
            BackgroundCtnr.ScaleX = BackgroundCtnr.ScaleY = ScreenWidth / 800.0f;

            /** Background image **/
            Background = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.creditscreen_background));
            Background.ID = "Background";
            BackgroundCtnr.Add(Background);

            TextEditBg = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.dialog_textboxbackground));
            TextEditBg.Size = new Point(130, 20);
            TextEditBg.Position = new Vector2(80, 100);
            BackgroundCtnr.Add(TextEditBg);

            DescBg = new UIImage(GetTexture((ulong)FileIDs.UIFileIDs.dialog_textboxbackground));
            DescBg.Size = new Point(160, 340);
            DescBg.Position = new Vector2(80, 200);
            BackgroundCtnr.Add(DescBg);



            var lbl = new UILabel();
            lbl.Caption = "House lot creator - Customize the house lot to be created";
            lbl.X = 120;
            lbl.Y = 20;
            BackgroundCtnr.Add(lbl);

            string[] columns = new string[10];
            columns[0] = "Type";

            SelectionBox = new UIListBox();
            SelectionBox.Items.Add(new UIListBoxItem("House", columns));
            SelectionBox.Position = new Vector2(400, 200);
            SelectionBox.Size = new Vector2(200, 200);
            SelectionBox.Tooltip = "House category";
            BackgroundCtnr.Add(SelectionBox);

            CancelButton = new UIButton();
            CancelButton.X = 680;
            CancelButton.Y = 560;
            CancelButton.Texture = GetTexture((ulong)FileIDs.UIFileIDs.person_edit_cancelbtn);
            BackgroundCtnr.Add(CancelButton);
            CancelButton.Visible = true;
            CancelButton.OnButtonClick += new ButtonClickDelegate( CancelButton_OnButtonClick);

            
            AcceptButton = new UIButton();
            AcceptButton.X = 740;
            AcceptButton.Y = 560;
            AcceptButton.Texture = GetTexture((ulong)FileIDs.UIFileIDs.person_edit_acceptbtn);
            BackgroundCtnr.Add(AcceptButton);
            AcceptButton.Visible = true;
            AcceptButton.OnButtonClick += new ButtonClickDelegate(AcceptButton_OnButtonClick);

            m_ExitButton = new UIButton();
            m_ExitButton.X = 300;
            m_ExitButton.Y = 560;
            m_ExitButton.Texture = GetTexture((ulong)FileIDs.UIFileIDs.person_edit_closebtn);
            BackgroundCtnr.Add(m_ExitButton);
            m_ExitButton.Visible = true;
            m_ExitButton.OnButtonClick += new ButtonClickDelegate(m_ExitButton_OnButtonClick);

            var namelabel = new UILabel();
            namelabel.Caption = "House name:";
            namelabel.X = 80;
            namelabel.Y = 80;
            BackgroundCtnr.Add(namelabel);

            NameTextEdit = new UITextEdit();
            NameTextEdit.Position = new Vector2(80, 100);
            NameTextEdit.Tooltip = "House name";
            NameTextEdit.CurrentText = "My House";
            NameTextEdit.MaxLines = 1;
            NameTextEdit.Size = new Vector2(130, 180);
            BackgroundCtnr.Add(NameTextEdit);
            NameTextEdit.Visible = true;
            NameTextEdit.OnChange += new ChangeDelegate(NameTextEdit_OnChange);

            var sizelabel = new UILabel();
            sizelabel.Caption = "Lot size:";
            sizelabel.X = 80;
            sizelabel.Y = 120;
            BackgroundCtnr.Add(sizelabel);

            SizeTextEdit = new UITextEdit();
            SizeTextEdit.Position = new Vector2(80, 140);
            SizeTextEdit.Tooltip = "Lot size";
            SizeTextEdit.CurrentText = "64";
            SizeTextEdit.MaxLines = 1;
            SizeTextEdit.Size = new Vector2(130, 180);
            BackgroundCtnr.Add(SizeTextEdit);
            SizeTextEdit.Visible = true;

            var desclabel = new UILabel();
            desclabel.Caption = "Basic description:";
            desclabel.X = 80;
            desclabel.Y = 200;
            BackgroundCtnr.Add(desclabel);

            DescriptionTextEdit = new UITextEdit();
            DescriptionTextEdit.Position = new Vector2(80, 220);
            DescriptionTextEdit.Tooltip = "House description";
            DescriptionTextEdit.MaxLines = 10;
            DescriptionTextEdit.FrameColor = Color.BlueViolet;
            DescriptionTextEdit.Size = new Microsoft.Xna.Framework.Vector2(130, 290);
            BackgroundCtnr.Add(DescriptionTextEdit);
            DescriptionTextEdit.Visible = true;

            PlayBackgroundMusic(
                new string[] { GlobalSettings.Default.StartupPath + "\\music\\modes\\create\\tsosas2_v2.mp3" }
            );

            this.Add(BackgroundCtnr);

        }

        private void m_ExitButton_OnButtonClick(UIElement button)
        {
            GameFacade.Kill();
        }

        private void CancelButton_OnButtonClick(UIElement button)
        {

            Visible = false;
            GameFacade.Screens.CurrentUIScreen = new CoreGameScreen();
            GameFacade.Screens.RemoveCurrent();
            GameFacade.Screens.AddScreen(GameFacade.Screens.CurrentUIScreen);


        }


        private void NameTextEdit_OnChange(UIElement element)
        {
            AcceptButton.Disabled = NameTextEdit.CurrentText.Length == 0;
        }


       private void AcceptButton_OnButtonClick(UIElement button)
        {

            if (NameTextEdit.CurrentText.Length != 0)
                CreateHouse(NameTextEdit.CurrentText, Convert.ToInt32(SizeTextEdit.CurrentText));

        }

       private void CreateHouse(string name, int size)
       {

           HouseData = new XmlHouseData();
           HouseData.Name = name;

           HouseData.World = new XmlHouseDataWorld();
           HouseData.World.Floors = new List<XmlHouseDataFloor>();
           HouseData.World.Walls = new List<XmlHouseDataWall>();
           HouseData.Objects = new List<XmlHouseDataObject>();

           HouseData.Category = 0;
           HouseData.Size = size;

           for (short x = 0; x < size; x++)

               for (short y = 0; y < size; y++)
                   {
                  var floor = new XmlHouseDataFloor()
                    {

                        
                        Value = 0,
                        X = x,
                        Y = y,
                        Level = 0,

                    };

                     HouseData.World.Floors.Add(floor);

                    }

           XmlHouseDataObject mailbox = new XmlHouseDataObject()
                {
                    GUID = "0x39CCF441",
                    X= 32,
                    Y= 56, 
                    Dir= 4,
                    Level= 1

                };

           HouseData.Objects.Add(mailbox);


           XmlHouseData.Save(GlobalSettings.Default.DocumentsPath + @"Houses\" + name + ".xml", HouseData);

       }

    }
}
