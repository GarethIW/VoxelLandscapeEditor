using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoxelLandscapeEditor
{
    public partial class SaveForm : Form
    {
        World World;

        public SaveForm(World world)
        {
            World = world;
            InitializeComponent();
        }

        private void SaveForm_Load(object sender, EventArgs e)
        {
            foreach (string n in Enum.GetNames(typeof(MapType))) cboType.Items.Add(n);
            foreach (string n in Enum.GetNames(typeof(Theme))) cboTheme.Items.Add(n);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            World.Type = (MapType)cboType.SelectedIndex;
            World.Theme = (Theme)cboTheme.SelectedIndex;
            World.DisplayName = txtDisplay.Text;
            World.CodeName = txtCode.Text;
            LoadSave.Save(World);
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}
