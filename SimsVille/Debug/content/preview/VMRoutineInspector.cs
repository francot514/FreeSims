using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FSO.SimAntics;

namespace FSO.Debug.content.preview
{
    public partial class VMRoutineInspector : Form
    {
        private VMRoutine Routine;
        public VMRoutineInspector(VMRoutine routine)
        {
            this.Routine = routine;
            
            InitializeComponent();


            this.Text = routine.ID + " " + routine.ToString();
            

        }

        private void VMRoutineInspector_Load(object sender, EventArgs e)
        {
            display.Routine = Routine;
        }
    }
}
