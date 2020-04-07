using DiscUtils.Iso9660;
using FileParser.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FileParser
{
    public partial class Form1 : Form
    {
        private Game SimsGame;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            openFileDialog1.Filter = "Sims ps2 Neighbor data (*.ngh)|*.ngh|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SimsGame = new Game("Sims");


                if (openFileDialog1.FileName.Contains(".ngh") ||
                    openFileDialog1.FileName.Contains(".NGH"))
                {

                    ParseNeighborhoodData();
                }
                else
                    MessageBox.Show("Not a valid Neighbor data");


            }
        }

        private void openIsoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            openFileDialog1.Filter = "Sims ps2 Iso (*.iso)|*.iso|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;

            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                try
                {


                    using (FileStream isoStream = File.Open(openFileDialog1.FileName, FileMode.Open))
                    {
                        CDReader cd = new CDReader(isoStream, true);
                        Stream fileStream = cd.OpenFile(@"default.ngh", FileMode.Open);

                        

                        MessageBox.Show("Iso found " + cd.VolumeLabel);

                    }

                }
                catch (Exception ex)
                {
                    ex = new Exception();

                    MessageBox.Show("File is not Sims ps2 iso");


                }

            }


        }

        private void ParseNeighborhoodData()
        {

            string[] Items = new string[3];

            SimsGame.ParseNGH(openFileDialog1.FileName);

            MessageBox.Show("File found: " + SimsGame.DefaultNgh.Name + "," + SimsGame.DefaultNgh.Id);


            if (SimsGame.DefaultNgh.Neighborhood != null && SimsGame.DefaultNgh.Neighborhood.Chunks > 0)
            {

                Items = new string[SimsGame.DefaultNgh.Neighborhood.Chunks];

                Items[0] = SimsGame.DefaultNgh.Neighborhood.Main.ChunkLabel;
                Items[1] = SimsGame.DefaultNgh.Neighborhood.Scores.ChunkLabel;
                //Items[2] = SimsGame.DefaultNgh.Neighborhood.Tables.Name;

                for (int i = 0; i < Items.Length; i++)
                    if (Items[i] != null)
                        listBox1.Items.Add(Items[i]);



            }


        }

    }
}
