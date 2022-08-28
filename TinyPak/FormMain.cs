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
namespace TinyPak {
    public partial class FormMain : Form {
        public FormMain() {
            InitializeComponent();
        }

        private void pakToolStripMenuItem_Click(object sender, EventArgs e) {
            var dlg = new FolderBrowserDialog();
            var r = dlg.ShowDialog(this);
            if (r != DialogResult.OK)
                return;

            var dir = new DirectoryInfo(dlg.SelectedPath);                      // 입력 디렉토리
            string pakPath = Util.MakeNewFilePath(dlg.SelectedPath + ".pak");   // 출력 파일

            long size = 0;

            // 인코딩
            var t0 = Util.GetTimeMs();
            using (var sr = new FileStream(pakPath, FileMode.Create)) {
                size = PakEncoder.EncodeDirectory(sr, dir.GetFileSystemInfos());
            }
            var t1 = Util.GetTimeMs();

            // 결과 출력
            Log($"{dlg.SelectedPath} -> {pakPath} encoded successfully : {size}bytes");
            Log($"encode time : {(t1 - t0):f0}ms");
        }

        private void unpakToolStripMenuItem_Click(object sender, EventArgs e) {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Tiny Pak Files(*.pak)|*.pak";
            var r = dlg.ShowDialog(this);
            if (r != DialogResult.OK)
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
                PakEncoder.DecodeDirectory(sr, dinfo);
            }
            var t1 = Util.GetTimeMs();

            // 결과 출력
            Log($"{pakPath} -> {dirPath} decoded successfully");
            Log($"encode time : {(t1 - t0):f0}ms");
        }

        private void Log(string msg) {
            this.tbxConsole.AppendText(msg);
            this.tbxConsole.AppendText(Environment.NewLine);
        }
    }
}
