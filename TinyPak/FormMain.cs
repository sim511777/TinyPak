using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

            var dir = new DirectoryInfo(dlg.FileName);                      // 입력 디렉토리
            string pakPath = Util.MakeNewFilePath(dlg.FileName + ".pak");   // 출력 파일

            long size;

            // 인코딩
            var t0 = Util.GetTimeMs();
            using (var sr = new FileStream(pakPath, FileMode.Create)) {
                size = FileSystemEncoder.EncodeDirectory(sr, dir.GetFileSystemInfos());
            }
            var t1 = Util.GetTimeMs();

            // 결과 출력
            Console.WriteLine($"{pakPath} encoded successfully : {size}bytes");
            Console.WriteLine($"encode time : {(t1 - t0):f0}ms");
        }

        private void unpakToolStripMenuItem_Click(object sender, EventArgs e) {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = false;
            dlg.Multiselect = false;
            dlg.Filters.Add(new CommonFileDialogFilter("Tiny Pak Files", "pak"));
            var r = dlg.ShowDialog(this.Handle);
            if (r != CommonFileDialogResult.Ok)
                return;

            var pakPath = dlg.FileName;
            var dir = Path.GetDirectoryName(pakPath);
            var fname = Path.GetFileNameWithoutExtension(pakPath);
            var dirPath = Util.MakeNewDirectoryPath(dir + "\\" + fname);
            Directory.CreateDirectory(dirPath);
            var dinfo = new DirectoryInfo(dirPath);

            // 디코딩
            var t0 = Util.GetTimeMs();
            using (var sr = new FileStream(pakPath, FileMode.Open)) {
                FileSystemEncoder.DecodeDirectory(sr, dinfo);
            }
            var t1 = Util.GetTimeMs();

            // 결과 출력
            Console.WriteLine($"{pakPath} decoded successfully");
            Console.WriteLine($"encode time : {(t1 - t0):f0}ms");
        }
    }
}
