using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TinyPak {
    public partial class FormMain : Form {
        public FormMain() {
            InitializeComponent();
            Console.SetOut(new TextBoxWriter(this.tbxConsole));
        }

        private void pakToolStripMenuItem_Click(object sender, EventArgs e) {
            var r = openFileDialog1.ShowDialog(this);
            if (r != DialogResult.OK)
                return;
            foreach (var file in openFileDialog1.FileNames) {
                Console.WriteLine(file);
            }
        }

        private void unpakToolStripMenuItem_Click(object sender, EventArgs e) {

        }
    }
}
