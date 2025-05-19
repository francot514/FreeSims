using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using FSO.Files.FAR1;


namespace sims.debug
{
    public partial class Form1 : Form
    {

        private string Userpath, Famdir, Housedir;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            DirectoryInfo dirinfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            foreach (DirectoryInfo dir in dirinfo.GetDirectories())
                listBox1.Items.Add(dir.FullName);

            FileInfo file = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "GameData/Objects/Objects.far");
            if (File.Exists(file.FullName))
                listBox2.Items.Add(file.FullName);
            
        }

        private void LoadHouse(string path)
        {
           

            

            FileInfo housefile = new FileInfo(path);
            label5.Text = housefile.Name;

            IffFile iff = new IffFile(path);

            foreach (BMP bmp in iff.List<BMP>())
                if (bmp.ChunkID == 512)
                    pictureBox1.BackgroundImage = bmp.GetBitmap();

            foreach (OBJT obj in iff.List<OBJT>())
            {
                var entries = obj.Entries;

                foreach (OBJTEntry entry in entries)
                    if (entry.Name != null)
                        listBox4.Items.Add(entry.Name);
            }

        }

        private void LoadFamilies(string path)
        {
            Famdir = path + "/Export";

            DirectoryInfo dir = new DirectoryInfo(path + "/Export");

            foreach (FileInfo file in dir.GetFiles())
            {

                if (file.Extension == ".FAM")
                {
                    string famid = file.Name.Split('_')[1].Split('.')[0];

                    listBox3.Items.Add(file.Name);


                }
            }
        }

        private sbyte GetCategoryName(string name)
        {
            sbyte category = -1;

            if (name.Contains("Door") || name.Contains("door"))
                category = 0;
            if (name.Contains("Window") || name.Contains("window"))
                category = 1;
            else if (name.Contains("Stair"))
                category = 2;
            else if (name.Contains("Shrub") || name.Contains("Tree") || name.Contains("Flower"))
                category = 3;
            else if (name.Contains("Fireplace"))
                category = 4;
            else if (name.Contains("Pool"))
                category = 5;
            else if (name.Contains("Column"))
                category = 7;
            else if (name.Contains("Chair") || name.Contains("Sofa") || name.Contains("Bed") || name.Contains("bed") || name.Contains("bench"))
                category = 12;
            else if (name.Contains("Table") || name.Contains("Counter") || name.Contains("counter") || name.Contains("Saloon")
                || name.Contains("Horn") || name.Contains("Desk") || name.Contains("desk") || name.Contains("table") || name.Contains("Dining"))
                category = 13;
            else if (name.Contains("Dishwasher") || name.Contains("Machine") || name.Contains("Stove") || name.Contains("stove")
                || name.Contains("Fridge") || name.Contains("Shower") || name.Contains("tub") || name.Contains("Toilet") || name.Contains("shower") || name.Contains("sink"))
                category = 14;
            else if (name.Contains("tv") || name.Contains("Stereo") || name.Contains("Computer") || name.Contains("Dance") || name.Contains("Mechanical")
                || name.Contains("Teleport") || name.Contains("phone") ||  name.Contains("Cash"))
                category = 15;
            else if (name.Contains("Piano") || name.Contains("Book") || name.Contains("Mirror") || name.Contains("Telescope") || name.Contains("mirror") || name.Contains("book")
                || name.Contains("Chemistry") || name.Contains("Guitar") || name.Contains("mirror") || name.Contains("chess"))
                category = 16;
            else if (name.Contains("Sculpture") || name.Contains("Plant") || name.Contains("Painting") || name.Contains("Clock")
                || name.Contains("Rug") || name.Contains("Ball") || name.Contains("Christmas") || name.Contains("awning") || name.Contains("Sign"))
                category = 17;
            else if (name.Contains("Cloth") || name.Contains("Dress") || name.Contains("Can") || name.Contains("Bar")
                || name.Contains("Trash") || name.Contains("Alarm") || name.Contains("Trunk") || name.Contains("DisplayCase"))
                category = 18;
            else if (name.Contains("Lamp") || name.Contains("Light") || name.Contains("lamp") || name.Contains("candle") || name.Contains("Menorah"))
                category = 19;
            else if (name.Contains("Pet") || name.Contains("Dog") || name.Contains("Cat"))
                category = 20;
            else if (name.Contains("Job"))
                category = 21;
            else if (name.Contains("Restaurant"))
                category = 22;

            return category;

        }


        private void CreateTableFile(string path)
        {

            InfoTable infotable = new InfoTable();
            infotable.Items = new List<TableItem>();

            //Far = new FAR1Archive(path, 0);
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach(FileInfo file in dir.GetFiles())
                if (file.Extension == ".iff")
               {
               var iff = new IffFile(path + "/" + file.Name);
               ulong FileID = 0;


               foreach (OBJD obj in iff.List<OBJD>())
               if (obj.IsMultiTile)
                   {

                   FileID = obj.GUID;
                   string name = Path.GetFileNameWithoutExtension(file.Name);

                    infotable.Items.Add(new TableItem()
                            {
                                GUID = FileID.ToString("X"),
                                FileName = name,
                                Name = iff.Filename.Substring(0,iff.Filename.Length - 4),
                                Group = obj.MasterID.ToString(),
                                SubIndex = obj.SubIndex.ToString()

                            });

                    listBox2.Items.Add(file.Name + " " + FileID);
                    InfoTable.Save("table.xml", infotable);

                   }
               else if (!obj.IsMultiTile)
               {

                   FileID = obj.GUID;
                   string name = Path.GetFileNameWithoutExtension(file.Name);

                   infotable.Items.Add(new TableItem()
                   {
                       GUID = FileID.ToString("X"),
                       FileName = name,
                       Name = iff.Filename.Substring(0, iff.Filename.Length - 4),
                       Group = obj.MasterID.ToString(),
                       SubIndex = obj.SubIndex.ToString()

                   });

                    listBox2.Items.Add(file.Name + " " + FileID);
                    InfoTable.Save("table.xml", infotable);
                        }
          
                }
        }

        private void CreateCatalogFile(string path)
        {

            CatalogTable catalog = new CatalogTable();
            catalog.Items = new List<CatalogItem>();

            //Far = new FAR1Archive(path, 0);
            DirectoryInfo dir = new DirectoryInfo(path);


           foreach(FileInfo file in dir.GetFiles())
               if (file.Extension == ".iff")
               {
                   var iff = new IffFile(path + "/" + file.Name);
                    ulong FileID = 0;


               foreach (OBJD obj in iff.List<OBJD>())
               if (obj.IsMultiTile && obj.SubIndex == -1)
                   {

                   FileID = obj.GUID;
                   string name = Path.GetFileNameWithoutExtension(file.Name);

                    catalog.Items.Add(new CatalogItem()
                            {
                                GUID = FileID.ToString("X"),
                                Name = name,
                                Category = GetCategoryName(name),
                                Price = obj.Price

                            });

                    listBox2.Items.Add(file.Name + " " + FileID);
                    CatalogTable.Save("catalog.xml", catalog);

                   }
               else if (!obj.IsMultiTile)
               {

                   FileID = obj.GUID;
                   string name = Path.GetFileNameWithoutExtension(file.Name);

                   catalog.Items.Add(new CatalogItem()
                   {
                       GUID = FileID.ToString("X"),
                       Name = name,
                       Category = GetCategoryName(name),
                       Price = obj.Price

                   });

                   listBox2.Items.Add(file.Name + " " + FileID);
                   CatalogTable.Save("catalog.xml", catalog);

               }

               }

        }


        private void button1_Click(object sender, EventArgs e)
        {

            listBox2.Items.Clear();

            string file = "";

            if (listBox1.SelectedItem != null)

                file = listBox1.SelectedItem.ToString();

            if (file != String.Empty)
                CreateTableFile(file);

            
                

        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();

            string file = "";

            if (listBox1.SelectedItem != null)

                file = listBox1.SelectedItem.ToString();

            if (file != String.Empty)
                CreateCatalogFile(file);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            
            List<FarEntry> Resource = new List<FarEntry>();

            if (File.Exists(("UIGraphics/UIGraphics.far")))
            {
                    FAR1Archive Archive = new FAR1Archive("UIGraphics/UIGraphics.far", false);
                    //m_Archives.Add(path, Archive);
                    Resource = Archive.GetAllFarEntries();

                foreach (var entry in Resource)
                 {

                     if (entry.Filename.Contains(".bmp"))
                     listBox1.Items.Add(entry.Filename + "" + entry.DataLength);



                }

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            uint id = 0;
            string name = "";

            RoofTable roofs = new RoofTable();
            roofs.Items = new List<RoofItem>();

            if (Directory.Exists("Roofs"))
            {
                DirectoryInfo dir = new DirectoryInfo("Roofs");

                foreach (FileInfo file in dir.GetFiles())
                if (file.Extension == ".bmp")
                {
                    name = Path.GetFileNameWithoutExtension(file.Name);

                    roofs.Items.Add(new RoofItem()
                    {
                        ID = id,
                        Name = name,
                        Category = 6,
                        Price = 0

                    });


                    id++;

                }

            }

            RoofTable.Save("roofs.xml", roofs);

        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int current = comboBox1.SelectedIndex;

            
            Userpath = "UserData";

        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox3.Items.Clear();


            int current = comboBox1.SelectedIndex;
            DirectoryInfo dir;

            if (Userpath != "")
            {
                if (current == 0)
                {
                    dir = new DirectoryInfo(Userpath + "/Houses");

                    if (dir.Exists)
                        foreach (FileInfo file in dir.GetFiles())
                        if (file.Extension == ".iff")
                            listBox3.Items.Add(file.Name);

                }

                else if (current == 1)
                {
                    dir = new DirectoryInfo(Userpath + "/Families");

                    if (dir.Exists)
                        foreach (FileInfo file in dir.GetFiles())
                        if (file.Extension == ".iff")
                            listBox3.Items.Add(file.Name);

                }

            }



        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

            string current = "";
            if (listBox3.SelectedItem != null)
                current = listBox3.SelectedItem.ToString();
            string userdata = "";
            if (listBox1.SelectedItem != null)
             userdata = comboBox1.SelectedItem.ToString();


            var iff = new IffFile(Userpath + "/Houses/" + current);

            var simi = iff.Get<SIMI>(326);

            // int value = simi.ArchitectureValue();
            var obj = iff.Get<OBJT>(0);

            var bmp = iff.Get<BMP>(512);


            //int rooms = simi.Rooms();

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            string current = listBox2.SelectedItem.ToString();

            if (!Directory.Exists("Export"))
                Directory.CreateDirectory("Export");

            if (current != null)
            {
                FAR1Archive far = new FAR1Archive(current, false);

                foreach ( KeyValuePair<string, byte[]> entry in far.GetAllEntries())
                    if (entry.Key.Contains(".iff"))
                    {
                        File.WriteAllBytes("Export/" + entry.Key, entry.Value);

                    }
            }
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string current = listBox4.SelectedItem.ToString();


            IffFile file = new IffFile(AppDomain.CurrentDomain.BaseDirectory + current + ".iff");


        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {

            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            openFileDialog1.Filter = "Sims file (*.iff)|*.iff|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName.Contains("House"))
                    LoadHouse(openFileDialog1.FileName);



            }

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }





    }
}
