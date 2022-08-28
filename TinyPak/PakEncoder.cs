using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyPak {
    public class PakEncoder {
        public static void DecodeDirectory(Stream sr, DirectoryInfo dinfo) {
            PakHeaderInfo header = new PakHeaderInfo();
            ReadHeader(header, sr);
            if (Encoding.ASCII.GetString(header.fileSignature) != "TinyPak")
                throw new Exception("Invalid File Signature");
            var ds = new DirectoryStructure();
            ReadDirectoryStructure(sr, ds);
        }

        private static void ReadDirectoryStructure(Stream sr, DirectoryStructure ds) {
            int fileNum = sr.DecodeInt();
            int 
        }

        private static void ReadHeader(PakHeaderInfo header, Stream sr) {
            sr.Read(header.fileSignature, 0, 8);
            header.fileFormatVerion = (byte)sr.ReadByte();
            sr.Seek(32, SeekOrigin.Current);
        }

        internal static long EncodeDirectory(FileStream sr, FileSystemInfo[] sinfos) {
            return 0;
        }
    }

    internal class DirectoryStructure {
        public string name;
        public List<FileStructure> files;
        public List<DirectoryStructure> directories;
    }

    public class FileStructure {
        public string name;
    }

    internal class PakHeaderInfo {
        public byte[] fileSignature = new byte[7];
        public byte fileFormatVerion;
        public byte[] reserved = new byte[32];
    }
}
