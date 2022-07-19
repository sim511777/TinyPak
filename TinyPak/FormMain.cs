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
            byte[] buffer = PakFileSystemInfo(dir);
        }

        private byte[] PakFileSystemInfo(FileSystemInfo fsi) {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((int)fsi.Attributes));            // 파일 정보
            buffer.AddRange(BitConverter.GetBytes(fsi.CreationTimeUtc.Ticks));      // 생성 시간
            buffer.AddRange(BitConverter.GetBytes(fsi.LastWriteTimeUtc.Ticks));     // 마지막 기록 시간
            buffer.AddRange(BitConverter.GetBytes(fsi.LastAccessTimeUtc.Ticks));    // 마지막 접속 시간
            
            byte[] nameEnc = Encoding.UTF8.GetBytes(fsi.Name);
            buffer.AddRange(BitConverter.GetBytes(nameEnc.Length));                 // 이름 길이 기록
            buffer.AddRange(nameEnc);                                               // 이름 기록

            if (fsi is FileInfo fi) {
                byte[] fileEnc = File.ReadAllBytes(fi.FullName);
                buffer.AddRange(BitConverter.GetBytes(fileEnc.Length));             // 파일 길이 기록
                buffer.AddRange(fileEnc);                                           // 파일 기록
            } else if (fsi is DirectoryInfo di) {
                var childInfos = di.GetFileSystemInfos();
                buffer.AddRange(BitConverter.GetBytes(childInfos.Length));          // 차일드 갯수 기록
                foreach (var fsiChildren in childInfos) {
                    byte[] childEnc = PakFileSystemInfo(fsiChildren);
                    buffer.AddRange(BitConverter.GetBytes(childEnc.Length));        // 차일드 데이터 크기 기록
                    buffer.AddRange(childEnc);                                      // 파일 데이터 기록
                }
            }

            return buffer.ToArray();
        }

        private List<byte> PakFile(FileInfo fileInfo) {
            throw new NotImplementedException();
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

    public class SFileInfo {
        public FileAttributes Attributes;
        public DateTime CreationTimeUtc;
        public DateTime LastAccessTimeUtc;
        public DateTime LastWriteTImeUtc;
        public int NameOffset;
        public int NameSize;
        public int DataOffset;
        public int DataSize;
    }
}
