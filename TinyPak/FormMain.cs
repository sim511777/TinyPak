using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            
            var dir = new DirectoryInfo(dlg.FileName);
            List<byte> buffer = new List<byte>();
            PakDirectory(dir, buffer);
        }

        private void PakDirectory(DirectoryInfo parentDir, List<byte> buffer) {
            var dirs = parentDir.GetDirectories();
            var files = parentDir.GetFiles();

            buffer.AddRange(BitConverter.GetBytes(dirs.Length));    // 폴더 개수 인코딩
            buffer.AddRange(BitConverter.GetBytes(files.Length));   // 파일 개수 인코딩
            foreach (var dir in dirs) {
                PakDirectoryInfo(dir, buffer);  // 폴더 정보 인코딩
            }
            foreach (var file in files) {
                PakFileInfo(file, buffer);      // 파일 정보 인코딩
            }
        }

        private void PakDirectoryInfo(DirectoryInfo dir, List<byte> buffer) {
            throw new NotImplementedException();
        }

        private void PakFileInfo(FileInfo file, List<byte> buffer) {
            throw new NotImplementedException();
        }

        private void unpakToolStripMenuItem_Click(object sender, EventArgs e) {

        }
    }
}
