using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace TinyPak {
    public partial class FormMain : Form {
        public FormMain() {
            InitializeComponent();
            Console.SetOut(new TextBoxWriter(this.tbxConsole));
        }

        private void pakToolStripMenuItem_Click(object sender, EventArgs e) {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            var r = dlg.ShowDialog(this.Handle);
            if (r != CommonFileDialogResult.Ok)
                return;
            foreach (var file in dlg.FileNames) {
                Console.WriteLine(file);
            }
        }

        private void unpakToolStripMenuItem_Click(object sender, EventArgs e) {

        }
    }
}
