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
            
            var t0 = Stopwatch.GetTimestamp();
            
            var dir = new DirectoryInfo(dlg.FileName);
            byte[] buffer = EncodeFileSystemInfo(dir);
            
            var t1 = Stopwatch.GetTimestamp();

            var fileName = Path.GetFileNameWithoutExtension(dlg.FileName);
            string pakPath;
            int i = 0;
            do {
                if (i == 0)
                    pakPath = Path.Combine(Path.GetDirectoryName(dlg.FileName), fileName +  ".pak");
                else if (i == 1)
                    pakPath = Path.Combine(Path.GetDirectoryName(dlg.FileName), fileName + " - 복사본.pak");
                else
                    pakPath = Path.Combine(Path.GetDirectoryName(dlg.FileName), fileName + $" - 복사본 ({i}).pak");
                i++;
            } while (File.Exists(pakPath));
            File.WriteAllBytes(pakPath, buffer);
            
            var t2 = Stopwatch.GetTimestamp();
            
            Console.WriteLine($"{pakPath} saved successfully : {buffer.Length}bytes");
            var tPak = (t1 - t0) * 1000.0 / Stopwatch.Frequency;
            var tSave = (t2 - t1) * 1000.0 / Stopwatch.Frequency;
            Console.WriteLine($"pak time : {tPak:f0}ms");
            Console.WriteLine($"save time : {tSave:f0}ms");
        }

        private byte[] EncodeFileSystemInfo(FileSystemInfo sinfo) {
            if (sinfo is FileInfo finfo) {
                return File.ReadAllBytes(finfo.FullName);               // 파일 데이터 리턴
            }

            List<byte> buffer = new List<byte>();
            var dinfo = sinfo as DirectoryInfo;
            var childs = dinfo.GetFileSystemInfos();
            buffer.AddRange(BitConverter.GetBytes(childs.Length));  // 자식 갯수 기록
            var infoOffset = buffer.Count;
            var encodedList = childs.Select(child => Tuple.Create(child, Encoding.UTF8.GetBytes(child.Name), EncodeFileSystemInfo(child)));
            foreach (var child in childs) {
                buffer.AddRange(BitConverter.GetBytes((int)child.Attributes));              // [4] 파일 정보
                buffer.AddRange(BitConverter.GetBytes(child.CreationTimeUtc.Ticks));        // [8] 생성 시간
                buffer.AddRange(BitConverter.GetBytes(child.LastWriteTimeUtc.Ticks));       // [8] 마지막 기록 시간
                buffer.AddRange(BitConverter.GetBytes(child.LastAccessTimeUtc.Ticks));      // [8] 마지막 접속 시간
                buffer.AddRange(BitConverter.GetBytes(0));                                  // [4] 이름 옵셋 들어갈 자리
                buffer.AddRange(BitConverter.GetBytes(0));                                  // [4] 이름 사이즈 들어갈 자리
                buffer.AddRange(BitConverter.GetBytes(0));                                  // [4] 파일 옵셋 들어갈 자리
                buffer.AddRange(BitConverter.GetBytes(0));                                  // [4] 파일 사이즈 들어갈 자리
            }
            int i = 0;
            foreach (var child in childs) {
                var nameEncOffset = buffer.Count;
                var nameEnc = Encoding.UTF8.GetBytes(child.Name);                           // 이름 데이터
                buffer.AddRange(nameEnc);

                var dataEncOffset = buffer.Count;
                var dataEnc = EncodeFileSystemInfo(child);                                  // 파일 데이터
                buffer.AddRange(dataEnc);

                // 이름,데이터 옵셋과 사이즈 쓰기
                int baseIndex = infoOffset + 44 * i;
                int nameOffsetIndex = baseIndex + 28;
                int nameSizeIndex = baseIndex + 32;
                int dataOffsetIndex = baseIndex + 36;
                int dataSizeIndex = baseIndex + 40;

                WriteByteArraryToList(buffer, nameOffsetIndex, BitConverter.GetBytes(nameEncOffset));
                WriteByteArraryToList(buffer, nameSizeIndex, nameEnc);
                WriteByteArraryToList(buffer, dataOffsetIndex, BitConverter.GetBytes(dataEncOffset));
                WriteByteArraryToList(buffer, dataSizeIndex, dataEnc);

                i++;
            }
            return buffer.ToArray();
        }

        public static void WriteByteArraryToList(List<byte> byteList, int index, byte[] byteArray) {
            for (int i = 0; i < byteArray.Length; i++) {
                byteList[index + i] = byteArray[i];
            }
        }

        private void unpakToolStripMenuItem_Click(object sender, EventArgs e) {

        }
    }
}
