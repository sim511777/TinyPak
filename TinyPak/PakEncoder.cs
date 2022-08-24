using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyPak {
    public class PakEncoder {

        // ==== encoding
        public static long EncodeDirectory(Stream sr, FileSystemInfo[] sinfos) {
            var fileStructure = ReadFileStructure(sinfos);
            var oldPos = sr.Position;
            
            // 항목 개수 기록
            StreamUtil.EncodeInt(sr, sinfos.Length);
            
            // 파일 정보 기록할 공간 확보
            var tablePos = sr.Position;
            for (int i = 0; i < sinfos.Length; i++) {
                sr.Write(FileSystemInfoTable.DummyBuffer, 0, (int)FileSystemInfoTable.StructSize);
            }

            // 이름, 파일시스템 데이터 기록
            var tableDic = new Dictionary<FileSystemInfo, FileSystemInfoTable>();
            foreach (var sinfo in sinfos) {
                var table = new FileSystemInfoTable();
                tableDic[sinfo] = table;
                table.attributes = sinfo.Attributes;
                table.nameOffset = sr.Position;
                table.nameSize = StreamUtil.EncodeString(sr, sinfo.Name);
                table.dataOffset = sr.Position;
                var bFile = !sinfo.Attributes.HasFlag(FileAttributes.Directory);
                if (bFile) {
                    var finfo = sinfo as FileInfo;
                    table.dataSize = StreamUtil.EncodeFile(sr, finfo);
                } else {
                    var dinfo = sinfo as DirectoryInfo;
                    table.dataSize = EncodeDirectory(sr, dinfo.GetFileSystemInfos());
                }
            }
            
            // 파일 정보 기록
            sr.Position = tablePos;     // 스트림 테이블 영역으로 이동
            foreach (var sinfo in sinfos) {
                var size = EncodeFileSystemInfoTable(sr, tableDic[sinfo]);
            }
            sr.Seek(0, SeekOrigin.End); // 스트림 끝으로 이동

            return sr.Position - oldPos;
        }

        private static object ReadFileStructure(FileSystemInfo[] sinfos) {
            throw new NotImplementedException();
        }

        public static long EncodeFileSystemInfoTable(Stream sr, FileSystemInfoTable table) {
            StreamUtil.EncodeInt(sr, (int)table.attributes);               // ( 0, 4) 파일 속성
            StreamUtil.EncodeLong(sr, table.nameOffset);                // (4, 8) 이름 데이터 옵셋
            StreamUtil.EncodeLong(sr, table.nameSize);                // (12, 8) 이름 데이터 사이즈
            StreamUtil.EncodeLong(sr, table.dataOffset);                // (20, 8) 파일 데이터 옵셋
            StreamUtil.EncodeLong(sr, table.dataSize);                // (28, 8) 파일 데이터 사이즈
            return FileSystemInfoTable.StructSize;
        }

        // ==== decoding
        public static void DecodeDirectory(Stream sr, DirectoryInfo dinfo) {
            var childNum = StreamUtil.DecodeInt(sr);
            List<FileSystemInfoTable> tables = new List<FileSystemInfoTable>(childNum);
            for (int i = 0; i < childNum; i++) {
                var table = DecodeFileSystemInfoTable(sr);
                tables.Add(table);
            }
            
            foreach (var table in tables) {
                sr.Position = table.nameOffset;
                var name = StreamUtil.DecodeString(sr, table.nameSize);
                bool bFile = !table.attributes.HasFlag(FileAttributes.Directory);
                if (bFile) {
                    var filePath = Util.MakeNewFilePath(dinfo.FullName + "\\" + name);
                    sr.Position = table.dataOffset;
                    var finfo = new FileInfo(filePath);
                    StreamUtil.DecodeFile(sr, table.dataSize, finfo);
                } else {
                    var dirPath = Util.MakeNewDirectoryPath(dinfo.FullName + "\\" + name);
                    Directory.CreateDirectory(dirPath);
                    var childDirInfo = new DirectoryInfo(dirPath);
                    sr.Position = table.dataOffset;
                    DecodeDirectory(sr, childDirInfo);
                }
            }
        }

        public static FileSystemInfoTable DecodeFileSystemInfoTable(Stream sr) {
            FileSystemInfoTable table = new FileSystemInfoTable();
            table.attributes = (FileAttributes)StreamUtil.DecodeInt(sr);
            table.nameOffset = StreamUtil.DecodeLong(sr);
            table.nameSize = StreamUtil.DecodeLong(sr);
            table.dataOffset = StreamUtil.DecodeLong(sr);
            table.dataSize = StreamUtil.DecodeLong(sr);
            return table;
        }
    }

    public class FileNode {
        public string name;
    }

    public class DirectoryNode {
        public List<FileNode> children;
    }

    public class FileSystemInfoTable {
        public const long StructSize = 36;
        public static readonly byte[] DummyBuffer = new byte[StructSize];

        public FileAttributes attributes;
        public long nameOffset;
        public long nameSize;
        public long dataOffset;
        public long dataSize;
    }
}
