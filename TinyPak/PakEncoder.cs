﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TinyPak {
    public class PakEncoder {
        // e디렉토리 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        public static long EncodeDirectory(Stream sr, FileSystemInfo[] sinfos) {
            var oldPos = sr.Position;
            
            // 항목 개수 기록
            EncodeInt(sr, sinfos.Length);
            
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
                table.nameSize = EncodeString(sr, sinfo.Name);
                table.dataOffset = sr.Position;
                var bFile = !sinfo.Attributes.HasFlag(FileAttributes.Directory);
                if (bFile) {
                    var finfo = sinfo as FileInfo;
                    table.dataSize = EncodeFile(sr, finfo);
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

        public static long EncodeFileSystemInfoTable(Stream sr, FileSystemInfoTable table) {
            EncodeInt(sr, (int)table.attributes);               // ( 0, 4) 파일 속성
            EncodeLong(sr, table.nameOffset);                // (4, 8) 이름 데이터 옵셋
            EncodeLong(sr, table.nameSize);                // (12, 8) 이름 데이터 사이즈
            EncodeLong(sr, table.dataOffset);                // (20, 8) 파일 데이터 옵셋
            EncodeLong(sr, table.dataSize);                // (28, 8) 파일 데이터 사이즈
            return FileSystemInfoTable.StructSize;
        }

        // 파일 데이터 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        // todo: 파일 압축시 이 코드를 수정한다. Chunk 분할 필요
        public static long EncodeFile(Stream sr, FileInfo finfo) {
            var buf = File.ReadAllBytes(finfo.FullName);
            sr.Write(buf, 0, buf.Length);
            return buf.Length;
        }

        // 문자열 데이터 인코딩
        // >= 0 : 스트림에 기록된 바이트 수
        // -1 : 실패
        // todo: 문자열 압축시 이 코드를 수정한다.
        public static long EncodeString(Stream sr, String str) {
            var buf = Encoding.UTF8.GetBytes(str);
            sr.Write(buf, 0, buf.Length);
            return buf.Length;
        }

        public static long EncodeInt(Stream sr, int v) {
            var buf = BitConverter.GetBytes(v);
            sr.Write(buf, 0, buf.Length);
            return Marshal.SizeOf(v.GetType());
        }

        public static long EncodeLong(Stream sr, long v) {
            var buf = BitConverter.GetBytes(v);
            sr.Write(buf, 0, buf.Length);
            return Marshal.SizeOf(v.GetType());
        }

        // ==== decoding
        public static void DecodeDirectory(Stream sr, DirectoryInfo dinfo) {
            var childNum = DecodeInt(sr);
            List<FileSystemInfoTable> tables = new List<FileSystemInfoTable>(childNum);
            for (int i = 0; i < childNum; i++) {
                var table = DecodeFileSystemInfoTable(sr);
                tables.Add(table);
            }
            
            foreach (var table in tables) {
                sr.Position = table.nameOffset;
                var name = DecodeString(sr, table.nameSize);
                bool bFile = !table.attributes.HasFlag(FileAttributes.Directory);
                if (bFile) {
                    var filePath = Util.MakeNewFilePath(dinfo.FullName + "\\" + name);
                    sr.Position = table.dataOffset;
                    var finfo = new FileInfo(filePath);
                    DecodeFile(sr, table.dataSize, finfo);
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
            table.attributes = (FileAttributes)DecodeInt(sr);
            table.nameOffset = DecodeLong(sr);
            table.nameSize = DecodeLong(sr);
            table.dataOffset = DecodeLong(sr);
            table.dataSize = DecodeLong(sr);
            return table;
        }


        public static void DecodeFile(Stream sr, long size, FileInfo finfo) {
            var buf = new byte[size];
            sr.Read(buf, 0, buf.Length);
            File.WriteAllBytes(finfo.FullName, buf);
        }

        public static string DecodeString(Stream sr, long size) {
            var buf = new byte[size];
            sr.Read(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(buf);
        }

        public static int DecodeInt(Stream sr) {
            var buf = new byte[4];
            sr.Read(buf, 0, buf.Length);
            return BitConverter.ToInt32(buf, 0);
        }

        public static long DecodeLong(Stream sr) {
            var buf = new byte[8];
            sr.Read(buf, 0, buf.Length);
            return BitConverter.ToInt64(buf, 0);
        }
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
